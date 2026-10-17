using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSoundSaveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IdleActiveSoundUsesChannelGuardAndPersistsFullVolumeFlag(bool fullVolume)
    {
        var sim = Room(); var actor = sim.Players.Single(); var legacy = sim.CaptureState();
        var baseline = sim.Checksum; actor.FullVolumeActive = fullVolume;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Active] = "idle" };
        sim.RestoreState(sim.CaptureState());
        actor = sim.Players.Single(); var expectedChecksum = sim.Checksum;
        if (fullVolume) Assert.NotEqual(baseline, expectedChecksum);
        var bytes = SimSavegame.Write(sim);
        if (fullVolume) Assert.Equal(127, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var saved, out var error), error);
        sim.RestoreState(saved); actor = sim.Players.Single();
        Assert.Equal(fullVolume, actor.FullVolumeActive); Assert.Equal(expectedChecksum, sim.Checksum);
        ActorSoundActions.PlayActiveSound(actor);
        var request = Assert.Single(sim.DrainSoundRequests());
        Assert.True(request.OnlyIfChannelFree); Assert.Equal(2, request.Channel); Assert.Equal(0, request.Flags);
        Assert.Equal(fullVolume ? 0 : 1.001f, request.Attenuation);
        if (fullVolume)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
            bytes[bytes.Length - size + 8] = 2;
            Assert.False(SimSavegame.TryRead(bytes, out _, out error));
            Assert.Equal("save-full-volume-active-invalid", error);
            saved.GeometryHealth = null; Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(saved));
        }
        sim.RestoreState(legacy); Assert.False(sim.Players.Single().FullVolumeActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("custom/push")]
    public void PushSoundActionUsesConfiguredBodySound(string? sound)
    {
        var sim = Room(); var actor = sim.Players.Single();
        if (sound is not null) actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = sound };
        actor.States.Configure(actor, [new ActorFrame(-1, 0, ActorSoundActions.PlayPushSound)], 0);
        if (string.IsNullOrEmpty(sound)) { Assert.Empty(sim.PendingSoundRequests); return; }
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal(sound, request.Sound); Assert.Equal(4, request.Channel);
        Assert.Equal(0, request.Flags);
        Assert.True(request.OnlyIfSoundNotPlaying);
        Assert.Equal(1, request.Volume); Assert.Equal(1, request.Attenuation); Assert.False(request.Loop);
    }

    [Theory]
    [InlineData(null, 5, -1, "*wimpydeath")]
    [InlineData("", 10, -50, "*crazydeath")]
    [InlineData("None", 10, -100, "*xdeath")]
    [InlineData("nOnE", 10, -100, "*gibbed")]
    [InlineData(null, 10, -1, "*death")]
    public void UntypedPlayerDeathTriesNoneSuffixBeforeBase(string? type, int damage, int health, string variant)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.LastDamageType = type;
        sim.Level.SoundDefinitions = new Dictionary<string, string>
        { [variant] = "BASE", [variant + "-None"] = "SPECIFIC" };
        actor.DeathDamageAmount = damage; actor.Health = health;
        var request = sim.DrainSoundRequests().Last();
        Assert.Equal(variant + "-None", request.Sound);
        Assert.Equal(variant == "*gibbed" ? 4 : 2, request.Channel);
    }

    [Fact]
    public void UntypedPlayerDeathFallsBackToBaseButPainDoesNotUseNoneSuffix()
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.LastDamageType = "None";
        sim.Level.SoundDefinitions = new Dictionary<string, string>
        { ["*death"] = "DEATH", ["*pain100"] = "PAIN", ["*pain100-None"] = "TYPED", ["*pain-None"] = "GENERIC" };
        ActorSoundActions.Pain(actor);
        Assert.Equal("*pain100", Assert.Single(sim.DrainSoundRequests()).Sound);
        actor.DeathDamageAmount = 10; actor.Health = -1;
        Assert.Equal("*death", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Theory]
    [InlineData(true, -39, true, "*splat", 4)]
    [InlineData(true, -40, true, "*splat", 4)]
    [InlineData(true, -38.999, true, "*wimpydeath-Fire", 2)]
    [InlineData(false, -40, true, "*wimpydeath-Fire", 2)]
    [InlineData(true, -40, false, "*wimpydeath-Fire", 4)]
    public void FallingPlayerDeathSelectsSplatBeforeTypedWimpySound(bool enabled, double velocity,
        bool splatDefined, string expected, int channel)
    {
        var sim = Room(); sim.FallingDeathSplatEnabled = enabled;
        var actor = sim.Players.Single(); actor.LastDamageType = "Fire";
        var definitions = new Dictionary<string, string>
        { ["*wimpydeath-Fire"] = "SOUND", ["*splat-Fire"] = "SOUND" };
        if (splatDefined) definitions["*splat"] = "SOUND";
        sim.Level.SoundDefinitions = definitions;
        actor.VelocityZ = Fixed.FromDouble(velocity);
        actor.DeathDamageAmount = 5; actor.Health = -50;
        var requests = sim.DrainSoundRequests(); var request = requests.Last();
        Assert.Equal(expected, request.Sound); Assert.Equal(channel, request.Channel);
        Assert.Equal((int)SimulationSoundFlags.NoRumble, request.Flags);
        Assert.Equal(1, request.Attenuation);
        Assert.Equal(channel == 4 ? new[] { 0, 3, 4, 5, 6, 7 } : Array.Empty<int>(),
            requests.Where(r => r.Stop).Select(r => r.Channel).ToArray());
    }

    [Fact]
    public void ConfiguredDeathSoundBypassesFallingSplatAndGateChangesChecksum()
    {
        var sim = Room(); var checksum = sim.Checksum;
        sim.FallingDeathSplatEnabled = true;
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
        var actor = sim.Players.Single();
        sim.Level.SoundDefinitions = new Dictionary<string, string> { ["*splat"] = "SOUND" };
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Death] = "custom/death" };
        actor.VelocityZ = Fixed.FromInt(-40); actor.Health = -100;
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal("custom/death", request.Sound); Assert.Equal(2, request.Channel);
    }

    [Theory]
    [InlineData(5, "*wimpydeath")]
    [InlineData(20, "*crazydeath")]
    public void DelayedDeathSoundPreservesHitAmountThroughSave(int amount, string expected)
    {
        var sim = Room(); var legacy = sim.CaptureState(); var actor = sim.Players.Single();
        sim.Level.SoundDefinitions = new Dictionary<string, string> { ["*wimpydeath"] = "SOUND", ["*crazydeath"] = "SOUND" };
        actor.States.ConfigureSpawn(actor, [new ActorFrame(-1, 0), new ActorFrame(-1, 1),
            new ActorFrame(1, 3), new ActorFrame(-1, 3, ActorSoundActions.PlayerScream)], 0);
        actor.DeathDamageAmount = amount; actor.Health = -50; Assert.Empty(sim.PendingSoundRequests);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var changed = sim.CaptureState(); changed.Actors[0].DeathDamageAmount = amount + 1;
        sim.RestoreState(changed); Assert.NotEqual(checksum, sim.Checksum);
        actor.DeathDamageAmount = amount; sim.RestoreState(sim.CaptureState());
        var bytes = SimSavegame.Write(sim); Assert.Equal(303, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var saved, out var error), error);
        sim.RestoreState(saved); Assert.Equal(checksum, sim.Checksum);
        actor = sim.Players.Single(); Assert.Equal(amount, actor.DeathDamageAmount);
        actor.States.Tick(actor); Assert.Equal(expected, Assert.Single(sim.PendingSoundRequests).Sound);
        saved.GeometryHealth = null; Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(saved));
        sim.RestoreState(legacy); Assert.Equal(0, sim.Players.Single().DeathDamageAmount);
    }

    [Fact]
    public void PlayerConfiguredDeathSoundOverridesVariantsWithNormalAttenuation()
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.Boss = actor.FullVolumeDeath = true;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Death] = "custom/death" };
        actor.Health = -100;
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal("custom/death", request.Sound); Assert.Equal(2, request.Channel); Assert.Equal(1, request.Attenuation);
    }

    [Theory]
    [InlineData(5, -1, "*wimpydeath", "*wimpydeath", 2)]
    [InlineData(10, -50, "*crazydeath", "*crazydeath", 2)]
    [InlineData(10, -100, "*xdeath", "*xdeath", 2)]
    [InlineData(10, -100, "*gibbed", "*gibbed", 4)]
    [InlineData(10, -1, "*death-Fire", "*death-Fire", 2)]
    public void PlayerDeathSoundSelectsRepresentedNativeVariants(int damage, int health, string defined, string expected, int channel)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.LastDamageType = "Fire";
        sim.Level.SoundDefinitions = new Dictionary<string, string> { [defined] = "SOUND" };
        actor.DeathDamageAmount = damage; actor.Health = health;
        var requests = sim.DrainSoundRequests(); var request = requests.Last();
        Assert.Equal(expected, request.Sound); Assert.Equal(channel, request.Channel);
        Assert.Equal((int)SimulationSoundFlags.NoRumble, request.Flags); Assert.Equal(1, request.Attenuation);
        Assert.Equal(channel == 4 ? 6 : 0, requests.Count(r => r.Stop));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void BlockedOrAbsorbedPlayerHitsPreserveDamageMemoryUnlessForced(bool godMode, bool forced)
    {
        var sim = Room(); var player = sim.Players.Single(); player.LastDamageType = "Ice";
        player.GodMode = godMode; player.Inventory.Armor = 100; player.Inventory.MaxFullAbsorb = 100;
        ActorDamage.Apply(player, 10, flags: DamageFlags.NoPain | (forced ? DamageFlags.Forced : 0), damageType: "Fire");
        Assert.Equal(forced ? "Fire" : "Ice", player.LastDamageType);
        Assert.Equal(forced ? 90 : 100, player.Health);
    }

    [Fact]
    public void LastPlayerDamageTypeChangesChecksumAndUntypedDamageClearsIt()
    {
        var sim = Room(); var baseline = sim.Checksum; var state = sim.CaptureState();
        state.Actors[0].PlayerLastDamageType = "Fire";
        sim.RestoreState(state); Assert.NotEqual(baseline, sim.Checksum);
        var fire = sim.Checksum; state.Actors[0].PlayerLastDamageType = "Ice";
        sim.RestoreState(state); Assert.NotEqual(fire, sim.Checksum);
        ActorDamage.Apply(sim.Players.Single(), 1, flags: DamageFlags.NoPain);
        Assert.Null(sim.Players.Single().LastDamageType);
    }

    [Fact]
    public void DelayedPlayerPainRetainsDamageTypeAcrossSaveAndLegacyRestoreClearsIt()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim); var player = sim.Players.Single();
        player.PainChance = 256;
        sim.Level.SoundDefinitions = new Dictionary<string, string> { ["*pain100-Fire"] = "PAIN" };
        player.States.ConfigureSpawn(player, [new ActorFrame(-1, 0), new ActorFrame(1, 2),
            new ActorFrame(-1, 2, ActorSoundActions.Pain)], 0);
        ActorDamage.Apply(player, 1, damageType: "Fire"); Assert.Empty(sim.PendingSoundRequests);
        Assert.Equal("Fire", player.LastDamageType);
        sim.RestoreState(sim.CaptureState()); var expected = sim.Checksum;
        var bytes = SimSavegame.Write(sim); Assert.Equal(159, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var saved, out var error), error); sim.RestoreState(saved);
        player = sim.Players.Single(); Assert.Equal("Fire", player.LastDamageType); Assert.Equal(expected, sim.Checksum);
        player.States.Tick(player); Assert.Equal("*pain100-Fire", Assert.Single(sim.PendingSoundRequests).Sound);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size), 159);
        Assert.False(SimSavegame.TryRead(bytes, out _, out error)); Assert.NotNull(error);
        Assert.True(SimSavegame.TryRead(legacy, out saved, out error), error); sim.RestoreState(saved);
        Assert.Null(sim.Players.Single().LastDamageType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void NonpositiveActorSoundPitchSelectsAutomaticPitch(float pitch)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorSoundActions.StartSound(actor, "sound", pitch: pitch);
        Assert.Null(Assert.Single(sim.PendingSoundRequests).Pitch);
    }

    [Fact]
    public void TentativeSoundEntriesKeepTheirNativeLogicalIdentity()
    {
        var sim = Room(); var actor = sim.Players.Single();
        sim.Level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["first"] = new(IsTentative: true), ["second"] = new(IsTentative: true) };
        sim.Level.SoundAliases = new Dictionary<string, string> { ["alias"] = "FIRST" };
        ActorSoundActions.StartSoundIfNotSame(actor, "alias", "first"); Assert.Empty(sim.PendingSoundRequests);
        ActorSoundActions.StartSoundIfNotSame(actor, "first", "missing"); Assert.Single(sim.DrainSoundRequests());
        ActorSoundActions.StartSoundIfNotSame(actor, "first", "second"); Assert.Single(sim.DrainSoundRequests());
    }

    [Fact]
    public void PlayerPainFindsTentativeDamageVariantBeforeGenericFallback()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 45; player.PainChance = 256;
        sim.Level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["*PAIN50-FIRE"] = new(IsTentative: true) };
        sim.Level.SoundDefinitions = new Dictionary<string, string> { ["*pain-Fire"] = "PAIN" };
        ActorDamage.Apply(player, 1, damageType: "Fire");
        Assert.Equal("*pain50-Fire", Assert.Single(sim.PendingSoundRequests).Sound);
    }

    [Theory]
    [InlineData("missing-a", "missing-b", false)]
    [InlineData("missing-a", "None", false)]
    [InlineData("None", "", false)]
    [InlineData("known", "missing", true)]
    [InlineData("known", "None", true)]
    public void SoundEquivalencePreservesSharedNoSoundIdentity(string sound, string check, bool emits)
    {
        var sim = Room(); var actor = sim.Players.Single();
        sim.Level.SoundDefinitions = new Dictionary<string, string> { ["known"] = "LUMP" };
        actor.States.Configure(actor, [new ActorFrame(-1, 0,
            a => ActorSoundActions.StartSoundIfNotSame(a, sound, check))], 0);
        Assert.Equal(emits ? 1 : 0, sim.PendingSoundRequests.Count);
        if (emits) Assert.Equal(sound, Assert.Single(sim.PendingSoundRequests).Sound);
    }

    [Theory]
    [InlineData("Auto", 0, -1, 3)]
    [InlineData("weapon", 1, 0, 1)]
    [InlineData("Voice", 2, 1, 0)]
    [InlineData("Item", 3, 2, 0)]
    [InlineData("Body", 4, 99, 1)]
    [InlineData("SoundSlot5", 5, 0, 1)]
    [InlineData("SoundSlot6", 6, 0, 1)]
    [InlineData("SoundSlot7", 7, 0, 1)]
    [InlineData("invalid", 0, 0, 1)]
    public void LegacyNamedSoundActionsMapSlotsAndAttenuation(string slot, int channel, int attenuation, float scale)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorSoundActions.PlaySoundEx(actor, "sound", slot, true, attenuation);
        var request = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal(channel, request.Channel); Assert.Equal(scale, request.Attenuation);
        Assert.Equal((int)SimulationSoundFlags.Loop, request.Flags);
        Assert.True(request.OnlyIfSoundNotPlaying);
        ActorSoundActions.StopSoundEx(actor, slot);
        if (channel == 0) Assert.Empty(sim.PendingSoundRequests);
        else { request = Assert.Single(sim.PendingSoundRequests); Assert.True(request.Stop); Assert.Equal(channel, request.Channel); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SoundEquivalenceIgnoresDictionaryComparerAndNameCase(bool random)
    {
        var sim = Room(); var actor = sim.Players.Single();
        sim.Level.SoundAliases = new Dictionary<string, string> { ["chain"] = "ALIAS", ["alias"] = "TARGET" };
        if (random) sim.Level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["target"] = ["child"] };
        else sim.Level.SoundDefinitions = new Dictionary<string, string> { ["target"] = "LUMP" };
        actor.States.Configure(actor, [new ActorFrame(-1, 0,
            a => ActorSoundActions.StartSoundIfNotSame(a, "CHAIN", "target"))], 0);
        Assert.Empty(sim.PendingSoundRequests);
        ActorSoundActions.StartSoundIfNotSame(actor, "chain", "different");
        Assert.Equal("chain", Assert.Single(sim.PendingSoundRequests).Sound);
    }

    [Fact]
    public void StartSoundIfNotSameComparesAliasesWithoutPickingRandomOrComparingResources()
    {
        var sim = Room(); var actor = sim.Players.Single();
        sim.Level.SoundDefinitions = new Dictionary<string, string> { ["a"] = "SOUND", ["b"] = "SOUND" };
        sim.Level.SoundAliases = new Dictionary<string, string> { ["alias"] = "a", ["chain"] = "alias" };
        sim.Level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["random"] = ["a"] };
        ActorSoundActions.StartSoundIfNotSame(actor, "chain", "a");
        ActorSoundActions.StartSoundIfNotSame(actor, "a", "A"); Assert.Empty(sim.PendingSoundRequests);
        ActorSoundActions.StartSoundIfNotSame(actor, "a", "b"); Assert.Single(sim.DrainSoundRequests());
        ActorSoundActions.StartSoundIfNotSame(actor, "random", "a"); Assert.Single(sim.DrainSoundRequests());
        sim.Level.SoundAliases = new Dictionary<string, string> { ["cycle"] = "cycle" };
        Assert.Throws<InvalidOperationException>(() => ActorSoundActions.StartSoundIfNotSame(actor, "cycle", "a"));
        Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void LegacyPlaySoundDefaultsAndPackedFlagsMatchNativeWrapper()
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorSoundActions.PlaySound(actor);
        var request = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("weapons/pistol", request.Sound); Assert.Equal(4, request.Channel); Assert.Equal(0, request.Flags);
        var flags = SimulationSoundFlags.Loop | SimulationSoundFlags.NoStop | SimulationSoundFlags.Local | SimulationSoundFlags.NoPause;
        actor.States.Configure(actor, [new ActorFrame(-1, 0, a => ActorSoundActions.PlaySound(a,
            "custom", 2 | (int)SimulationSoundFlags.NoPause, 0.5f, true, 0.25f, true))], 0);
        request = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal(2, request.Channel); Assert.Equal((int)flags, request.Flags);
        Assert.Equal(0.5f, request.Volume); Assert.Equal(0.25f, request.Attenuation);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorSoundActions.PlaySound(actor, pitch: float.NaN));
        Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void StartSoundStateActionPreservesParametersAndRejectsUnsupportedOffsets()
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorSoundActions.StartSound(actor, "default");
        var request = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal(4, request.Channel); Assert.Equal(0, request.Flags);
        Assert.Equal(1, request.Volume); Assert.Equal(1, request.Attenuation);
        var flags = SimulationSoundFlags.Loop | SimulationSoundFlags.NoStop | SimulationSoundFlags.Local;
        actor.States.Configure(actor, [new ActorFrame(-1, 0,
            a => ActorSoundActions.StartSound(a, "custom", 2, flags, 0.25f, 0.5f))], 0);
        request = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("custom", request.Sound); Assert.Equal(2, request.Channel); Assert.Equal((int)flags, request.Flags);
        Assert.Equal(0.25f, request.Volume); Assert.Equal(0.5f, request.Attenuation);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorSoundActions.StartSound(actor, "sound", pitch: float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorSoundActions.StartSound(actor, "sound", startTime: float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorSoundActions.StartSound(actor, "sound", volume: float.NaN));
        Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void WeaponSoundActionIgnoresEmptyOrDestroyedSources()
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorSoundActions.PlayWeaponSound(actor, ""); Assert.Empty(sim.PendingSoundRequests);
        Assert.Throws<ArgumentNullException>(() => ActorSoundActions.PlayWeaponSound(actor, null!));
        actor.Destroy(); ActorSoundActions.PlayWeaponSound(actor, "weapon"); Assert.Empty(sim.PendingSoundRequests);
    }

    [Theory]
    [InlineData("*pain50-Fire", "*pain50-Fire")]
    [InlineData("*pain-Fire", "*pain-Fire")]
    [InlineData("unrelated", "*pain50")]
    public void DamagePainSelectsDefinedVariantAndClearsTransientContext(string defined, string expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 45; player.PainChance = 256;
        sim.Level.SoundDefinitions = new Dictionary<string, string> { [defined.ToUpperInvariant()] = "PAIN" };
        ActorDamage.Apply(player, 1, damageType: "Fire");
        Assert.Equal(expected, Assert.Single(sim.DrainSoundRequests()).Sound);
        Assert.Null(player.PainDamageTypeContext);
        ActorSoundActions.Pain(player); Assert.Equal(expected, Assert.Single(sim.PendingSoundRequests).Sound);
    }

    [Fact]
    public void PainVariantPrefersHealthBandAliasOverGenericRandomGroup()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 45; player.PainChance = 256;
        sim.Level.SoundAliases = new Dictionary<string, string> { ["*pain50-Fire"] = "pain" };
        sim.Level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["*pain-Fire"] = ["pain"] };
        ActorDamage.Apply(player, 1, damageType: "Fire");
        Assert.Equal("*pain50-Fire", Assert.Single(sim.PendingSoundRequests).Sound);
    }

    [Theory]
    [InlineData(24, "*pain25")]
    [InlineData(25, "*pain50")]
    [InlineData(49, "*pain50")]
    [InlineData(50, "*pain75")]
    [InlineData(74, "*pain75")]
    [InlineData(75, "*pain100")]
    [InlineData(100, "*pain100")]
    public void PlayerPainFrameSelectsNativeHealthBand(int health, string sound)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = health;
        player.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pain] = "monster/pain" };
        player.States.Enter(player, player.PainState);
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal(player.Id, request.ActorId); Assert.Equal(sound, request.Sound);
        Assert.Equal(2, request.Channel); Assert.Equal((int)SimulationSoundFlags.NoRumble, request.Flags);
        Assert.Equal(1, request.Volume); Assert.Equal(1, request.Attenuation); Assert.False(request.Loop);
    }

    [Fact]
    public void DestroyedPlayerDoesNotEmitPainSound()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Destroy();
        ActorSoundActions.Pain(player); Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void StandalonePlayerPainStillEntersThePainState()
    {
        var player = new PlayerPawn { Health = 50 };
        player.States.Enter(player, player.PainState);
        Assert.Equal(player.PainState, player.States.Current);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MonsterAcquisitionEmitsSeeSoundOnceBeforeSeeState(bool hearing)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 128 }],
            Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var actor = sim.Actors.Single(a => a is not PlayerPawn); var player = sim.Players.Single();
        actor.ReactionTime = 0; actor.FullVolumeSee = true;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "alert" };
        actor.SeeState = 1;
        actor.States.ConfigureSpawn(actor, [new ActorFrame(-1, 0), new ActorFrame(-1, 1,
            a => Assert.Single(sim.PendingSoundRequests))], 0);
        if (hearing) actor.Brain!.Hear(sim, actor, player); else actor.Brain!.Tick(sim, actor);
        Assert.Equal(player.Id, actor.Brain!.TargetId); Assert.Equal(1, actor.States.Current);
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal(actor.Id, request.ActorId); Assert.Equal("alert", request.Sound); Assert.Equal(0, request.Attenuation);
        actor.Brain.Tick(sim, actor); actor.Brain.Hear(sim, actor, player);
        Assert.Single(sim.PendingSoundRequests);
    }

    [Fact]
    public void MonsterWithoutSeeStateDoesNotRepeatSightSoundEveryTick()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 128 }] });
        var actor = sim.Actors.Single(a => a is not PlayerPawn); actor.ReactionTime = 0;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "alert" };
        for (var index = 0; index < 3; index++) actor.Brain!.Tick(sim, actor);
        Assert.Single(sim.PendingSoundRequests);
    }

    [Fact]
    public void NoTargetAndDamageRetaliationDoNotEmitLookSound()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] });
        var actor = Assert.Single(sim.Actors); actor.ReactionTime = 0;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "alert" };
        actor.Brain!.Tick(sim, actor); Assert.Empty(sim.PendingSoundRequests);
        var attacker = sim.AddBot(128, 0);
        actor.Brain.WakeOnDamage(actor, attacker, 1, false);
        actor.Brain.Tick(sim, actor); Assert.Empty(sim.PendingSoundRequests);
    }

    [Theory]
    [InlineData(false, false, 1)]
    [InlineData(true, false, 0)]
    [InlineData(false, true, 0)]
    public void SeeSoundUsesNativeVoiceChannelAndAttenuation(bool fullVolume, bool boss, float attenuation)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.FullVolumeSee = fullVolume; actor.Boss = boss;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "see" };
        actor.States.Configure(actor, [new ActorFrame(-1, 0, ActorSoundActions.SeeSound)], 0);
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal("see", request.Sound); Assert.Equal(2, request.Channel); Assert.Equal(0, request.Flags);
        Assert.Equal(attenuation, request.Attenuation); Assert.Equal(1, request.Volume); Assert.False(request.Loop);
    }

    [Fact]
    public void FullVolumeSeeSavePreservesDeathFlagAndRejectsInvalidTrailer()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim); var baseline = sim.Checksum;
        sim.Players.Single().FullVolumeSee = true;
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        sim.Players.Single().FullVolumeDeath = true;
        sim.RestoreState(sim.CaptureState()); var expected = sim.Checksum;
        var bytes = SimSavegame.Write(sim); Assert.Equal(124, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var saved, out var error), error);
        sim.RestoreState(saved); Assert.True(sim.Players.Single().FullVolumeSee);
        Assert.True(sim.Players.Single().FullVolumeDeath); Assert.Equal(expected, sim.Checksum);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        saved.GeometryHealth = null; Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(saved));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        bytes[bytes.Length - size + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out error)); Assert.NotNull(error);
        Assert.True(SimSavegame.TryRead(legacy, out saved, out error), error); sim.RestoreState(saved);
        Assert.False(sim.Players.Single().FullVolumeSee); Assert.False(sim.Players.Single().FullVolumeDeath);
        Assert.Equal(baseline, sim.Checksum);
    }

    [Fact]
    public void FullVolumeSoundsPatchReachesSpawnedDeathMetadataAndSave()
    {
        var patch = HCDE.Gamedata.DehackedPatch.Apply("Thing 2\nBits = FULLVOLSOUNDS\nDeath sound = custom/death\n");
        Assert.Empty(patch.Errors); var definition = patch.Actors.Single(actor => actor.Index == 2);
        var level = new PlayLevel { Things = [new LevelThing { Type = definition.DoomEdNum }],
            Sectors = [new LevelSector { CeilingHeight = 128 }] };
        var sim = AuthoritySimulation.Start(level, dehacked: patch); var actor = Assert.Single(sim.Actors);
        Assert.True(actor.FullVolumeDeath); Assert.True(actor.FullVolumeSee); Assert.False(actor.Boss);
        ActorSoundActions.Scream(actor); Assert.Equal(0, Assert.Single(sim.PendingSoundRequests).Attenuation);
        var before = sim.Checksum; actor.FullVolumeDeath = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(before, sim.Checksum);
        actor = Assert.Single(sim.Actors); actor.FullVolumeDeath = true;
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var saved, out var error), error);
        sim.RestoreState(saved); Assert.True(Assert.Single(sim.Actors).FullVolumeDeath);
        Assert.True(Assert.Single(sim.Actors).FullVolumeSee);
    }

    [Fact]
    public void FullVolumeDeathPersistsAndChangesChecksumAndSoundAttenuation()
    {
        var sim = Room(); var legacy = sim.CaptureState(); var actor = sim.Players.Single(); var baseline = sim.Checksum;
        actor.FullVolumeDeath = true;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Death] = "death" };
        sim.RestoreState(sim.CaptureState()); actor = sim.Players.Single(); Assert.NotEqual(baseline, sim.Checksum);
        var expected = sim.Checksum; var bytes = SimSavegame.Write(sim);
        Assert.Equal(123, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var saved, out var error), error);
        sim.RestoreState(saved); Assert.True(sim.Players.Single().FullVolumeDeath); Assert.Equal(expected, sim.Checksum);
        ActorSoundActions.Scream(sim.Players.Single()); Assert.Equal(0, Assert.Single(sim.PendingSoundRequests).Attenuation);
        sim.RestoreState(legacy); Assert.False(sim.Players.Single().FullVolumeDeath);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4)); bytes[bytes.Length - size + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out error)); Assert.NotNull(error);
    }

    [Theory]
    [InlineData(1, "*gibbed")]
    [InlineData(3004, "misc/gibbed")]
    public void ExtremeScreamSelectsPlayerOrMonsterLogicalSound(int type, string sound)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = type }] });
        var actor = Assert.Single(sim.Actors);
        actor.States.Configure(actor, [new ActorFrame(-1, 0, ActorSoundActions.XScream)], 0);
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal(actor.Id, request.ActorId); Assert.Equal(sound, request.Sound); Assert.Equal(2, request.Channel);
        Assert.Equal((int)SimulationSoundFlags.NoRumble, request.Flags);
        Assert.Equal(1, request.Attenuation); Assert.Equal(1, request.Volume); Assert.False(request.Loop);
    }

    [Theory]
    [InlineData("active", 1)]
    [InlineData("", 0)]
    public void ActiveSoundActionRunsFromTimedState(string sound, int count)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] });
        var actor = Assert.Single(sim.Actors);
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Active] = sound };
        actor.States.ConfigureSpawn(actor, [new ActorFrame(1, 1), new ActorFrame(-1, 1, ActorSoundActions.ActiveSound)], 0);
        actor.States.Tick(actor);
        Assert.Equal(count, sim.PendingSoundRequests.Count);
        if (count == 0) return;
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal(sound, request.Sound); Assert.Equal(2, request.Channel);
        Assert.Equal((int)SimulationSoundFlags.NoRumble, request.Flags);
        Assert.Equal(1, request.Attenuation); Assert.Equal(1, request.Volume);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    public void DefaultDeathFrameEmitsVoiceSoundWithBossAttenuation(bool boss, float attenuation)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] });
        var actor = Assert.Single(sim.Actors); actor.Boss = boss;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Death] = "death" };
        actor.States.Enter(actor, ActorStateMachine.Death);
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal(actor.Id, request.ActorId); Assert.Equal("death", request.Sound); Assert.Equal(2, request.Channel);
        Assert.Equal((int)SimulationSoundFlags.NoRumble, request.Flags); Assert.Equal(attenuation, request.Attenuation);
        Assert.Equal(1, request.Volume); Assert.False(request.Loop);
    }

    [Theory]
    [InlineData("pain", 1)]
    [InlineData("", 0)]
    public void DefaultPainFrameEmitsConfiguredNonPlayerSound(string sound, int count)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] });
        var actor = Assert.Single(sim.Actors);
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pain] = sound };
        actor.States.Enter(actor, ActorStateMachine.Pain);
        Assert.Equal(count, sim.PendingSoundRequests.Count);
        if (count == 0) return;
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal(actor.Id, request.ActorId); Assert.Equal(sound, request.Sound); Assert.Equal(2, request.Channel);
        Assert.Equal(0, request.Flags); Assert.Equal(1, request.Volume); Assert.Equal(1, request.Attenuation);
    }

    [Fact]
    public void NamedDehackedSoundsPopulateSpawnedActorMetadata()
    {
        var patch = HCDE.Gamedata.DehackedPatch.Apply("Thing 2\nAlert sound = custom/see\nAttack sound = custom/attack\nPain sound = custom/pain\nDeath sound = custom/death\nAction sound = custom/active\n");
        Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(actor => actor.Index == 2);
        var level = new PlayLevel { Things = [new LevelThing { Type = definition.DoomEdNum }] };
        var actors = ActorSpawner.Spawn(level, new ThinkerCollection(), patch);
        var actor = Assert.Single(actors);
        Assert.Equal(5, actor.ActorSounds.Count);
        Assert.Equal("custom/see", actor.ActorSounds[ActorSoundType.See]);
        Assert.Equal("custom/active", actor.ActorSounds[ActorSoundType.Active]);
        definition.SoundNameOverrides[0] = "changed";
        Assert.Equal("custom/see", actor.ActorSounds[ActorSoundType.See]);
    }

    [Fact]
    public void ActorSoundsRoundTripAndSnapshotsOwnTheirDictionaries()
    {
        var sim = Room(); var actor = sim.Players.Single();
        var sounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push", [ActorSoundType.See] = "" };
        actor.ActorSounds = sounds;
        var snapshot = sim.CaptureState(); sounds[ActorSoundType.Push] = "changed";
        sim.RestoreState(snapshot); actor = sim.Players.Single(); Assert.Equal("push", actor.ActorSounds[ActorSoundType.Push]);
        var bytes = SimSavegame.Write(sim); Assert.Equal(122, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var saved, out var error), error);
        actor.ActorSounds = new Dictionary<ActorSoundType, string>(); sim.RestoreState(saved);
        Assert.Equal("push", sim.Players.Single().ActorSounds[ActorSoundType.Push]); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void LegacySaveClearsSoundOverrides()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var saved, out var error), error);
        sim.Players.Single().ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "see" };
        sim.RestoreState(saved); Assert.Empty(sim.Players.Single().ActorSounds);
    }

    [Fact]
    public void MalformedSoundTrailerAndInvalidSnapshotAreRejected()
    {
        var sim = Room(); sim.Players.Single().ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "see" };
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 12), 99);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.NotNull(error);
        var snapshot = sim.CaptureState();
        snapshot.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(snapshot));
        snapshot.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [(ActorSoundType)99] = "bad" };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(snapshot));
    }

    [Fact]
    public void SoundChecksumDetectsNameAndSelectorChanges()
    {
        var sim = Room(); var baseline = sim.Checksum;
        var state = sim.CaptureState();
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "see" };
        sim.RestoreState(state); var see = sim.Checksum; Assert.NotEqual(baseline, see);
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "other" };
        sim.RestoreState(state); Assert.NotEqual(see, sim.Checksum);
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pain] = "see" };
        sim.RestoreState(state); Assert.NotEqual(see, sim.Checksum);
    }

    [Fact]
    public void SoundChecksumIgnoresDictionaryInsertionOrderAndSurvivesSaveRestore()
    {
        var sim = Room(); var state = sim.CaptureState();
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string>
        { [ActorSoundType.Push] = "push", [ActorSoundType.See] = "see" };
        sim.RestoreState(state); var expected = sim.Checksum;
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string>
        { [ActorSoundType.See] = "see", [ActorSoundType.Push] = "push" };
        sim.RestoreState(state); Assert.Equal(expected, sim.Checksum);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var saved, out var error), error);
        sim.RestoreState(saved); Assert.Equal(expected, sim.Checksum);
    }

    [Fact]
    public void EmptySoundOverrideIsDistinctFromAbsentAndLegacyRestoreRecoversChecksum()
    {
        var sim = Room(); var baseline = sim.Checksum; var legacy = SimSavegame.Write(sim);
        var state = sim.CaptureState();
        state.Actors[0].ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "" };
        sim.RestoreState(state); Assert.NotEqual(baseline, sim.Checksum);
        Assert.True(SimSavegame.TryRead(legacy, out var saved, out var error), error);
        sim.RestoreState(saved); Assert.Equal(baseline, sim.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
