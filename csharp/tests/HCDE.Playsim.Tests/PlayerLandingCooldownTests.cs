using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerLandingCooldownTests
{
    [Theory]
    [InlineData(-1, -2, -8, 7)]
    [InlineData(-18, -2, -8, 7)]
    [InlineData(0, -9, -8, 7)]
    [InlineData(4, -9, -8, 7)]
    [InlineData(0, -8, -8, 0)]
    [InlineData(4, -8, -8, 4)]
    [InlineData(0, -7, -6.5, 7)]
    public void FloorLandingUsesNegativeCounterOrStrictLandingThreshold(int initial, double velocity,
        double threshold, int expected)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.JumpTics = initial; player.LandingSpeed = threshold;
        Drop(player, velocity); ActorPhysics.Step(sim, player);
        Assert.Equal(expected, player.JumpTics); Assert.True(player.OnGround);
    }

    [Fact]
    public void NoGravityDoesNotSuppressJumpDelay()
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoGravity = true;
        Drop(player, -9); ActorPhysics.Step(sim, player); Assert.Equal(7, player.JumpTics);
    }

    [Fact]
    public void IceEarlyReturnDoesNotAssignJumpDelay()
    {
        var sim = Room(); var player = sim.Players.Single(); player.DamageType = "Ice"; player.JumpTics = -1;
        Drop(player, -9); ActorPhysics.Step(sim, player);
        Assert.Equal(-1, player.JumpTics); Assert.Equal(1, player.States.RemainingTics);
    }

    [Fact]
    public void AirbornePlayerKeepsCounter()
    {
        var sim = Room(); var player = sim.Players.Single(); player.JumpTics = -1;
        Drop(player, -9); player.Z = Fixed.FromInt(30); ActorPhysics.Step(sim, player);
        Assert.Equal(-1, player.JumpTics); Assert.False(player.OnGround);
    }

    [Fact]
    public void ActorSupportLandingDoesNotUseFloorJumpDelayRule()
    {
        var sim = Room(); var player = sim.Players.Single(); sim.AddBot(0, 0);
        player.JumpTics = -1; Drop(player, -9); player.Z = Fixed.FromInt(60);
        ActorPhysics.Step(sim, player);
        Assert.True(player.OnMobj); Assert.Equal(-1, player.JumpTics);
    }

    [Fact]
    public void LandingDelaySurvivesSaveAndBlocksHeldJumpUntilCountdownCompletes()
    {
        var sim = Room(); var player = sim.Players.Single(); Drop(player, -9); ActorPhysics.Step(sim, player);
        Assert.Equal(7, player.JumpTics);
        var saved = SimSavegame.Write(sim, true); var restored = Room(); SimSavegame.Apply(restored, saved);
        var loaded = restored.Players.Single();
        Assert.Equal(7, loaded.JumpTics); Assert.Equal(saved, SimSavegame.Write(restored, true));
        for (var i = 0; i < 6; i++)
        {
            var command = new PlayerCommand { Jump = true };
            sim.QueueCommand(0, command); restored.QueueCommand(0, command); sim.Tick(); restored.Tick();
            Assert.Equal(0, loaded.Z.Raw); Assert.Equal(6 - i, loaded.JumpTics);
            Assert.Equal(sim.Checksum, restored.Checksum);
        }
        sim.QueueCommand(0, new PlayerCommand { Jump = true });
        restored.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick(); restored.Tick();
        Assert.Equal(-1, loaded.JumpTics); Assert.True(loaded.Z.Raw > 0);
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static void Drop(PlayerPawn player, double velocity)
    { player.Z = Fixed.FromInt(1); player.VelocityZ = Fixed.FromDouble(velocity); player.OnGround = false; }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
