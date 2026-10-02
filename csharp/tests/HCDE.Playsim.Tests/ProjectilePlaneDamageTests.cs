using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectilePlaneDamageTests
{
    private static AuthoritySimulation Room(string texture = "STONE", int health = 1000) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = health, HealthCeiling = health,
            HealthFloorGroup = 4, HealthCeilingGroup = 5, FloorPic = texture, CeilingPic = texture }],
        Lines = [new LevelLine { Health = health, HealthGroup = 4 }, new LevelLine { Health = health, HealthGroup = 5 }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static ProjectileActor Shoot(AuthoritySimulation sim, bool ceiling)
    {
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.X = Fixed.FromInt(100);
        missile.Z = Fixed.FromInt(ceiling ? 118 : 2);
        missile.VelocityX = missile.VelocityY = Fixed.FromInt(0);
        missile.VelocityZ = Fixed.FromInt(ceiling ? 10 : -10);
        return missile;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlaneCollisionDamagesOnlySelectedHealthGroup(bool ceiling)
    {
        var sim = Room(); var missile = Shoot(sim, ceiling);
        sim.Tick();
        Assert.True(missile.Destroyed);
        var sector = sim.Level.Sectors[0];
        var health = ceiling ? sector.HealthCeiling : sector.HealthFloor;
        Assert.InRange(health, 960, 995);
        Assert.Equal(0, (1000 - health) % 5);
        Assert.Equal(1000, ceiling ? sector.HealthFloor : sector.HealthCeiling);
        Assert.Equal(health, sim.Level.Lines[ceiling ? 1 : 0].Health);
        Assert.Equal(ceiling ? 120 : 0, missile.Z.ToDouble());
    }

    [Theory]
    [InlineData(false, "F_SKY1", 1000)]
    [InlineData(true, "f_sky1", 1000)]
    [InlineData(false, "STONE", 0)]
    [InlineData(true, "STONE", 0)]
    public void SkyOrZeroHealthDoesNotConsumeDamageRandomness(bool ceiling, string texture, int health)
    {
        var sim = Room(texture, health); var missile = Shoot(sim, ceiling); var random = sim.CombatRandomState;
        sim.Tick();
        Assert.True(missile.Destroyed);
        Assert.Equal(random, sim.CombatRandomState);
        Assert.Equal(health, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(health, sim.Level.Sectors[0].HealthCeiling);
    }

    [Fact]
    public void CloserActorPreventsPlaneDamage()
    {
        var sim = Room(); var target = sim.AddBot(100, 0);
        var missile = Shoot(sim, false);
        sim.Tick();
        Assert.True(missile.Destroyed);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
        Assert.True(target.Health < 100);
    }
}
