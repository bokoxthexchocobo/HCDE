namespace HCDE.Playsim.Tests;

public class ProjectileLedgeGravityTests
{
    [Theory]
    [InlineData(0, 0, false, 1, 1, -2)]
    [InlineData(1, 0, false, 1, 1, -1)]
    [InlineData(0, 1, false, 1, 1, 0)]
    [InlineData(1, -1, false, 1, 1, -2)]
    [InlineData(0, 0, true, 1, 1, 0)]
    [InlineData(0, 0, false, 0.25, 0.5, -0.25)]
    [InlineData(0, 0, false, 2, 1, -4)]
    [InlineData(0, 0, false, -0.5, 1, 1)]
    [InlineData(0, 0, false, 0, 1, 0)]
    public void FirstLedgeAccelerationUsesNativeRestingFloorGate(double z, double velocity,
        bool noGravity, double actorGravity, double sectorGravity, double expectedVelocity)
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.SetXYZ(-1, 80, z); missile.Radius = Fixed.FromInt(1);
        missile.VelocityX = Fixed.FromInt(4); missile.VelocityY = default;
        missile.VelocityZ = Fixed.FromDouble(velocity); missile.NoGravity = noGravity;
        missile.Gravity = Fixed.FromDouble(actorGravity); sim.Level.Sectors[1].Gravity = sectorGravity;
        missile.Tick();
        Assert.False(missile.Destroyed); Assert.Equal(1, missile.SectorIndex);
        Assert.Equal(z + velocity, missile.Z.ToDouble());
        Assert.Equal(expectedVelocity, missile.VelocityZ.ToDouble());
    }

    [Fact]
    public void SavedLedgeFallContinuesWithNormalGravity()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.SetXYZ(-1, 80, 0); missile.Radius = Fixed.FromInt(1);
        missile.VelocityX = Fixed.FromInt(4); missile.VelocityY = missile.VelocityZ = default;
        ActorPropertyActions.Gravity(missile); missile.Tick();
        Assert.Equal(-2, missile.VelocityZ.ToDouble());
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = GameplayFoundationTests.TwoRooms(-24, 128);
        restored.SpawnProjectile(restored.Players.Single(), ProjectileKind.Plasma);
        restored.RestoreState(state);
        var loaded = restored.Actors.OfType<ProjectileActor>().Single();
        missile.Tick(); loaded.Tick();
        Assert.Equal(-2, missile.Z.ToDouble()); Assert.Equal(-3, missile.VelocityZ.ToDouble());
        Assert.Equal(missile.Z, loaded.Z); Assert.Equal(missile.VelocityZ, loaded.VelocityZ);
        Assert.Equal(missile.RemainingTics, loaded.RemainingTics);
    }
}
