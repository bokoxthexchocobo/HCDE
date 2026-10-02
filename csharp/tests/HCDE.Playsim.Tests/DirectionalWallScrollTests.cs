using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DirectionalWallScrollTests
{
    private static AuthoritySimulation Room(LevelLine line, MapDataFormat format = MapDataFormat.HexenBinary) =>
        AuthoritySimulation.Start(new PlayLevel { Format = format,
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide()], Lines = [line] });

    [Theory]
    [InlineData(100, 1, 0)]
    [InlineData(101, -1, 0)]
    [InlineData(102, 0, 1)]
    [InlineData(103, 0, -1)]
    public void DirectionAndRateMatchNativeAndSpecialIsRetained(int special, int dx, int dy)
    {
        var sim = Room(new LevelLine { Special = special, Arg0 = 64, SideBack = -1 }); sim.Tick();
        Assert.Equal(dx, sim.Level.Sides[0].TopTextureOffsetX);
        Assert.Equal(dy, sim.Level.Sides[0].MidTextureOffsetY);
        Assert.Equal(dx, sim.Level.Sides[0].BottomTextureOffsetX);
        Assert.Equal(special, sim.Level.Lines[0].Special);
    }

    [Theory]
    [InlineData(-1, 7)]
    [InlineData(0, 7)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(5, 5)]
    [InlineData(6, 6)]
    [InlineData(7, 7)]
    [InlineData(8, 7)]
    public void NativePartMaskDefaultsAndSelection(int mask, int expected)
    {
        var sim = Room(new LevelLine { Special = 100, Arg0 = 32, Arg1 = mask, SideBack = -1 }); sim.Tick();
        Assert.Equal((expected & 1) != 0 ? 0.5 : 0, sim.Level.Sides[0].TopTextureOffsetX);
        Assert.Equal((expected & 2) != 0 ? 0.5 : 0, sim.Level.Sides[0].MidTextureOffsetX);
        Assert.Equal((expected & 4) != 0 ? 0.5 : 0, sim.Level.Sides[0].BottomTextureOffsetX);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void ScrollBothOnlyCreatesAtStartupForZeroId(int id)
    {
        var sim = Room(new LevelLine { Special = 221, Arg0 = id, Arg1 = 96, Arg2 = 32, Arg3 = 128, Arg4 = 64,
            SideBack = -1 }); sim.Tick();
        Assert.Equal(id == 0 ? 1 : 0, sim.Level.Sides[0].TopTextureOffsetX);
        Assert.Equal(id == 0 ? -1 : 0, sim.Level.Sides[0].MidTextureOffsetY);
        Assert.Equal(0, sim.Level.Lines[0].Special);
    }

    [Fact]
    public void DoomSpecialOneHundredIsNotAHexenWallScroller()
    {
        var sim = Room(new LevelLine { Special = 100, Arg0 = 64, SideFront = -1 }, MapDataFormat.DoomBinary);
        Assert.Equal(100, sim.Level.Lines[0].Special);
        Assert.Empty(sim.CaptureState().TextureScrolls!);
    }
}
