using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlaneWallImpactOrderingTests
{
    [Theory]
    [InlineData(45, -1e-10, false)]
    [InlineData(45, 1e-10, true)]
    [InlineData(45, 0, true)]
    [InlineData(-45, -1e-10, false)]
    [InlineData(-45, 1e-10, true)]
    [InlineData(-45, 0, false)]
    public void ClosePlaneAndWallIntersectionsUseActualDistance(double pitch, double wallOffset, bool planeFirst)
    {
        var radians = pitch * Math.PI / 180;
        var planeZ = pitch > 0 ? 0 : 128;
        var distance = (planeZ - 28) / -Math.Sin(radians);
        var wallX = distance * Math.Cos(radians) + wallOffset;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 100, HealthCeiling = 100 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [Wall(-128, -128, -128, 128), Wall(-128, 128, wallX, 128),
                Wall(wallX, 128, wallX, -128), Wall(wallX, -128, -128, -128)],
            Things = [new LevelThing { Type = 1 }],
        });
        var hit = CombatTrace.TraceLineAttack(sim, CenterOrigin(sim), new BamAngle(0), BamAngle.FromDegrees(pitch), 256);
        Assert.True(hit.Hit);
        Assert.Equal(planeFirst, hit.PlaneSector == 0);
        if (planeFirst) Assert.Null(hit.Wall); else Assert.Same(sim.Level.Lines[2], hit.Wall);
        GeometryLineAttack.Apply(sim, hit, 7);
        Assert.Equal(planeFirst ? 100 : 93, sim.Level.Lines[2].Health);
        Assert.Equal(planeFirst && pitch > 0 ? 93 : 100, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(planeFirst && pitch < 0 ? 93 : 100, sim.Level.Sectors[0].HealthCeiling);
    }

    private static LevelLine Wall(double x1, double y1, double x2, double y2) => new()
    { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, SideFront = 0, SideBack = -1, Health = 100 };
    // These geometry fixtures intentionally trace from the actor center.
    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
