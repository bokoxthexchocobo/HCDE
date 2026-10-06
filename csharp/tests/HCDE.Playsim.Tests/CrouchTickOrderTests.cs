using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CrouchTickOrderTests
{
    [Fact]
    public void InitialCrouchAffectsThrustInTheSameTic()
    {
        var sim = Room();
        Step(sim, new PlayerCommand { Crouch = true, ForwardMove = 8192 });
        var player = sim.Players.Single();
        Assert.Equal(1 - PlayerPawn.CrouchSpeed, player.CrouchFactor);
        Assert.Equal(player.CrouchFactor, player.X.ToDouble(), 4);
    }

    [Fact]
    public void ReleaseUsesTheExpandedFactorInTheSameTic()
    {
        var sim = Room(); Crouch(sim);
        Step(sim, new PlayerCommand { ForwardMove = 8192 });
        var player = sim.Players.Single();
        Assert.Equal(0.5 + PlayerPawn.CrouchSpeed, player.CrouchFactor);
        Assert.Equal(player.CrouchFactor, player.X.ToDouble(), 4);
    }

    [Fact]
    public void JumpOnFinalStandingTicLeavesTheGround()
    {
        var sim = Room(); Crouch(sim);
        while (sim.Players.Single().CrouchFactor < 1 - PlayerPawn.CrouchSpeed)
            Step(sim, new PlayerCommand());
        Step(sim, new PlayerCommand { Jump = true });
        var player = sim.Players.Single();
        Assert.Equal(1, player.CrouchFactor);
        Assert.False(player.UncrouchLocked);
        Assert.True(player.Z.ToDouble() > 0);
        Assert.False(player.OnGround);
    }

    [Fact]
    public void ReactionDelayStillUpdatesCrouchBeforeMovementResumes()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ReactionTime = 1;
        Step(sim, new PlayerCommand { Crouch = true, ForwardMove = 8192 });
        Assert.Equal(0, player.X.Raw); Assert.True(player.CrouchFactor < 1);
        Step(sim, new PlayerCommand { Crouch = true, ForwardMove = 8192 });
        Assert.Equal(player.CrouchFactor, player.X.ToDouble(), 4);
    }

    private static void Crouch(AuthoritySimulation sim)
    {
        for (var i = 0; i < 8; i++) Step(sim, new PlayerCommand { Crouch = true });
    }
    private static void Step(AuthoritySimulation sim, PlayerCommand command) { sim.QueueCommand(0, command); sim.Tick(); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
