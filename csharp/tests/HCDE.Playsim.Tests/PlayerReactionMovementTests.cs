using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerReactionMovementTests
{
    [Fact]
    public void CountdownBlocksYawThrustAndJumpThenResumesOnFollowingTic()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ReactionTime = 2;
        var command = new PlayerCommand { ForwardMove = 8192, YawDelta = 16384, Jump = true };
        for (var expected = 1; expected >= 0; expected--)
        {
            sim.QueueCommand(0, command); sim.Tick(); Assert.Equal(expected, player.ReactionTime);
            Assert.Equal(0u, player.Angle.Raw); Assert.Equal(0, player.X.Raw); Assert.Equal(0, player.Y.Raw);
            Assert.Equal(0, player.Z.Raw); Assert.True(player.OnGround);
        }
        sim.QueueCommand(0, command); sim.Tick();
        Assert.Equal(BamAngle.FromDegrees(90).Raw, player.Angle.Raw);
        Assert.Equal(1, player.Y.ToDouble()); Assert.Equal(7, player.Z.ToDouble());
    }

    [Theory]
    [InlineData(-1, -2)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void AnyNonZeroCounterBlocksMovementAndDecrementsWithIntegerWrap(int initial, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.ReactionTime = initial;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.Equal(expected, player.ReactionTime); Assert.Equal(0, player.X.Raw);
    }

    [Fact]
    public void FrozenMovementPreservesMomentumAndAllowsPitchAndCrouch()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ReactionTime = 2;
        player.VelocityX = Fixed.FromInt(4);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192, PitchDelta = 4096, Crouch = true }); sim.Tick();
        Assert.Equal(4, player.X.ToDouble()); Assert.Equal(4 * ActorPhysics.GroundFriction, player.VelocityX.ToDouble());
        Assert.Equal(22.5, player.PitchDegrees); Assert.True(player.CrouchFactor < 1);
    }

    [Fact]
    public void Turn180ArmsWhileFrozenButProgressWaitsForReactionToEnd()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ReactionTime = 1;
        sim.QueueCommand(0, new PlayerCommand { Turn180 = true }); sim.Tick();
        Assert.Equal(PlayerPawn.Turn180Ticks, player.TurnTicks); Assert.Equal(0u, player.Angle.Raw);
        sim.QueueCommand(0, new PlayerCommand { Turn180 = true }); sim.Tick();
        Assert.Equal(PlayerPawn.Turn180Ticks - 1, player.TurnTicks);
        Assert.Equal(BamAngle.FromDegrees(180.0 / PlayerPawn.Turn180Ticks).Raw, player.Angle.Raw);
    }

    [Fact]
    public void WeaponAttackAndUseStillProcessDuringReactionDelay()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ReactionTime = 2;
        sim.QueueCommand(0, new PlayerCommand { Attack = true, Use = true }); sim.Tick();
        Assert.Equal(49, player.Inventory.Bullets); Assert.True(player.UseHeld);
        Assert.Equal(1, player.ReactionTime);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
