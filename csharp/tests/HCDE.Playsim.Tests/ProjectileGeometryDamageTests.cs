using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileGeometryDamageTests
{
    private static AuthoritySimulation Room(int floor = 0, int ceiling = 128, bool twoSided = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { Index = 1,
            FloorHeight = (short)floor, CeilingHeight = (short)ceiling, HealthFloor = 1000, HealthCeiling = 1000 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1, Index = 1 }],
        Lines = [new LevelLine { X1 = 64, Y1 = 128, X2 = 64, Y2 = -128, SideFront = 0,
            SideBack = twoSided ? 1 : -1, Health = 1000, HealthGroup = 4 }, new LevelLine { Health = 1000, HealthGroup = 4 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.Plasma)]
    [InlineData(ProjectileKind.ImpBall)]
    [InlineData(ProjectileKind.BaronBall)]
    public void SweptWallImpactDamagesGeometryAndGroup(ProjectileKind kind)
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), kind);
        for (var i = 0; i < 12 && !missile.Destroyed; i++) sim.Tick();
        Assert.True(missile.Destroyed);
        var damage = 1000 - sim.Level.Lines[0].Health;
        Assert.InRange(damage, missile.ImpactDamage, missile.ImpactDamage * 8);
        Assert.Equal(0, damage % missile.ImpactDamage);
        Assert.Equal(sim.Level.Lines[0].Health, sim.Level.Lines[1].Health);
    }

    [Theory]
    [InlineData(64, 128, true)]
    [InlineData(0, 20, false)]
    public void WallImpactDamagesOppositeSectorPart(int floor, int ceiling, bool lower)
    {
        var sim = Room(floor, ceiling, true); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        for (var i = 0; i < 8 && !missile.Destroyed; i++) sim.Tick();
        Assert.True(missile.Destroyed);
        Assert.InRange(lower ? sim.Level.Sectors[1].HealthFloor : sim.Level.Sectors[1].HealthCeiling, 960, 995);
        Assert.Equal(1000, lower ? sim.Level.Sectors[1].HealthCeiling : sim.Level.Sectors[1].HealthFloor);
        Assert.InRange(sim.Level.Lines[0].Health, 960, 995);
    }

    [Fact]
    public void ActorBeforeWallPreventsDirectGeometryDamage()
    {
        var sim = Room(); sim.AddBot(40, 0);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        for (var i = 0; i < 8 && !missile.Destroyed; i++) sim.Tick();
        Assert.True(missile.Destroyed);
        Assert.Equal(1000, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void OpenTwoSidedBoundaryDoesNotDamageGeometry()
    {
        var sim = Room(twoSided: true);
        sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(1000, sim.Level.Lines[0].Health);
    }
}
