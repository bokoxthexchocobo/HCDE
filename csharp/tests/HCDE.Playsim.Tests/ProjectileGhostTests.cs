using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileGhostTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    public void GhostPassageRequiresTargetGhostAndMissileThruGhost(bool ghost, bool thruGhost, bool passes)
    {
        var (sim, missile, target) = Setup(); target.Ghost = ghost; missile.ThruGhost = thruGhost;
        var health = target.Health; sim.Tick(); Assert.Equal(!passes, missile.Destroyed);
        if (passes) { Assert.Equal(30, missile.X.ToDouble()); Assert.Equal(health, target.Health); }
        else Assert.True(target.Health < health);
    }

    [Fact]
    public void ReversedFlagsDoNotGrantPassage()
    {
        var (sim, missile, target) = Setup(); missile.Ghost = true; target.ThruGhost = true;
        sim.Tick(); Assert.True(missile.Destroyed);
    }

    [Fact]
    public void GhostTargetStillBlocksOrdinaryMovement()
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        mover.ThruGhost = true; target.Ghost = true;
        Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
    }

    [Fact]
    public void MissileCanHitEnemyBehindSkippedGhost()
    {
        var (sim, missile, ghost) = Setup(); ghost.Ghost = true; missile.ThruGhost = true;
        ghost.X = Fixed.FromInt(15); ghost.Radius = Fixed.FromInt(2);
        var rear = sim.AddBot(40, 0, 3002); rear.Brain = null; var health = rear.Health;
        sim.Tick(); Assert.True(missile.Destroyed); Assert.True(rear.Health < health);
        Assert.Equal(60, ghost.Health);
    }

    [Fact]
    public void ThruGhostDoesNotBypassFloorImpact()
    {
        var (sim, missile, target) = Setup(); missile.ThruGhost = true; target.X = Fixed.FromInt(500);
        missile.Z = Fixed.FromInt(1); missile.VelocityX = default; missile.VelocityZ = Fixed.FromInt(-2);
        sim.Tick(); Assert.True(missile.Destroyed); Assert.Equal(0, missile.Z.Raw);
    }

    [Theory]
    [InlineData("ghost")]
    [InlineData("thrughost")]
    public void AcsFlagsCanBeSetQueriedAndCleared(string flag)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, flag, true));
        Assert.True(AcsActorFlags.TryGet(actor, flag.ToUpperInvariant(), out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, flag, false));
        Assert.True(AcsActorFlags.TryGet(actor, flag, out enabled)); Assert.False(enabled);
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
