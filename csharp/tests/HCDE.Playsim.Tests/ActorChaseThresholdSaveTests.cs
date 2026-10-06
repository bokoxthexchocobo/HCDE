using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorChaseThresholdSaveTests
{
    [Theory]
    [InlineData(0, 100)]
    [InlineData(7, 25)]
    [InlineData(int.MaxValue, 0)]
    public void CurrentAndDefaultRestoreWithExactBytes(int current, int defaultThreshold)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var neighbor = sim.AddBot(100, 0);
        ActorPropertyActions.SetChaseThreshold(actor, current);
        ActorPropertyActions.SetChaseThreshold(actor, defaultThreshold, true);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.SetChaseThreshold(actor, 5);
        ActorPropertyActions.SetChaseThreshold(actor, 9, true);
        ActorPropertyActions.SetChaseThreshold(neighbor, 10);
        sim.RestoreState(state);
        Assert.Equal(current, actor.Brain!.Threshold); Assert.Equal(defaultThreshold, actor.Brain.DefThreshold);
        Assert.Equal(0, neighbor.Brain!.Threshold); Assert.Equal(100, neighbor.Brain.DefThreshold);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void DamageWakeupValuesAreCapturedWithoutActionTracking()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.Brain!.WakeOnDamage(actor, sim.Players.Single(), 1, false);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.SetChaseThreshold(actor, 0); sim.RestoreState(state);
        Assert.Equal(100, actor.Brain.Threshold);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 71)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    public void MalformedTrailerIsRejected(int offset, int value)
    {
        var sim = Room(); ActorPropertyActions.SetChaseThreshold(sim.AddBot(0, 0), 5);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.StartsWith("save-chase-threshold-", error);
    }

    [Fact]
    public void LegacyWithoutThresholdRecordsRestoresDefaults()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.All(state.Actors, pose => Assert.Null(pose.ChaseThreshold));
        ActorPropertyActions.SetChaseThreshold(actor, 7);
        sim.RestoreState(state);
        Assert.Equal(0, actor.Brain!.Threshold);
    }

    [Fact]
    public void BaselineRestoreClearsLaterStickinessAndRestoresDamageWakeupDefault()
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0);
        var current = sim.AddBot(100, 0, 3001, thingId: 7); var attacker = sim.AddBot(200, 0, 3002);
        hunter.Brain!.SetTargetThingId(sim, 7);
        var bytes = SimSavegame.Write(sim);
        Assert.All(sim.CaptureState().Actors, pose => Assert.Null(pose.ChaseThreshold));
        ActorPropertyActions.SetChaseThreshold(hunter, 7);
        ActorPropertyActions.SetChaseThreshold(hunter, 25, true);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(current.Id, hunter.Brain.TargetId);
        Assert.Equal(0, hunter.Brain.Threshold); Assert.Equal(100, hunter.Brain.DefThreshold);
        Assert.False(hunter.Brain.HasChaseThresholdOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        ActorDamage.Apply(hunter, 1, attacker, DamageFlags.NoPain);
        Assert.Equal(attacker.Id, hunter.Brain.TargetId); Assert.Equal(100, hunter.Brain.Threshold);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
