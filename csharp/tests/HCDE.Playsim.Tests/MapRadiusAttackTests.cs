using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapRadiusAttackTests
{
    [Fact]
    public void OldRadiusUsesChebyshevDistance()
    {
        var sim = Room();
        var source = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var victim = sim.Players.Single();
        victim.X = Fixed.FromDouble(source.X.ToDouble() + 48);
        source.OldRadiusDamage = true;
        var damage = MapRadiusAttack.GetRadiusDamage(source, victim, 128, 128, 0, oldRadius: true, circular: false);
        Assert.True(damage > 0);
    }

    [Fact]
    public void RadiusDamageFactorReducesSplash()
    {
        var sim = Room();
        var source = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var victim = sim.Players.Single();
        victim.X = Fixed.FromDouble(source.X.ToDouble() + 32);
        victim.RadiusDamageFactor = 0.5;
        var full = MapRadiusAttack.GetRadiusDamage(source, victim, 100, 128, 0, oldRadius: false, circular: false,
            fromAction: false, splashFactor: 1);
        Assert.True(full > 0);
        Assert.InRange(full, 20, 60);
    }

    [Fact]
    public void ApplyRespectsSplashImmuneGroup()
    {
        var sim = Room();
        var source = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var victim = sim.Actors.Single(a => a.DoomEdNum == 3003);
        source.SplashGroup = 9;
        victim.SplashGroup = 9;
        victim.X = Fixed.FromDouble(source.X.ToDouble() + 32);
        var health = victim.Health;
        MapRadiusAttack.Apply(sim, source, source, 80, 160, "Unknown");
        Assert.Equal(health, victim.Health);
    }

    private static AuthoritySimulation Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors = [new LevelSector { CeilingHeight = 128, FloorHeight = 0 }],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things =
            [
                new LevelThing { Type = 1, X = 0, Y = 0 },
                new LevelThing { Type = 3001, X = 64, Y = 0 },
                new LevelThing { Type = 3003, X = 128, Y = 0 },
            ],
        });
    }
}
