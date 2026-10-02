using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickActorRangeTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(-90, 50, false)]
    [InlineData(-90, 60, true)]
    [InlineData(-45, 70, false)]
    [InlineData(-45, 80, true)]
    public void PickActorUsesThreeDimensionalRange(double pitch, double range, bool expected)
    {
        var sim = Room(); var player = sim.Players.Single();
        var target = sim.AddBot(pitch == -90 ? 0 : 52, 0); target.Z = Fixed.FromInt(80);
        var health = target.Health;
        var picked = CombatTrace.PickActor(sim, player, player.Angle, BamAngle.FromDegrees(pitch), range);
        if (expected) Assert.Same(target, picked); else Assert.Null(picked);
        Assert.Equal(health, target.Health);
    }

    [Fact]
    public void SuccessfulActorTraversalPrecedesOrdinaryFloorFallback()
    {
        var sim = Room(); var player = sim.Players.Single(); var target = sim.AddBot(0, 0);
        target.Z = Fixed.FromInt(-100);
        Assert.Same(target, CombatTrace.PickActor(sim, player, player.Angle, BamAngle.FromDegrees(90), 200));
    }

    [Fact]
    public void PickingRemainsReadOnlyForDestructiblePlanes()
    {
        var sim = Room(); var player = sim.Players.Single(); sim.Level.Sectors[0].HealthFloor = 100;
        var random = sim.CombatRandomState;
        Assert.Null(CombatTrace.PickActor(sim, player, player.Angle, BamAngle.FromDegrees(90), 200));
        Assert.Equal(100, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(random, sim.CombatRandomState);
    }
}
