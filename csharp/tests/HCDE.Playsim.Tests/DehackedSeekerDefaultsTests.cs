using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedSeekerDefaultsTests
{
    [Theory]
    [InlineData("SEEKERMISSILE + 6", true, false)]
    [InlineData("SEEKERMISSILE + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordSeekerDefaults(string bits, bool seeker, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(seeker, actor.SeekerMissile); Assert.Equal(seeker, actor.DefaultSeeker);
        Assert.Equal(seeker, bot.SeekerMissile); Assert.Equal(seeker, bot.DefaultSeeker);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerSeekerMissilePatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = SEEKERMISSILE + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.SeekerMissile); Assert.True(player.DefaultSeeker); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("SEEKERMISSILE + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsSeekerDefault(string bits, bool seeker)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(seeker, missile.SeekerMissile); Assert.Equal(seeker, missile.DefaultSeeker);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool seekerDefault)
    {
        var sim = Room($"Thing 2\nBits = {(seekerDefault ? "SEEKERMISSILE + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.SeekerMissile = !seekerDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(seekerDefault, actor.SeekerMissile); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = SEEKERMISSILE + 6\n"); var actor = sim.Actors.Single();
        actor.SeekerMissile = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(335, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultSeeker = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.SeekerMissile); Assert.True(actor.DefaultSeeker); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.SeekerMissile);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultSeeker = actor.SeekerMissile = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultSeeker); Assert.False(actor.SeekerMissile);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = SEEKERMISSILE + 6\n"); var actor = sim.Actors.Single(); actor.SeekerMissile = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultSeeker = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = SEEKERMISSILE + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultSeeker);
        Assert.False(Start(replaced).Actors.Single().DefaultSeeker);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = SEEKERMISSILE + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-seekerdefaults-value", error);
    }

    [Theory]
    [InlineData(ProjectileKind.Rocket, 34, "SEEKERMISSILE", true)]
    [InlineData(ProjectileKind.RevenantTracer, 7, "PASSMOBJ", false)]
    public void PatchedFlagControlsReflectionAndSurvivesSave(ProjectileKind kind, int thing, string bits, bool seeker)
    {
        var sim = Room($"Thing {thing}\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, kind);
        var reflector = sim.AddBot(200, 0); reflector.Reflective = reflector.ThruReflect = true;
        var saved = SimSavegame.Write(sim, true);
        missile.DefaultSeeker = missile.SeekerMissile = !seeker;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(seeker, missile.DefaultSeeker); Assert.Equal(seeker, missile.SeekerMissile);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        missile.TracerId = null;
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, reflector));
        Assert.Equal(seeker ? owner.Id : (uint?)null, missile.TracerId);
        Assert.Same(reflector, missile.Owner);
    }

    [Fact]
    public void StockRevenantDefaultRequiresNoNewExtensionAndRestoresFromLegacySave()
    {
        var sim = Room(""); var missile = sim.SpawnProjectile(sim.Actors.Single(), ProjectileKind.RevenantTracer);
        var saved = SimSavegame.Write(sim, true);
        Assert.Null(sim.CaptureState().Actors.Single(a => a.Id == missile.Id).SeekerDefaultsFlag);
        Assert.True(BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)) < 335);
        missile.DefaultSeeker = missile.SeekerMissile = false;
        SimSavegame.Apply(sim, saved); Assert.True(missile.DefaultSeeker); Assert.True(missile.SeekerMissile);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
