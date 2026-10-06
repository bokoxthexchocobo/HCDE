using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerLandCommandTests
{
    [Fact]
    public void LandRestoresGravityWithoutReplacingMomentum()
    {
        var sim = Room(); var player = sim.Players.Single(); player.VelocityZ = Fixed.FromInt(2);
        Step(sim, new PlayerCommand { UpMove = short.MinValue });
        Assert.False(player.NoGravity); Assert.Equal(66, player.Z.ToDouble());
        Assert.Equal(1, player.VelocityZ.ToDouble());
    }

    [Fact]
    public void LandRunsAfterFlightJump()
    {
        var sim = Room(); var player = sim.Players.Single();
        Step(sim, new PlayerCommand { UpMove = short.MinValue, Jump = true });
        Assert.False(player.NoGravity); Assert.Equal(67, player.Z.ToDouble());
        Assert.Equal(2, player.VelocityZ.ToDouble());
    }

    [Fact]
    public void ReactionDelayBlocksLandUntilMovementResumes()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ReactionTime = 1;
        var command = new PlayerCommand { UpMove = short.MinValue };
        Step(sim, command); Assert.True(player.NoGravity);
        Step(sim, command); Assert.False(player.NoGravity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-32767)]
    [InlineData(-768)]
    [InlineData(768)]
    public void OrdinaryVerticalInputDoesNotActAsLand(int up)
    {
        var sim = Room(); Step(sim, new PlayerCommand { UpMove = (short)up });
        Assert.True(sim.Players.Single().NoGravity);
    }

    private static void Step(AuthoritySimulation sim, PlayerCommand command) { sim.QueueCommand(0, command); sim.Tick(); }
    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
        });
        var player = sim.Players.Single(); player.NoGravity = true; player.Z = Fixed.FromInt(64); player.OnGround = false;
        return sim;
    }
}
