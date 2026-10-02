using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TraceParallelWallTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0.5, 0)]
    [InlineData(-1, 0)]
    public void ParallelAndStationaryHorizontalRaysDoNotCrossCollinearWall(double dx, double dy)
    {
        var wall = new LevelLine { X1 = 20, Y1 = 0, X2 = 40, Y2 = 0 };
        Assert.True(double.IsPositiveInfinity(CombatTrace.RayLine(0, 0, dx, dy, wall)));
    }

    [Fact]
    public void ParallelOffsetWallDoesNotCross()
    {
        Assert.True(double.IsPositiveInfinity(CombatTrace.RayLine(0, 0, 1, 0,
            new LevelLine { X1 = 20, Y1 = 1, X2 = 40, Y2 = 1 })));
    }

    [Theory]
    [InlineData(1, 20)]
    [InlineData(0.5, 40)]
    public void ActualCrossingRetainsRayParameter(double dx, double expected)
    {
        Assert.Equal(expected, CombatTrace.RayLine(0, 0, dx, 0,
            new LevelLine { X1 = 20, Y1 = -10, X2 = 20, Y2 = 10 }));
    }

    [Theory]
    [InlineData(90, 0)]
    [InlineData(-90, 1)]
    public void VerticalAttackHitsPlaneInsideWalledRoom(double pitch, int part)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 100, HealthCeiling = 100 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [
                Wall(-128, -128, -128, 128), Wall(-128, 128, 128, 128),
                Wall(128, 128, 128, -128), Wall(128, -128, -128, -128)],
            Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single();
        var hit = CombatTrace.TraceLineAttack(sim, source, new BamAngle(0), BamAngle.FromDegrees(pitch), 256);
        Assert.True(hit.Hit); Assert.Null(hit.Wall);
        Assert.Equal(0, hit.PlaneSector); Assert.Equal(part, hit.PlanePart);
        GeometryLineAttack.Apply(sim, hit, 7);
        Assert.Equal(part == 0 ? 93 : 100, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(part == 1 ? 93 : 100, sim.Level.Sectors[0].HealthCeiling);
        Assert.All(sim.Level.Lines, line => Assert.Equal(100, line.Health));
    }

    private static LevelLine Wall(double x1, double y1, double x2, double y2) => new()
    { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, SideFront = 0, SideBack = -1, Health = 100 };
}
