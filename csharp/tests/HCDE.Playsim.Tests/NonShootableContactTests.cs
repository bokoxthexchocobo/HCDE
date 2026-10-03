using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NonShootableContactTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void MissileSkipsFlaggedTargetIndependentlyOfShootable(bool shootable, bool nonShootable)
    {
        var (sim, missile, target) = Setup(); target.Shootable = shootable; target.NonShootable = nonShootable;
        var health = target.Health; sim.Tick();
        var passes = !shootable || nonShootable;
        Assert.Equal(!passes, missile.Destroyed);
        if (passes) { Assert.Equal(30, missile.X.ToDouble()); Assert.Equal(health, target.Health); }
        else Assert.True(target.Health < health);
    }

    [Fact]
    public void MissileFlagDoesNotGrantTargetPassage()
    {
        var (sim, missile, _) = Setup(); missile.NonShootable = true;
        sim.Tick(); Assert.True(missile.Destroyed);
    }

    [Fact]
    public void NonShootableTargetStillBlocksOrdinaryMovement()
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        target.NonShootable = true;
        Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
    }

    [Fact]
    public void ContactFlagDoesNotPreventExplicitDamage()
    {
        var (sim, _, target) = Setup(); target.NonShootable = true; var health = target.Health;
        ActorDamage.Apply(target, 5); Assert.Equal(health - 5, target.Health);
    }

    [Fact]
    public void MissileHitsEnemyBehindSkippedTarget()
    {
        var (sim, missile, target) = Setup(); target.NonShootable = true;
        target.X = Fixed.FromInt(15); target.Radius = Fixed.FromInt(2);
        var rear = sim.AddBot(40, 0, 3002); rear.Brain = null;
        var frontHealth = target.Health; var rearHealth = rear.Health;
        sim.Tick(); Assert.True(missile.Destroyed); Assert.True(rear.Health < rearHealth);
        Assert.Equal(frontHealth, target.Health);
    }

    [Fact]
    public void AcsFlagRoundTripsWithoutChangingShootable()
    {
        var actor = new Actor { Shootable = true };
        Assert.True(AcsActorFlags.TrySet(actor, "nonshootable", true));
        Assert.True(AcsActorFlags.TryGet(actor, "NONSHOOTABLE", out var enabled)); Assert.True(enabled);
        Assert.True(actor.Shootable);
        Assert.True(AcsActorFlags.TrySet(actor, "NonShootable", false)); Assert.False(actor.NonShootable);
    }

    private static (AuthoritySimulation, ProjectileActor, Actor) Setup()
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0, 3004); var target = sim.AddBot(30, 0, 3001);
        owner.Brain = target.Brain = null; target.PainChance = 0;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
