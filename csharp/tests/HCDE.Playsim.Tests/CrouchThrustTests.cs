using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CrouchThrustTests
{
    [Theory]
    [InlineData(8192, 0)]
    [InlineData(0, 8192)]
    [InlineData(8192, 8192)]
    [InlineData(-8192, -8192)]
    public void FullCrouchHalvesNewThrust(int forward, int side)
    {
        var standing = Room(); var crouched = Room();
        Crouch(crouched, 8);
        var command = new PlayerCommand { ForwardMove = (short)forward, SideMove = (short)side, Crouch = true };
        Step(standing, new PlayerCommand { ForwardMove = (short)forward, SideMove = (short)side }); Step(crouched, command);
        var full = standing.Players.Single(); var half = crouched.Players.Single();
        Assert.Equal(full.X.ToDouble() * 0.5, half.X.ToDouble(), 5);
        Assert.Equal(full.Y.ToDouble() * 0.5, half.Y.ToDouble(), 5);
    }

    [Fact]
    public void PartialCrouchUsesFactorAndConfiguredSpeed()
    {
        var sim = Room(); Crouch(sim, 2);
        var player = sim.Players.Single();
        player.MovementSpeed = Fixed.FromInt(2);
        Step(sim, new PlayerCommand { ForwardMove = 8192, Crouch = true });
        Assert.Equal(2 * player.CrouchFactor, player.X.ToDouble(), 4);
    }

    [Fact]
    public void CrouchScalesThrustWithoutRescalingExistingMomentum()
    {
        var sim = Room(); Crouch(sim, 8);
        var player = sim.Players.Single(); player.VelocityX = Fixed.FromInt(4);
        Step(sim, new PlayerCommand { ForwardMove = 8192, Crouch = true });
        Assert.Equal(4.5, player.X.ToDouble());
        Assert.Equal(4.5 * ActorPhysics.GroundFriction, player.VelocityX.ToDouble());
    }

    [Fact]
    public void StandingRestoresFullThrust()
    {
        var sim = Room(); Crouch(sim, 8);
        for (var i = 0; i < 8; i++) Step(sim, new PlayerCommand());
        Step(sim, new PlayerCommand { ForwardMove = 8192 });
        Assert.Equal(1, sim.Players.Single().X.ToDouble());
    }

    private static void Crouch(AuthoritySimulation sim, int ticks)
    {
        for (var i = 0; i < ticks; i++) Step(sim, new PlayerCommand { Crouch = true });
    }
    private static void Step(AuthoritySimulation sim, PlayerCommand command) { sim.QueueCommand(0, command); sim.Tick(); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
