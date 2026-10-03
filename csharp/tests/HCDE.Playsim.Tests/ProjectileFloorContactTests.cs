using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileFloorContactTests
{
    [Theory]
    [InlineData(0, 0, false, true)]
    [InlineData(0, -1, false, true)]
    [InlineData(0, 1, false, false)]
    [InlineData(1, -1, false, true)]
    [InlineData(1, 0, false, false)]
    [InlineData(0, 0, true, false)]
    [InlineData(0, -1, true, false)]
    [InlineData(0, 1, true, false)]
    public void FloorContactUsesPostMovementHeight(int z, int velocity, bool survivesFloor, bool destroyed)
    {
        var sim = Room(); var missile = Shoot(sim);
        missile.Z = Fixed.FromInt(z); missile.VelocityZ = Fixed.FromInt(velocity); missile.NoExplodeFloor = survivesFloor;
        sim.Tick(); Assert.Equal(destroyed, missile.Destroyed);
        Assert.Equal(destroyed, sim.Level.Sectors[0].HealthFloor < 1000);
        Assert.Equal(Math.Max(0, z + velocity), missile.Z.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredClearedFlagExplodesStationaryFloorMissile(bool serialized)
    {
        var sim = Room(); var missile = Shoot(sim); var state = sim.CaptureState();
        if (serialized) Assert.True(SimSavegame.TryRead(SimSavegame.Write(state), out state, out var error), error);
        missile.NoExplodeFloor = true; sim.Tick(); Assert.False(missile.Destroyed);
        sim.RestoreState(state); sim.Tick(); Assert.True(missile.Destroyed);
        Assert.True(sim.Level.Sectors[0].HealthFloor < 1000);
    }

    [Fact]
    public void StationarySkyFloorContactDoesNotExplode()
    {
        var sim = Room(); sim.Level.Sectors[0].FloorPic = "F_SKY1";
        var missile = Shoot(sim); var random = sim.CombatRandomState;
        sim.Tick(); Assert.True(missile.Destroyed); Assert.Equal(random, sim.CombatRandomState);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 1000 }], Things = [new LevelThing { Type = 1 }] });
    private static ProjectileActor Shoot(AuthoritySimulation sim)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(100); missile.Z = default;
        missile.VelocityX = missile.VelocityY = missile.VelocityZ = default;
        return missile;
    }
}
