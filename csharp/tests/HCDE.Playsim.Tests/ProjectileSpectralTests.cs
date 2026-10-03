using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileSpectralTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    public void OnlyOrdinaryMissilePassesSpectralTarget(bool targetSpectral, bool missileSpectral, bool passes)
    {
        var (sim, missile, target) = Setup(); target.Spectral = targetSpectral; missile.Spectral = missileSpectral;
        var health = target.Health; sim.Tick(); Assert.Equal(!passes, missile.Destroyed);
        if (passes) Assert.Equal(health, target.Health); else Assert.True(target.Health < health);
    }

    [Fact]
    public void SolidNonShootableSpectralActorStillStopsMissile()
    {
        var (sim, missile, target) = Setup(); target.Spectral = true; target.Shootable = false;
        var health = target.Health; sim.Tick(); Assert.True(missile.Destroyed); Assert.Equal(health, target.Health);
    }

    [Fact]
    public void SpeciesImmuneSpectralTargetStopsMissileBeforeSpectralPassage()
    {
        var (sim, missile, target) = Setup(3004); target.Spectral = true;
        var health = target.Health; sim.Tick(); Assert.True(missile.Destroyed); Assert.Equal(health, target.Health);
    }

    [Fact]
    public void SpectralTargetStillBlocksOrdinaryMovement()
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        target.Spectral = true; Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
    }

    [Fact]
    public void OrdinaryMissileCanHitEnemyBehindSpectralTarget()
    {
        var (sim, missile, target) = Setup(); target.Spectral = true;
        target.X = Fixed.FromInt(15); target.Radius = Fixed.FromInt(2);
        var rear = sim.AddBot(40, 0, 3002); rear.Brain = null; var health = rear.Health;
        var frontHealth = target.Health; sim.Tick(); Assert.True(missile.Destroyed);
        Assert.True(rear.Health < health); Assert.Equal(frontHealth, target.Health);
    }

    [Fact]
    public void SpectralMissileStillHonorsGhostPassage()
    {
        var (sim, missile, target) = Setup(); target.Spectral = target.Ghost = true;
        missile.Spectral = missile.ThruGhost = true; sim.Tick(); Assert.False(missile.Destroyed);
    }

    [Fact]
    public void AcsFlagRoundTrips()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "spectral", true));
        Assert.True(AcsActorFlags.TryGet(actor, "SPECTRAL", out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, "Spectral", false)); Assert.False(actor.Spectral);
    }

    private static (AuthoritySimulation, ProjectileActor, Actor) Setup(int targetType = 3001)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0, 3004); var target = sim.AddBot(30, 0, targetType);
        owner.Brain = target.Brain = null; target.PainChance = 0;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
