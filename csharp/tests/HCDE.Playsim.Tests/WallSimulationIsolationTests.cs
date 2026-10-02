using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WallSimulationIsolationTests
{
    [Fact]
    public void RuntimeOffsetAndScaleActionsDoNotMutateOtherSimulationOrSourceMap()
    {
        var level = Map();
        var first = AuthoritySimulation.Start(level);
        var second = AuthoritySimulation.Start(level);
        Assert.True(LineSpecials.ActivateMapLine(first, new PlayerPawn(),
            new LevelLine { Special = 53, Arg0 = 7, Arg1 = 131072, Arg4 = 7, PlayerUse = true }, true));
        Assert.True(LineSpecials.ActivateMapLine(first, new PlayerPawn(),
            new LevelLine { Special = 56, Arg0 = 7, Arg1 = 196608, Arg2 = 131072, Arg4 = 7, PlayerUse = true }, true));
        Assert.Equal(2, first.Level.Sides[0].MidTextureOffsetX);
        Assert.Equal(3, first.Level.Sides[0].MidTextureScaleX);
        Assert.Equal(10, second.Level.Sides[0].MidTextureOffsetX);
        Assert.Equal(1, second.Level.Sides[0].MidTextureScaleX);
        Assert.Equal(10, level.Sides[0].MidTextureOffsetX);
        Assert.Equal(1, level.Sides[0].MidTextureScaleX);
        Assert.Equal(10, AuthoritySimulation.Start(level).Level.Sides[0].MidTextureOffsetX);
    }

    [Fact]
    public void ScrollTickDoesNotAdvanceOtherSimulationOrSource()
    {
        var level = Map();
        level.Sides[0].MapLoadWallScrolls.Add((1, 2, 2));
        var first = AuthoritySimulation.Start(level);
        var second = AuthoritySimulation.Start(level);
        first.Tick();
        Assert.NotEqual(10, first.Level.Sides[0].MidTextureOffsetX);
        Assert.Equal(10, second.Level.Sides[0].MidTextureOffsetX);
        Assert.Equal(10, level.Sides[0].MidTextureOffsetX);
    }

    [Fact]
    public void MapCopiesPreserveScalesAndOwnScrollLists()
    {
        var level = Map();
        level.Sides[0].BottomTextureScaleY = -2;
        level.Sides[0].MapLoadWallScrolls.Add((1, 2, 4));
        var copy = level.CopyForSimulation();
        Assert.Equal(-2, copy.Sides[0].BottomTextureScaleY);
        Assert.Equal(level.Sides[0].MapLoadWallScrolls, copy.Sides[0].MapLoadWallScrolls);
        copy.Sides[0].MapLoadWallScrolls.Clear();
        Assert.Single(level.Sides[0].MapLoadWallScrolls);
    }

    private static PlayLevel Map() => new()
    {
        Format = MapDataFormat.UdmfText,
        Namespace = "ZDoom",
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide { MidTextureOffsetX = 10 }],
        Lines = [new LevelLine { Tag = 7, SideFront = 0, SideBack = -1 }],
    };
}
