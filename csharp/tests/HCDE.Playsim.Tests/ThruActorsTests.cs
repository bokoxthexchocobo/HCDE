using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThruActorsTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void EitherFlagSkipsBlastCollision(bool moverFlag, bool targetFlag)
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        mover.ThruActors = moverFlag; target.ThruActors = targetFlag;
        var health = target.Health; mover.Blasted = true; mover.VelocityX = Fixed.FromInt(4);
        Assert.True(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.Equal(0, target.VelocityX.Raw); Assert.Equal(health, target.Health);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EitherFlagAllowsOccupiedPosition(bool moverFlag)
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(0, 0);
        mover.ThruActors = moverFlag; target.ThruActors = !moverFlag;
        Assert.True(ActorPhysics.CanOccupy(sim, mover));
        mover.ThruActors = target.ThruActors = false;
        Assert.False(ActorPhysics.CanOccupy(sim, mover));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EitherFlagDisablesRiderContact(bool carrierFlag)
    {
        var carrier = new Actor(); var rider = new Actor { Z = carrier.Height };
        Assert.True(ActorPhysics.IsStandingOn(carrier, rider));
        carrier.ThruActors = carrierFlag; rider.ThruActors = !carrierFlag;
        Assert.False(ActorPhysics.IsStandingOn(carrier, rider));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EitherFlagLetsProjectilePassWithoutDamage(bool missileFlag)
    {
        var sim = Room(); var owner = sim.AddBot(-100, 0); var target = sim.AddBot(30, 0);
        owner.Brain = target.Brain = null; var health = target.Health;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.ThruActors = missileFlag; target.ThruActors = !missileFlag;
        sim.Tick();
        Assert.False(missile.Destroyed); Assert.Equal(30, missile.X.ToDouble());
        Assert.Equal(health, target.Health);
    }

    [Fact]
    public void ActorPassThroughDoesNotBypassWalls()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Lines = [new LevelLine { X1 = 24, X2 = 24, Y1 = -128, Y2 = 128, SideBack = -1 }] });
        var actor = sim.AddBot(0, 0); actor.ThruActors = true;
        Assert.False(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
        Assert.False(ActorPhysics.CanOccupy(sim, new Actor { ThruActors = true, X = Fixed.FromInt(24) }));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EitherFlagRemovesBridgeSupport(bool riderFlag)
    {
        var sim = Room(); var carrier = sim.AddBot(0, 0); var rider = sim.AddBot(0, 0);
        carrier.ActsLikeBridge = true; carrier.Height = Fixed.FromInt(16);
        rider.Z = carrier.Height; rider.OnGround = true;
        carrier.ThruActors = !riderFlag; rider.ThruActors = riderFlag;
        ActorPhysics.FitToSector(sim, rider); Assert.Equal(0, rider.Z.Raw);
    }

    [Fact]
    public void AcsCanSetQueryAndClearFlag()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "thruactors", true));
        Assert.True(AcsActorFlags.TryGet(actor, "THRUACTORS", out var set)); Assert.True(set);
        Assert.True(AcsActorFlags.TrySet(actor, "ThruActors", false)); Assert.False(actor.ThruActors);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
