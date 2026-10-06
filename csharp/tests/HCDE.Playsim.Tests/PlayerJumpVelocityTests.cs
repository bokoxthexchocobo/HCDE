using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerJumpVelocityTests
{
    [Theory]
    [InlineData(2, 8, 10)]
    [InlineData(-2, 8, 6)]
    [InlineData(4, 0, 4)]
    [InlineData(4, -2, 2)]
    public void GroundJumpAddsConfiguredImpulse(double velocity, double jump, double expected)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.VelocityZ = Fixed.FromDouble(velocity); player.JumpZ = Fixed.FromDouble(jump);
        Step(sim);
        Assert.Equal(expected, player.Z.ToDouble());
        Assert.Equal(expected - 1, player.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(0, 8)]
    [InlineData(64, -12)]
    public void FlightJumpSetsThreeRatherThanAddingJumpZ(double z, double velocity)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.NoGravity = true; player.Z = Fixed.FromDouble(z);
        player.OnGround = z == 0; player.VelocityZ = Fixed.FromDouble(velocity);
        player.JumpZ = Fixed.FromInt(20);
        Step(sim);
        Assert.Equal(z + 3, player.Z.ToDouble());
        Assert.Equal(3 * ActorPhysics.FlyingFriction, player.VelocityZ.ToDouble());
    }

    [Fact]
    public void FlightJumpWhileCrouchedOnlyStandsUp()
    {
        var sim = Room(); var player = sim.Players.Single();
        for (var i = 0; i < 8; i++) { sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick(); }
        player.NoGravity = true;
        Step(sim);
        Assert.True(player.UncrouchLocked);
        Assert.Equal(0, player.Z.Raw);
    }

    [Fact]
    public void AirborneGravityPlayerCannotApplyAnotherJumpImpulse()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Z = Fixed.FromInt(64); player.OnGround = false; player.VelocityZ = Fixed.FromInt(2);
        Step(sim);
        Assert.Equal(66, player.Z.ToDouble());
        Assert.Equal(1, player.VelocityZ.ToDouble());
    }

    private static void Step(AuthoritySimulation sim) { sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick(); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
