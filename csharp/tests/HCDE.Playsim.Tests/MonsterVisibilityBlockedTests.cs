using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterVisibilityBlockedTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void HateeVisibilityBlockRespectsNoSightCheck(bool local, bool noSight)
    {
        var sim = Room();
        if (local) sim.Level.Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []);
        var hunter = sim.AddBot(40, 40); var target = sim.AddBot(80, 40, 3001);
        hunter.TidToHate = target.ThingId = 7; hunter.NoSightCheck = noSight; target.MVisBlocked = true;
        Assert.True(CombatTrace.HasLineOfSight(sim, hunter, target));
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(noSight ? target.Id : (uint?)null, hunter.TargetId);
    }

    [Fact]
    public void PlayerIsHiddenFromLookButExistingTargetIsRetained()
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0); var player = sim.Players.Single();
        player.MVisBlocked = true; hunter.Brain!.Tick(sim, hunter); Assert.Null(hunter.TargetId);
        player.MVisBlocked = false; hunter.Brain.Tick(sim, hunter); Assert.Equal(player.Id, hunter.TargetId);
        player.MVisBlocked = true; hunter.Brain.Tick(sim, hunter); Assert.Equal(player.Id, hunter.TargetId);
    }

    [Fact]
    public void FlagRoundTripsAndOlderSnapshotClearsIt()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var old = SimSavegame.Write(sim, true);
        actor.MVisBlocked = true; var saved = SimSavegame.Write(sim, true);
        actor.MVisBlocked = false; SimSavegame.Apply(sim, saved);
        Assert.True(actor.MVisBlocked); Assert.Equal(saved, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, old); Assert.False(actor.MVisBlocked);
    }

    [Theory]
    [InlineData("MVISBLOCKED")]
    [InlineData("Actor.MVisBlocked")]
    public void ScriptFlagAndReviveReset(string flag)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, flag, true));
        Assert.True(AcsActorFlags.TryGet(actor, flag, out var value)); Assert.True(value);
        ActorRaise.ReviveSupported(actor); Assert.False(actor.MVisBlocked);
    }

    [Fact]
    public void InvalidStructuredFlagIsRejected()
    {
        var sim = Room(); var saved = sim.CaptureState(); saved.Actors[0].MVisBlockedFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(saved));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(saved));
    }

    [Fact]
    public void RecursiveArchiveIsRejected()
    {
        var sim = Room(); sim.Players.Single().MVisBlocked = true; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size), 233);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-mvisblocked-header", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
