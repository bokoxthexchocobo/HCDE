using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTargetMemorySaveTests
{
    [Fact]
    public void BaselineOnlySaveClearsLaterCombatMemoriesAndStopsPursuit()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var hunter = sim.AddBot(0, 0); var enemy = sim.AddBot(100, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.All(sim.CaptureState().Actors, pose => Assert.Null(pose.TargetMemory));
        hunter.Brain!.RestoreTargetMemory(new(enemy.Id, enemy.Id, enemy.Id));
        hunter.LastHeardTargetId = enemy.Id;
        SimSavegame.Apply(sim, bytes);
        Assert.Null(hunter.Brain.TargetId); Assert.Null(hunter.Brain.LastEnemyId);
        Assert.Null(hunter.LastHeardTargetId); Assert.False(hunter.HasTargetMemoryOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        hunter.Brain.Tick(sim, hunter);
        Assert.Null(hunter.Brain.TargetId); Assert.Null(hunter.Brain.LastEnemyId);
        Assert.Equal(MonsterMode.Idle, hunter.Brain.Mode);
    }

    [Fact]
    public void BaselineOnlySaveClearsLastHeardWithoutBrain()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var bytes = SimSavegame.Write(sim);
        actor.LastHeardTargetId = sim.Players.Single().Id;
        SimSavegame.Apply(sim, bytes);
        Assert.Null(actor.LastHeardTargetId); Assert.False(actor.HasTargetMemoryOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ThreeMemoriesRoundTripAndPreserveBytes()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 64); var enemy = sim.AddBot(20, 0, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7); caller.Brain.WakeOnDamage(caller, sim.Players.Single(), 1, false);
        caller.LastHeardTargetId = enemy.Id;
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.ClearTarget(caller); sim.RestoreState(state);
        Assert.Equal(sim.Players.Single().Id, caller.Brain.TargetId);
        Assert.Equal(enemy.Id, caller.Brain.LastEnemyId); Assert.Equal(enemy.Id, caller.LastHeardTargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ExplicitClearsAreRestoredInsteadOfKeepingRuntimeTargets()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var enemy = sim.AddBot(20, 0);
        ActorPropertyActions.ClearTarget(caller); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        caller.Brain!.SetSpecialTarget(enemy); caller.LastHeardTargetId = enemy.Id;
        sim.RestoreState(state); Assert.Null(caller.Brain.TargetId); Assert.Null(caller.LastHeardTargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 69)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var sim = Room(); ActorPropertyActions.ClearTarget(sim.AddBot(0, 0)); var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    [Fact]
    public void EmptyActorMemoryBesideRecordedActorAlsoRestoresEmpty()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var other = sim.AddBot(20, 0);
        caller.Brain!.SetSpecialTarget(sim.Players.Single()); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        other.Brain!.SetSpecialTarget(caller); other.LastHeardTargetId = caller.Id;
        sim.RestoreState(state); Assert.Null(other.Brain.TargetId); Assert.Null(other.LastHeardTargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
