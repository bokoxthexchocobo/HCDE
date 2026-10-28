using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SwitchCheckRangeTests
{
    [Fact]
    public void TopSwitchRequiresReachableHeight()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var line = sim.Level.Lines[0];
        player.Z = Fixed.FromDouble(0);
        Assert.False(SwitchActions.CheckUseRange(sim, player, line, backSide: false));
        player.Z = Fixed.FromDouble(100);
        Assert.True(SwitchActions.CheckUseRange(sim, player, line, backSide: false));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Namespace = "ZDoom",
        Sectors =
        [
            new LevelSector { FloorHeight = 0, CeilingHeight = 128 },
            new LevelSector { FloorHeight = 0, CeilingHeight = 128 },
        ],
        Sides =
        [
            new LevelSide { Sector = 0, TopTexture = "SW1BRCOM" },
            new LevelSide { Sector = 1 },
        ],
        Lines =
        [
            new LevelLine
            {
                X1 = -64, Y1 = 0, X2 = 64, Y2 = 0, SideFront = 0, SideBack = 1,
                Flags = LevelLine.CheckSwitchRangeFlag,
                Special = 112, Arg0 = 0, PlayerUse = true,
            },
        ],
        Things = [new LevelThing { Type = 1, X = 0, Y = 32 }],
    });
}
