using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileFloorSurvivalTests
{
    [Theory]
    [InlineData(false, "STONE")]
    [InlineData(true, "STONE")]
    [InlineData(false, "F_SKY1")]
    [InlineData(true, "F_SKY1")]
    public void FloorFlagStopsFallingWithoutDamageAndKeepsHorizontalTravel(bool gravity, string texture)
    {
        var sim = Room(texture); var missile = Shoot(sim);
        missile.NoExplodeFloor = true; missile.NoGravity = !gravity;
        missile.VelocityX = Fixed.FromInt(10); missile.VelocityZ = Fixed.FromInt(-10);
        var random = sim.CombatRandomState;
        sim.Tick();
        Assert.False(missile.Destroyed); Assert.Equal(110, missile.X.ToDouble());
        Assert.Equal(0, missile.Z.ToDouble()); Assert.Equal(0, missile.VelocityZ.Raw);
        sim.Tick(); Assert.Equal(120, missile.X.ToDouble()); Assert.Equal(0, missile.Z.ToDouble());
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor); Assert.Equal(random, sim.CombatRandomState);
    }

    [Fact]
    public void FlagDoesNotSuppressCeilingDamage()
    {
        var sim = Room(); var missile = Shoot(sim); missile.NoExplodeFloor = true;
        missile.Z = Fixed.FromInt(118); missile.VelocityZ = Fixed.FromInt(10);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.True(sim.Level.Sectors[0].HealthCeiling < 1000);
    }

    [Fact]
    public void FloorSurvivorStillHitsActorOnFollowingTic()
    {
        var sim = Room(); var target = sim.AddBot(150, 0); target.Brain = null;
        var missile = Shoot(sim); missile.NoExplodeFloor = true;
        missile.VelocityX = Fixed.FromInt(10); missile.VelocityZ = Fixed.FromInt(-10);
        sim.Tick(); Assert.False(missile.Destroyed);
        for (var i = 0; i < 4 && !missile.Destroyed; i++) sim.Tick();
        Assert.True(missile.Destroyed); Assert.True(target.Health < 20);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
    }

    [Fact]
    public void ClearingFlagRestoresFloorExplosion()
    {
        var sim = Room(); var missile = Shoot(sim); missile.NoExplodeFloor = true;
        missile.VelocityZ = Fixed.FromInt(-10); sim.Tick(); Assert.False(missile.Destroyed);
        missile.NoExplodeFloor = false; missile.VelocityZ = Fixed.FromInt(-1);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.True(sim.Level.Sectors[0].HealthFloor < 1000);
    }

    [Fact]
    public void FloorContactDoesNotSkipActorCollisionLaterInSameTic()
    {
        var sim = Room(); var target = sim.AddBot(150, 0); target.Brain = null;
        var missile = Shoot(sim); missile.NoExplodeFloor = true;
        missile.VelocityX = Fixed.FromInt(40); missile.VelocityZ = Fixed.FromInt(-10);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.True(target.Health < 20);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
    }

    [Fact]
    public void FloorSurvivalDoesNotBypassProjectileLifetime()
    {
        var sim = Room(); var missile = Shoot(sim); missile.NoExplodeFloor = true;
        missile.VelocityZ = Fixed.FromInt(-10);
        for (var i = 0; i < 174; i++) sim.Tick();
        Assert.False(missile.Destroyed); sim.Tick(); Assert.True(missile.Destroyed);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
    }

    [Fact]
    public void AcsFlagCanBeSetQueriedAndClearedCaseInsensitively()
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, "noexplodefloor", true));
        Assert.True(AcsActorFlags.TryGet(actor, "NOEXPLODEFLOOR", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "NoExplodeFloor", false)); Assert.False(actor.NoExplodeFloor);
        actor.Destroy(); Assert.False(AcsActorFlags.TrySet(actor, "NOEXPLODEFLOOR", true));
    }

    [Fact]
    public void FlagParticipatesInChecksum()
    {
        var first = Room(); var second = Room();
        Shoot(first).NoExplodeFloor = true; Shoot(second);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(string texture = "STONE") => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = texture, HealthFloor = 1000, HealthCeiling = 1000 }],
        Things = [new LevelThing { Type = 1 }] });

    private static ProjectileActor Shoot(AuthoritySimulation sim)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(100); missile.Z = Fixed.FromInt(2);
        missile.VelocityX = missile.VelocityY = missile.VelocityZ = default;
        return missile;
    }
}
