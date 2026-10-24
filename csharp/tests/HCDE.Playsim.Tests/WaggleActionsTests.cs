using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WaggleActionsTests
{
    [Fact]
    public void FloorWaggleOscillatesAfterExpand()
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = LineSpecialFeasibleGaps.FloorWaggle, Arg0 = 7, Arg1 = 64, Arg2 = 32, Arg3 = 0, Arg4 = 0, PlayerUse = true }, true));
        var baseFloor = sim.Floors[0];
        for (var i = 0; i < 80; i++) sim.Tick();
        Assert.NotEqual(baseFloor, sim.Floors[0]);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, FloorHeight = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
