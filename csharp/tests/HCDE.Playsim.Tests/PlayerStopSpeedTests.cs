using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerStopSpeedTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    public void TinyHeldMovementInputIsNotStopped(int forward, int side)
    {
        var sim = Room(); var player = sim.Players.Single();
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = (short)forward, SideMove = (short)side }); sim.Tick();
        Assert.True(player.VelocityX.Raw != 0 || player.VelocityY.Raw != 0);
        sim.Tick(); Assert.Equal(0, player.VelocityX.Raw); Assert.Equal(0, player.VelocityY.Raw);
    }

    [Theory]
    [InlineData(0.03125, 0.03125, true)]
    [InlineData(0.0625, 0.03125, false)]
    [InlineData(0.03125, 1, false)]
    public void IdlePlayerUsesCoupledPreFrictionStopRule(double x, double y, bool stops)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.VelocityX = Fixed.FromDouble(x); player.VelocityY = Fixed.FromDouble(y); sim.Tick();
        Assert.Equal(Fixed.FromDouble(stops ? 0 : x * ActorPhysics.GroundFriction), player.VelocityX);
        Assert.Equal(Fixed.FromDouble(stops ? 0 : y * ActorPhysics.GroundFriction), player.VelocityY);
    }

    [Fact]
    public void ReactionDelayedInputStillPreventsStopping()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ReactionTime = 1;
        player.VelocityX = Fixed.FromDouble(0.03125);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 1 }); sim.Tick();
        Assert.Equal(Fixed.FromDouble(0.03125 * ActorPhysics.GroundFriction), player.VelocityX);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
