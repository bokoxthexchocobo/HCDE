using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThruBitsTests
{
    [Theory]
    [InlineData(1u, 1u, false, false, false)]
    [InlineData(1u, 1u, true, false, true)]
    [InlineData(1u, 1u, false, true, true)]
    [InlineData(1u, 1u, true, true, true)]
    [InlineData(1u, 2u, true, true, false)]
    [InlineData(0u, 1u, true, true, false)]
    [InlineData(0x80000000u, 0x80000000u, true, false, true)]
    [InlineData(5u, 6u, false, true, true)]
    public void BlastContactsUseSharedBitsAndEitherEnableFlag(uint first, uint second, bool enableFirst, bool enableSecond, bool passes)
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        mover.ThruBits = first; target.ThruBits = second;
        mover.AllowThruBits = enableFirst; target.AllowThruBits = enableSecond;
        mover.Blasted = true; mover.VelocityX = Fixed.FromInt(3);
        Assert.Equal(passes, ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.Equal(passes ? 0 : 3, target.VelocityX.ToDouble());
    }

    [Fact]
    public void MaskChangeRestoresOccupiedPositionBlocking()
    {
        var sim = Room(); var mover = sim.AddBot(0, 0); var target = sim.AddBot(0, 0);
        mover.ThruBits = target.ThruBits = 1; target.AllowThruBits = true;
        Assert.True(ActorPhysics.CanOccupy(sim, mover));
        target.ThruBits = 2; Assert.False(ActorPhysics.CanOccupy(sim, mover));
    }

    [Fact]
    public void SharedEnabledBitsRemoveBridgeAndRiderSupport()
    {
        var sim = Room(); var carrier = sim.AddBot(0, 0); var rider = sim.AddBot(0, 0);
        carrier.Height = Fixed.FromInt(16); carrier.ActsLikeBridge = true;
        rider.Z = carrier.Height; rider.OnGround = true;
        carrier.ThruBits = rider.ThruBits = 8; carrier.AllowThruBits = true;
        Assert.False(ActorPhysics.IsStandingOn(carrier, rider));
        ActorPhysics.FitToSector(sim, rider); Assert.Equal(0, rider.Z.Raw);
    }

    [Fact]
    public void ProjectilePassageUsesMissileMaskRatherThanOwnerMask()
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0); var target = sim.AddBot(30, 0);
        owner.Brain = target.Brain = null; owner.ThruBits = 2;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.ThruBits = target.ThruBits = 1; target.AllowThruBits = true;
        var health = target.Health; sim.Tick(); Assert.False(missile.Destroyed); Assert.Equal(health, target.Health);
    }

    [Fact]
    public void AcsEnableFlagRoundTrips()
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, "allowthrubits", true));
        Assert.True(AcsActorFlags.TryGet(actor, "ALLOWTHRUBITS", out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, "AllowThruBits", false)); Assert.False(actor.AllowThruBits);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
