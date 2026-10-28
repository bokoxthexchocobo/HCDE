using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapRadiusThrustTests
{
    [Fact]
    public void ExplosionAppliesHorizontalThrustToNearbyActor()
    {
        var sim = Room();
        var source = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var victim = sim.Players.Single();
        victim.X = Fixed.FromDouble(source.X.ToDouble() + 48);
        victim.Mass = 100;
        var vx = victim.VelocityX.ToDouble();
        MapRadiusAttack.Apply(sim, source, source, 128, 128, "Unknown", hurtSource: true);
        Assert.True(victim.VelocityX.ToDouble() > vx);
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
            ],
        });
    }
}
