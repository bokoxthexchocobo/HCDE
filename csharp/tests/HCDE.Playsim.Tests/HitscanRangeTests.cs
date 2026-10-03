using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HitscanRangeTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(45, 35, false)]
    [InlineData(45, 40, true)]
    [InlineData(-45, 120, false)]
    [InlineData(-45, 150, true)]
    public void PitchedPlaneRangeUsesThreeDimensionalDistance(double pitch, double range, bool expected)
    {
        var sim = Room(); var player = CenterOrigin(sim);
        var hit = CombatTrace.TraceLineAttack(sim, player, player.Angle, BamAngle.FromDegrees(pitch), range);
        Assert.Equal(expected, hit.Hit);
        if (expected)
        {
            var distance = Math.Sqrt(hit.X * hit.X + hit.Y * hit.Y + Math.Pow(hit.Z - 28, 2));
            Assert.True(distance <= range);
            Assert.Equal(pitch > 0 ? 0 : 1, hit.PlanePart);
        }
    }

    [Theory]
    [InlineData(90, 27, false)]
    [InlineData(90, 28, false)]
    [InlineData(90, 28.0000152587890625, true)]
    [InlineData(-90, 99, false)]
    [InlineData(-90, 100, false)]
    [InlineData(-90, 100.0000152587890625, true)]
    public void VerticalShotsUseExactPlaneDistanceWithoutHorizontalDrift(double pitch, double range, bool expected)
    {
        var sim = Room(); var player = CenterOrigin(sim);
        var hit = CombatTrace.TraceLineAttack(sim, player, player.Angle, BamAngle.FromDegrees(pitch), range);
        Assert.Equal(expected, hit.Hit);
        if (expected) { Assert.Equal(0, hit.X); Assert.Equal(0, hit.Y); }
    }

    [Theory]
    [InlineData(50, false)]
    [InlineData(60, true)]
    public void VerticalActorCylinderIntersectionRespectsRange(double range, bool expected)
    {
        var sim = Room(); var player = CenterOrigin(sim); var target = sim.AddBot(0, 0);
        target.Z = Fixed.FromInt(80);
        var hit = CombatTrace.TraceLineAttack(sim, player, player.Angle, BamAngle.FromDegrees(-90), range);
        Assert.Equal(expected, hit.Hit);
        if (expected) { Assert.Same(target, hit.Victim); Assert.Equal(80, hit.Z); }
    }
    // These geometry fixtures intentionally trace from the actor center.
    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
