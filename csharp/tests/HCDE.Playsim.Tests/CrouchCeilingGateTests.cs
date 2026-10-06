using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CrouchCeilingGateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CurrentCeilingContactPreventsStanding(bool ceilingHugger)
    {
        var sim = Room(); var player = sim.Players.Single();
        Crouch(sim);
        player.NoGravity = true;
        player.CeilingHugger = ceilingHugger;
        player.Z = Fixed.FromDouble(128 - player.Height.ToDouble());
        player.VelocityZ = default;
        var factor = player.CrouchFactor;
        sim.QueueCommand(0, new PlayerCommand()); sim.Tick();
        Assert.Equal(factor, player.CrouchFactor);
        Assert.Equal(128, player.Z.ToDouble() + player.Height.ToDouble());
    }

    [Fact]
    public void RepeatedStandAndShrinkClampAfterQuery()
    {
        var sim = Room(); var player = sim.Players.Single();
        Crouch(sim);
        Assert.Equal(0.5, player.CrouchFactor);
        for (var i = 0; i < 8; i++) { sim.QueueCommand(0, new PlayerCommand()); sim.Tick(); }
        Assert.Equal(1, player.CrouchFactor);
        Assert.Equal(56, player.Height.ToDouble());
    }

    private static void Crouch(AuthoritySimulation sim)
    {
        for (var i = 0; i < 8; i++) { sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick(); }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
