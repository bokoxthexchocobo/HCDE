using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BotIdentityRestoreTests
{
    [Theory]
    [InlineData(3001, false)]
    [InlineData(3001, true)]
    [InlineData(3005, false)]
    [InlineData(3005, true)]
    public void IdentitySnapshotReconstructsRemovedBotAndPointers(int type, bool remove)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); caller.Brain!.Enabled = false;
        var target = sim.AddBot(200, 0, type, thingId: 77); target.Brain!.Enabled = false; target.Health = 37;
        caller.SetTargetPointer(target); caller.MasterId = caller.TracerId = target.Id;
        var missile = sim.SpawnProjectile(target, ProjectileKind.Plasma); var missileId = missile.Id;
        var id = target.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        Assert.Equal(216, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        target.Destroy(); if (remove) sim.Tick();
        SimSavegame.Apply(sim, bytes);
        var restored = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.False(restored.Destroyed); Assert.Equal(type, restored.DoomEdNum);
        Assert.Equal(77, restored.ThingId); Assert.Equal(37, restored.Health); Assert.False(restored.Brain!.Enabled);
        Assert.Equal(id, caller.TargetId); Assert.Equal(id, caller.MasterId); Assert.Equal(id, caller.TracerId);
        Assert.Same(restored, sim.Actors.OfType<ProjectileActor>().Single(actor => actor.Id == missileId).Owner);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void EmptyIdentityTableRewindsLaterBotMembership()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var extra = sim.AddBot(100, 0); SimSavegame.Apply(sim, bytes);
        Assert.True(extra.Destroyed); Assert.Empty(sim.Actors.OfType<BotPawn>());
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(extra.Id, sim.AddBot(200, 0).Id);
    }

    [Fact]
    public void InvalidIdentityAndConflictingClassAreRejectedBeforeMutation()
    {
        var sim = Room(); var bot = sim.AddBot(100, 0, 3001); bot.Brain!.Enabled = false;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var malformed = bytes.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(malformed.AsSpan(malformed.Length - 28), 0);
        Assert.False(SimSavegame.TryRead(malformed, out _, out var error)); Assert.Equal("save-bot-identity-value", error);
        var state = sim.CaptureState(includeBotIdentities: true, includeThinkerOrder: false);
        state.Actors.Single().BotIdentity = new(3005, 0, true);
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
    }

    [Fact]
    public void ReconstructedEstablishedBotDoesNotReplaySpawnNotification()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0); bot.Brain!.Enabled = false; sim.Tick();
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        bot.Destroy(); sim.Tick(); var notifications = 0; sim.WorldThingSpawned += _ => notifications++;
        SimSavegame.Apply(sim, bytes); sim.Tick();
        Assert.Equal(0, notifications); Assert.False(Assert.Single(sim.Actors.OfType<BotPawn>()).JustSpawned);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void IdentityRestorePreservesBrainPresence(bool present, bool reconstruct)
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001);
        if (present) bot.Brain!.Enabled = false; else bot.Brain = null;
        var id = bot.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        Assert.Equal(present ? 199 : 200, BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 12)));
        if (reconstruct) { bot.Destroy(); sim.Tick(); }
        else bot.Brain = present ? null : new MonsterBrain(MonsterAttack.Hitscan);
        SimSavegame.Apply(sim, bytes);
        var restored = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(present, restored.Brain is not null);
        if (present) Assert.False(restored.Brain!.Enabled);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
    }

    [Fact]
    public void BrainlessReconstructionRestoresStoredTargetAndThreshold()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001); bot.Brain = null;
        var target = sim.AddBot(200, 0, 3001); target.Brain!.Enabled = false;
        bot.SetTargetPointer(target); ActorPropertyActions.SetChaseThreshold(bot, 12, setDefault: true);
        ActorPropertyActions.SetChaseThreshold(bot, 7);
        var id = bot.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        bot.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        var restored = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Null(restored.Brain); Assert.Equal(target.Id, restored.TargetId);
        Assert.Equal(7, restored.ChaseThreshold); Assert.Equal(12, restored.DefaultChaseThreshold);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void UnknownBrainPresenceBitsAreRejected()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0); bot.Brain = null;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 20), 4);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-bot-identity-value", error);
    }

    [Theory]
    [InlineData(ThinkerStat.Info, false)]
    [InlineData(ThinkerStat.Info, true)]
    [InlineData(50, false)]
    [InlineData(50, true)]
    [InlineData(ThinkerStat.Default, false)]
    [InlineData(ThinkerStat.Default, true)]
    public void IdentityRestorePreservesThinkerGroupAndTickEligibility(int stat, bool reconstruct)
    {
        var sim = Room(); var bot = sim.AddBot(0, 0); bot.Brain!.Enabled = false;
        sim.Thinkers.ChangeStatNum(bot, stat); var id = bot.Id;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        Assert.Equal(stat == ThinkerStat.Default ? 199 : 201, BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 12)));
        if (reconstruct) { bot.Destroy(); sim.Tick(); }
        else sim.Thinkers.ChangeStatNum(bot, stat == ThinkerStat.Info ? ThinkerStat.Default : ThinkerStat.Info);
        SimSavegame.Apply(sim, bytes);
        var restored = Assert.IsType<BotPawn>(sim.Actors.Single(actor => actor.Id == id));
        Assert.Equal(stat, restored.StatNum); Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
        var ticks = restored.TickCount; sim.Tick();
        Assert.Equal(ticks + (stat >= ThinkerStat.FirstThinking ? 1 : 0), restored.TickCount);
    }

    [Fact]
    public void RestoringEstablishedBotMovesItOutOfFreshThinkerList()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0); bot.Brain!.Enabled = false; sim.Tick();
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        bot.JustSpawned = true; sim.Thinkers.ChangeStatNum(bot, bot.StatNum);
        SimSavegame.Apply(sim, bytes);
        Assert.Contains(bot, sim.Thinkers.ThinkersIn(ThinkerStat.Default));
        var notifications = 0; sim.WorldThingSpawned += _ => notifications++;
        sim.Tick(); Assert.Equal(0, notifications);
    }

    [Fact]
    public void OutOfRangeThinkerGroupIsRejected()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0); sim.Thinkers.ChangeStatNum(bot, 50);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 20), 128 << 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-bot-identity-value", error);
    }

    [Fact]
    public void IdentitySnapshotReplaysBotAndProjectileIdsAfterAllocatorGap()
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); owner.Brain!.Enabled = false;
        var removed = sim.AddBot(200, 0); removed.Destroy(); sim.Tick();
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var botId = sim.AddBot(300, 0).Id;
        var projectileId = sim.SpawnProjectile(owner, ProjectileKind.Plasma).Id;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(botId, sim.AddBot(300, 0).Id);
        Assert.Equal(projectileId, sim.SpawnProjectile(owner, ProjectileKind.Plasma).Id);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    public void InvalidAllocatorIsRejectedBeforeMutation(uint next)
    {
        var sim = Room(); sim.AddBot(0, 0);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var malformed = bytes.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(malformed.AsSpan(malformed.Length - 8), next);
        Assert.False(SimSavegame.TryRead(malformed, out _, out var error));
        Assert.Equal("save-bot-allocator-state", error);
        var state = sim.CaptureState(includeBotIdentities: true, includeThinkerOrder: false); state.NextActorId = next;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
    }

    [Theory]
    [InlineData(199)]
    [InlineData(200)]
    [InlineData(201)]
    public void OlderIdentityArchivesRemainReadableWithoutAllocator(int version)
    {
        var sim = Room(); var bot = sim.AddBot(0, 0);
        if (version == 200) bot.Brain = null;
        if (version == 201) sim.Thinkers.ChangeStatNum(bot, 50);
        var current = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var legacy = current.AsSpan(0, current.Length - 12).ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)version);
        Assert.True(SimSavegame.TryRead(legacy, out var state, out var error), error);
        Assert.Null(state.NextActorId);
        Assert.Equal(legacy, SimSavegame.Write(state));
    }

    [Theory]
    [InlineData(198, "save-bot-allocator-header")]
    [InlineData(202, "save-bot-allocator-header")]
    public void InvalidAllocatorWrapperVersionIsRejected(int prior, string expectedError)
    {
        var bytes = SimSavegame.Write(Room(), includeBotIdentities: true, includeThinkerOrder: false);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 12), prior);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(expectedError, error);
    }

    [Fact]
    public void DefaultSnapshotsKeepAllocatorMetadataAbsent()
    {
        var sim = Room(); sim.AddBot(0, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Null(state.NextActorId); Assert.False(state.IncludesBotIdentities);
        Assert.Equal(bytes, SimSavegame.Write(state));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LaterPuffIsRemovedBeforeRestoringAllocator(bool destroyed)
    {
        var sim = Room(); var bot = sim.AddBot(0, 0);
        var snapshot = sim.CaptureState(includeBotIdentities: true);
        var puff = sim.SpawnHitscanPuff(100, 0, 0);
        if (destroyed) puff.Destroy();
        sim.RestoreState(snapshot);
        Assert.DoesNotContain(sim.Actors, actor => actor.Id == puff.Id);
        Assert.Equal(puff.Id, sim.SpawnHitscanPuff(100, 0, 0).Id);
    }

    [Theory]
    [InlineData(MonsterAttack.Melee, false)]
    [InlineData(MonsterAttack.Melee, true)]
    [InlineData(MonsterAttack.Hitscan, false)]
    [InlineData(MonsterAttack.Hitscan, true)]
    [InlineData(MonsterAttack.Fireball, false)]
    [InlineData(MonsterAttack.Fireball, true)]
    public void BasicBrainIdentitySurvivesReplacementAndReconstruction(MonsterAttack attack, bool reconstruct)
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001);
        bot.Brain = new MonsterBrain(attack) { Enabled = false };
        var id = bot.Id; var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        Assert.Equal(203, BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 12)));
        if (reconstruct) { bot.Destroy(); sim.Tick(); }
        else bot.Brain = MonsterBrain.ForType(3001);
        SimSavegame.Apply(sim, bytes);
        var restored = sim.Actors.Single(actor => actor.Id == id);
        Assert.Equal(attack, restored.Brain!.BasicAttack); Assert.False(restored.Brain.Enabled);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
    }

    [Fact]
    public void ClassBrainSnapshotReplacesLaterBasicBrain()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0, 3001); bot.Brain!.Enabled = false;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        bot.Brain = new MonsterBrain(MonsterAttack.Melee);
        SimSavegame.Apply(sim, bytes);
        Assert.Null(bot.Brain!.BasicAttack); Assert.Equal(MonsterAttack.Fireball, bot.Brain.Attack);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void BasicBrainCannotBeSavedAsAbsent()
    {
        var sim = Room(); var bot = sim.AddBot(0, 0);
        bot.Brain = new MonsterBrain(MonsterAttack.Melee);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var lifecycle = bytes.AsSpan(bytes.Length - 20);
        BinaryPrimitives.WriteInt32LittleEndian(lifecycle, BinaryPrimitives.ReadInt32LittleEndian(lifecycle) | 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-bot-identity-value", error);
        var state = sim.CaptureState(includeBotIdentities: true, includeThinkerOrder: false);
        state.Actors.Single().BotIdentity = new(3004, 0, BrainPresent: false, BasicAttack: MonsterAttack.Melee);
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public void UndefinedBasicAttackIsRejectedBeforeMutation(int attack)
    {
        var sim = Room(); sim.AddBot(0, 0);
        var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true);
        state.Actors.Single().BotIdentity = new(3004, 0, BasicAttack: (MonsterAttack)attack);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void MixedBrainIdentitiesPreserveThinkerGroupsAndFreshFlags()
    {
        var sim = Room(); var basic = sim.AddBot(0, 0, 3001);
        basic.Brain = new MonsterBrain(MonsterAttack.Fireball) { Enabled = false };
        sim.Thinkers.ChangeStatNum(basic, ThinkerStat.MaxStatNum);
        var standard = sim.AddBot(200, 0); standard.Brain!.Enabled = false;
        var absent = sim.AddBot(400, 0); absent.Brain = null;
        sim.Thinkers.ChangeStatNum(absent, ThinkerStat.Info);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        basic.Destroy(); standard.Destroy(); absent.Destroy(); sim.Tick();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(ThinkerStat.MaxStatNum, sim.Actors.Single(actor => actor.Id == basic.Id).StatNum);
        Assert.Equal(ThinkerStat.Info, sim.Actors.Single(actor => actor.Id == absent.Id).StatNum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
