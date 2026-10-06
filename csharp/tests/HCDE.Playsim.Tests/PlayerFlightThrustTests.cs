using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerFlightThrustTests
{
    [Theory]
    [InlineData(30, 8192)]
    [InlineData(-30, 8192)]
    [InlineData(30, -8192)]
    [InlineData(0, 8192)]
    public void ForwardFlightProjectsThrustAlongPitch(double pitch, int forward)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.NoGravity = true; player.PitchDegrees = pitch;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = (short)forward }); sim.Tick();
        Assert.Equal(forward / 8192.0 * Math.Cos(pitch * Math.PI / 180), player.X.ToDouble(), 4);
        Assert.Equal(64 - forward / 8192.0 * Math.Sin(pitch * Math.PI / 180), player.Z.ToDouble(), 4);
    }

    [Fact]
    public void SidewaysFlightRemainsHorizontal()
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoGravity = true; player.PitchDegrees = 60;
        sim.QueueCommand(0, new PlayerCommand { SideMove = 8192 }); sim.Tick();
        Assert.Equal(-1, player.Y.ToDouble()); Assert.Equal(64, player.Z.ToDouble());
    }

    [Fact]
    public void PitchDoesNotRedirectGravityEnabledGroundThrust()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Z = default; player.OnGround = true; player.PitchDegrees = 60;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.Equal(1, player.X.ToDouble()); Assert.Equal(0, player.Z.Raw);
    }

    [Fact]
    public void FlightJumpOverridesEarlierVerticalForwardThrust()
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoGravity = true; player.PitchDegrees = 30;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192, Jump = true }); sim.Tick();
        Assert.Equal(67, player.Z.ToDouble());
    }

    [Fact]
    public void PitchThrustAddsToMomentumAndUsesConfiguredSpeed()
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoGravity = true;
        player.PitchDegrees = -30; player.MovementSpeed = Fixed.FromInt(2); player.VelocityZ = Fixed.FromInt(4);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.Equal(69, player.Z.ToDouble(), 4);
        Assert.Equal(Math.Sqrt(3), player.X.ToDouble(), 4);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1, Z = 64 }]
    });
}
