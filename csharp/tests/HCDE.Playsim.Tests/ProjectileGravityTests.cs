using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileGravityTests
{
    [Theory]
    [InlineData(1, 1, false, -1)]
    [InlineData(0.5, 0.25, false, -0.125)]
    [InlineData(0, 1, false, 0)]
    [InlineData(1, 0, false, 0)]
    [InlineData(-1, 1, false, 1)]
    [InlineData(1, 1, true, 0)]
    public void GravityChangesNextTicVelocity(double actorGravity, double sectorGravity, bool noGravity, double acceleration)
    {
        var sim = Room(); var missile = Shoot(sim);
        missile.Gravity = Fixed.FromDouble(actorGravity); missile.NoGravity = noGravity;
        sim.Level.Sectors[0].Gravity = sectorGravity;
        sim.Tick();
        Assert.Equal(64, missile.Z.ToDouble()); Assert.Equal(acceleration, missile.VelocityZ.ToDouble());
        Assert.Equal(0, missile.SectorIndex);
        sim.Tick();
        Assert.Equal(64 + acceleration, missile.Z.ToDouble());
        Assert.Equal(2 * acceleration, missile.VelocityZ.ToDouble()); Assert.False(missile.Destroyed);
    }

    [Fact]
    public void FallingMissileHitsFloorThroughExistingImpactPath()
    {
        var sim = Room(); var missile = Shoot(sim); missile.NoGravity = false; missile.Z = Fixed.FromInt(1);
        sim.Tick(); Assert.False(missile.Destroyed); Assert.Equal(-1, missile.VelocityZ.ToDouble());
        sim.Tick(); Assert.True(missile.Destroyed); Assert.Equal(0, missile.Z.ToDouble());
        Assert.InRange(sim.Level.Sectors[0].HealthFloor, 960, 995);
    }

    [Fact]
    public void RuntimeGravityChangeAffectsNextVelocityWithoutReaiming()
    {
        var sim = Room(); var missile = Shoot(sim); missile.NoGravity = false;
        sim.Tick(); missile.Gravity = Fixed.FromDouble(0.5);
        sim.Tick(); Assert.Equal(63, missile.Z.ToDouble()); Assert.Equal(-1.5, missile.VelocityZ.ToDouble());
        missile.NoGravity = true;
        sim.Tick(); Assert.Equal(61.5, missile.Z.ToDouble()); Assert.Equal(-1.5, missile.VelocityZ.ToDouble());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 256, HealthFloor = 1000 }],
        Things = [new LevelThing { Type = 1 }] });

    private static ProjectileActor Shoot(AuthoritySimulation sim)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(100); missile.Z = Fixed.FromInt(64);
        missile.VelocityX = missile.VelocityY = missile.VelocityZ = default;
        return missile;
    }
}
