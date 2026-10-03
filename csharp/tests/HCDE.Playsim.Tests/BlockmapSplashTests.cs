using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlockmapSplashTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RocketWallSplashSkipsExcludedActors(bool excluded)
    {
        var sim = Room(wall: true); var target = Assert.Single(sim.Actors);
        target.NoBlockmap = excluded;
        var owner = new Actor { Health = 100, X = Fixed.FromInt(-64) };
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.True(missile.Destroyed);
        Assert.Equal(excluded, target.Health == 1000);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArchvileExclusionAppliesToSplashButNotDirectDamage(bool excluded)
    {
        var sim = Room(wall: false); var target = Assert.Single(sim.Actors);
        target.NoBlockmap = excluded;
        var archvile = new Actor { Health = 100, X = Fixed.FromInt(-200) };
        ArchvileActions.Attack(sim, archvile, target, fireExists: true);
        Assert.Equal(excluded, target.Health == 980);
        Assert.True(target.Health <= 980);
        Assert.True(target.VelocityZ.Raw > 0);
    }

    private static AuthoritySimulation Room(bool wall)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = wall ? [new LevelLine { X1 = 50, Y1 = -128, X2 = 50, Y2 = 128, SideFront = 0, SideBack = -1 }] : [],
            Things = [new LevelThing { Type = 3004, X = 20, Y = 40 }],
        });
        var actor = Assert.Single(sim.Actors); actor.Health = 1000; actor.Brain!.Enabled = false;
        return sim;
    }
}
