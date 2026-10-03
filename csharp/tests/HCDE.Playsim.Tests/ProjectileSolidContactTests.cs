using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileSolidContactTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void SolidOrShootableLiveTargetsStopMissiles(bool solid, bool shootable, bool stops)
    {
        var (sim, missile, target) = Setup(); target.Solid = solid; target.Shootable = shootable;
        var health = target.Health; sim.Tick(); Assert.Equal(stops, missile.Destroyed);
        if (shootable) Assert.True(target.Health < health); else Assert.Equal(health, target.Health);
        if (!stops) Assert.Equal(30, missile.X.ToDouble());
    }

    [Theory]
    [InlineData("nonshootable")]
    [InlineData("thruactors")]
    [InlineData("ghost")]
    [InlineData("thrubits")]
    public void PassageFlagsStillSkipSolidNonShootableTargets(string rule)
    {
        var (sim, missile, target) = Setup(); target.Shootable = false;
        switch (rule)
        {
            case "nonshootable": target.NonShootable = true; break;
            case "thruactors": target.ThruActors = true; break;
            case "ghost": target.Ghost = true; missile.ThruGhost = true; break;
            case "thrubits": target.ThruBits = missile.ThruBits = 1; target.AllowThruBits = true; break;
        }
        sim.Tick(); Assert.False(missile.Destroyed);
    }

    [Fact]
    public void SolidNonShootableFrontActorProtectsRearEnemy()
    {
        var (sim, missile, target) = Setup(); target.Shootable = false;
        target.X = Fixed.FromInt(15); target.Radius = Fixed.FromInt(2);
        var rear = sim.AddBot(40, 0, 3002); rear.Brain = null; var health = rear.Health;
        sim.Tick(); Assert.True(missile.Destroyed); Assert.Equal(health, rear.Health);
    }

    [Fact]
    public void SolidNonShootableImpactStillRunsRocketExplosion()
    {
        var (sim, _, target) = Setup(); target.Shootable = false;
        var owner = sim.Actors.First(a => a.DoomEdNum == 3004);
        // Remove the plasma before inserting a rocket at the same contact point.
        sim.Actors.OfType<ProjectileActor>().Single().Destroy();
        var rocket = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        rocket.X = rocket.Y = default; rocket.Z = Fixed.FromInt(20);
        rocket.VelocityX = Fixed.FromInt(30); rocket.VelocityY = rocket.VelocityZ = default;
        var nearby = sim.AddBot(30, 60, 3002); nearby.Brain = null; var health = nearby.Health;
        sim.Tick(); Assert.True(rocket.Destroyed); Assert.True(nearby.Health < health);
    }

    private static (AuthoritySimulation, ProjectileActor, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); var target = sim.AddBot(30, 0, 3001);
        owner.Brain = target.Brain = null; target.PainChance = 0;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
