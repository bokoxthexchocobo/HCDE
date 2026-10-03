using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThruSpeciesTests
{
    [Theory]
    [InlineData(3004, 3004, true)]
    [InlineData(3002, 58, true)]
    [InlineData(58, 3002, true)]
    [InlineData(3003, 69, true)]
    [InlineData(69, 3003, true)]
    [InlineData(3004, 3001, false)]
    public void MoverFlagFiltersItsOwnDoomSpecies(int moverType, int targetType, bool passes)
    {
        var sim = Room(); var mover = sim.AddBot(0, 0, moverType); var target = sim.AddBot(50, 0, targetType);
        mover.ThruSpecies = true; mover.Blasted = true; mover.VelocityX = Fixed.FromInt(3);
        Assert.Equal(passes, ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.Equal(passes ? 0 : 3, target.VelocityX.ToDouble());
    }

    [Fact]
    public void TargetFlagDoesNotGrantPassage()
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        target.ThruSpecies = true;
        Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OccupancyFilteringIsDirectional(bool moverFlag)
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(0, 0);
        mover.ThruSpecies = moverFlag; target.ThruSpecies = !moverFlag;
        Assert.Equal(moverFlag, ActorPhysics.CanOccupy(sim, mover));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RiderAndBridgeFilteringUsesRidersFlag(bool riderFlag)
    {
        var sim = Room(); var carrier = sim.AddBot(0, 0); var rider = sim.AddBot(0, 0);
        carrier.ActsLikeBridge = true; carrier.Height = Fixed.FromInt(16); rider.Z = carrier.Height;
        rider.OnGround = true; rider.ThruSpecies = riderFlag; carrier.ThruSpecies = !riderFlag;
        Assert.Equal(!riderFlag, ActorPhysics.IsStandingOn(carrier, rider));
        ActorPhysics.FitToSector(sim, rider); Assert.Equal(riderFlag ? 0 : 16, rider.Z.ToDouble());
    }

    [Fact]
    public void OrdinaryMonsterFlagDoesNotSkipDifferentSpecies()
    {
        var sim = Room(); var mover = sim.AddBot(0, 0, 3004); sim.AddBot(50, 0, 3001);
        mover.ThruSpecies = true;
        Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
    }

    [Fact]
    public void MissileFlagUsesMissileSpeciesRatherThanOwnersSpecies()
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0); var target = sim.AddBot(30, 0);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.ThruSpecies = true; sim.Tick(); Assert.True(missile.Destroyed);
    }

    [Fact]
    public void ClearingMoverFlagRestoresOrdinaryBlocking()
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); sim.AddBot(50, 0);
        mover.ThruSpecies = true; Assert.True(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        mover.X = default; mover.ThruSpecies = false;
        Assert.False(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
    }

    [Fact]
    public void SpeciesPassageDoesNotBypassWalls()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Lines = [new LevelLine { X1 = 24, X2 = 24, Y1 = -128, Y2 = 128, SideBack = -1 }] });
        var mover = sim.AddBot(0, 0); mover.ThruSpecies = true;
        Assert.False(ActorPhysics.TryMove(sim, mover, 40, 0, out _));
    }

    [Fact]
    public void AcsSupportsFlagRoundTrip()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "thruspecies", true));
        Assert.True(AcsActorFlags.TryGet(actor, "THRUSPECIES", out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, "ThruSpecies", false)); Assert.False(actor.ThruSpecies);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
