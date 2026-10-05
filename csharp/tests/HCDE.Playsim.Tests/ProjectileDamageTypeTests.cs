using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileDamageTypeTests
{
    [Theory]
    [InlineData("fire", true)]
    [InlineData("Ice", false)]
    public void DirectHitPassesProjectileTypeToTypedOnlyTarget(string type, bool killed)
    {
        var (sim, missile, target) = Setup(ProjectileKind.Plasma);
        target.Health = 1;
        target.DeathState = -1;
        target.SetTypedDeath("Fire", 3);
        missile.DamageType = type;
        missile.Tick();
        Assert.True(missile.Destroyed);
        Assert.Equal(killed, target.IsDead);
        if (killed) Assert.Equal(3, target.States.Current);
    }

    [Fact]
    public void SplashPassesProjectileTypeToNearbyTypedOnlyTarget()
    {
        var (sim, missile, target) = Setup(ProjectileKind.Rocket);
        var nearby = sim.AddBot(30, 60, 3001);
        nearby.Brain = null;
        nearby.Health = 1;
        nearby.DeathState = -1;
        nearby.SetTypedDeath("Fire", 3);
        missile.DamageType = "Fire";
        missile.Tick();
        Assert.True(missile.Destroyed);
        Assert.True(nearby.IsDead);
        Assert.Equal(3, nearby.States.Current);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup(ProjectileKind kind)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004);
        var target = sim.AddBot(30, 0, 3001);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, kind);
        missile.X = missile.Y = default;
        missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30);
        missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
