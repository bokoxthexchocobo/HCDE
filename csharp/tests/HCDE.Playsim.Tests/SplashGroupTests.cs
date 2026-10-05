using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SplashGroupTests
{
    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(7, 7, true)]
    [InlineData(7, 8, false)]
    public void SplashUsesExplosionGroupNotOwner(int targetGroup, int explosionGroup, bool immune)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] },
            dehacked: DehackedPatch.Apply($"Thing 34\nSplash group = {explosionGroup}\n"));
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null; owner.SplashGroup = targetGroup;
        var direct = sim.AddBot(30, 0, 3001); direct.Brain = null; direct.Health = 1000; direct.SplashGroup = explosionGroup;
        var nearby = sim.AddBot(30, 60, 3001); nearby.Brain = null; nearby.Health = 1000; nearby.SplashGroup = targetGroup;
        var rocket = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(explosionGroup, rocket.SplashGroup);
        rocket.X = rocket.Y = default; rocket.Z = Fixed.FromInt(20);
        rocket.VelocityX = Fixed.FromInt(30); rocket.VelocityY = rocket.VelocityZ = default;
        rocket.Tick();
        Assert.True(rocket.Destroyed);
        Assert.True(direct.Health < 1000);
        Assert.Equal(immune, nearby.Health == 1000);
    }

    [Theory]
    [InlineData(9, 9)]
    [InlineData(-9, 0)]
    public void DehackedSplashDefaultsReachSpawnedActor(int value, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }],
        }, dehacked: DehackedPatch.Apply($"Thing 2\nSplash group = {value}\n"));
        Assert.Equal(expected, Assert.Single(sim.Actors).SplashGroup);
    }
}
