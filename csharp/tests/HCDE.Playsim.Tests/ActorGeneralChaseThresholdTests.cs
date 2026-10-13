using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGeneralChaseThresholdTests
{
    [Theory]
    [InlineData(0, -1, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(2, -1, 0)]
    [InlineData(0, int.MaxValue, int.MaxValue)]
    [InlineData(1, int.MaxValue, int.MaxValue)]
    [InlineData(2, int.MaxValue, int.MaxValue)]
    public void GeneralThresholdsClampAndRemainIndependent(int kind, int value, int expected)
    {
        var sim = Room(); var actor = Select(sim, kind);
        ActorPropertyActions.SetChaseThreshold(actor, value);
        Assert.Equal(expected, actor.ChaseThreshold); Assert.Equal(100, actor.DefaultChaseThreshold);
        ActorPropertyActions.SetChaseThreshold(actor, value, true);
        Assert.Equal(expected, actor.ChaseThreshold); Assert.Equal(expected, actor.DefaultChaseThreshold);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ThresholdsRoundTripAndLegacyRestoreResetsValuesAndChecksum(int kind)
    {
        var sim = Room(); var actor = Select(sim, kind);
        var baseline = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, baseline); var hash = sim.Checksum;
        ActorPropertyActions.SetChaseThreshold(actor, 7);
        ActorPropertyActions.SetChaseThreshold(actor, 25, true);
        var bytes = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes);
        Assert.NotEqual(hash, sim.Checksum);
        Assert.Equal(7, actor.ChaseThreshold); Assert.Equal(25, actor.DefaultChaseThreshold);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, baseline);
        Assert.Equal(0, actor.ChaseThreshold); Assert.Equal(100, actor.DefaultChaseThreshold);
        Assert.Equal(hash, sim.Checksum); Assert.Equal(baseline, SimSavegame.Write(sim));
    }

    [Fact]
    public void PointerSelectionChangesOnlySelectedGeneralActor()
    {
        var sim = Room(); var caller = sim.Players.Single(); var missile = Select(sim, 2);
        caller.TracerId = missile.Id;
        ActorPropertyActions.SetChaseThreshold(caller, 42, pointerSelector: AcsActorPointer.Tracer);
        Assert.Equal(42, missile.ChaseThreshold); Assert.Equal(0, caller.ChaseThreshold);
        ActorPropertyActions.SetChaseThreshold(caller, 8, pointerSelector: AcsActorPointer.Null);
        Assert.Equal(0, caller.ChaseThreshold);
    }

    private static Actor Select(AuthoritySimulation sim, int kind)
    {
        if (kind == 0) return sim.Players.Single();
        if (kind == 2) return sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        var actor = sim.AddBot(100, 0); actor.Brain = null; return actor;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
