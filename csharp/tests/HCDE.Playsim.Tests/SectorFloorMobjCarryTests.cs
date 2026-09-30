using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorFloorMobjCarryTests
{
    [Fact]
    public void RisingSectorFloorCarriesPlatformAndRiderOnMobj()
    {
        var sim = Room();
        var platform = sim.AddBot(0, 0);
        platform.Brain = null;
        platform.Height = Fixed.FromInt(16);
        var player = sim.Players.Single();
        player.X = platform.X;
        player.Y = platform.Y;
        player.Z = Fixed.FromInt(16);
        player.VelocityZ = default;
        Assert.True(LineSpecials.ActivateMapLine(sim, player, new LevelLine { Special = 18, Tag = 7 }, true));
        for (var tick = 0; tick < 8; tick++)
            sim.Tick();
        Assert.Equal(8, sim.FloorOf(0));
        Assert.Equal(8, platform.Z.ToDouble());
        Assert.Equal(24, player.Z.ToDouble());
        Assert.True(player.OnMobj);
        Assert.True(player.OnGround);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.DoomBinary,
        Namespace = "ZDoom",
        Sectors =
        [
            new LevelSector { Tag = 7, FloorHeight = 0, CeilingHeight = 128 },
            new LevelSector { Index = 1, FloorHeight = 64, CeilingHeight = 256 },
        ],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines =
        [
            new LevelLine { X1 = -128, Y1 = -128, X2 = 128, Y2 = -128, SideBack = -1 },
            new LevelLine { X1 = 128, Y1 = -128, X2 = 128, Y2 = 128, SideBack = 1 },
            new LevelLine { X1 = 128, Y1 = 128, X2 = -128, Y2 = 128, SideBack = -1 },
            new LevelLine { X1 = -128, Y1 = 128, X2 = -128, Y2 = -128, SideBack = -1 },
        ],
        Things = [new LevelThing { Type = 1 }],
    });
}
