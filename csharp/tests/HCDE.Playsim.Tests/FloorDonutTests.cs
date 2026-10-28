using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloorDonutTests
{
    [Fact]
    public void DonutSpecialStartsPillarAndPoolFloors()
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = 250, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
        Assert.True(sim.Motions.Count >= 1);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors =
        [
            new LevelSector { Tag = 7, FloorHeight = 32, CeilingHeight = 128 },
            new LevelSector { Tag = 8, FloorHeight = 0, CeilingHeight = 64 },
            new LevelSector { Tag = 9, FloorHeight = 16, CeilingHeight = 64 },
        ],
        Sides =
        [
            new LevelSide { Sector = 0 },
            new LevelSide { Sector = 1 },
            new LevelSide { Sector = 2 },
            new LevelSide { Sector = 1 },
        ],
        Lines =
        [
            new LevelLine { SideFront = 0, SideBack = 1 },
            new LevelLine { SideFront = 1, SideBack = 2 },
        ],
        Things = [new LevelThing { Type = 1 }],
    });
}
