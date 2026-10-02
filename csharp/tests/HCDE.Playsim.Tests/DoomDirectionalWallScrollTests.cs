using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomDirectionalWallScrollTests
{
    private static PlayLevel Room(int special, bool mid3D = false) => new()
    {
        Format = MapDataFormat.DoomBinary,
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide(), new LevelSide { Index = 1 }],
        Lines = [new LevelLine { Special = special, Tag = 99, Arg0 = 255, Arg1 = 1, Arg2 = 255,
            Arg3 = 255, Arg4 = 255, SideFront = 0, SideBack = mid3D ? 1 : -1,
            Flags = mid3D ? LevelLine.MidTex3DFlag : 0 }],
    };

    [Theory]
    [InlineData(48, 1, 0, false)]
    [InlineData(85, -1, 0, false)]
    [InlineData(422, -1, 0, false)]
    [InlineData(423, 0, 1, false)]
    [InlineData(424, 0, -1, false)]
    [InlineData(425, 1, 1, true)]
    [InlineData(426, 1, -1, true)]
    [InlineData(427, -1, 1, true)]
    [InlineData(428, -1, -1, true)]
    public void NativeMappingsUseOneUnitPerTicAndIgnoreHexenArguments(int special, int dx, int dy, bool consumed)
    {
        var source = Room(special);
        var sim = AuthoritySimulation.Start(source); sim.Tick(); sim.Tick();
        var front = sim.Level.Sides[0];
        Assert.Equal(2 * dx, front.TopTextureOffsetX); Assert.Equal(2 * dy, front.TopTextureOffsetY);
        Assert.Equal(2 * dx, front.MidTextureOffsetX); Assert.Equal(2 * dy, front.MidTextureOffsetY);
        Assert.Equal(2 * dx, front.BottomTextureOffsetX); Assert.Equal(2 * dy, front.BottomTextureOffsetY);
        Assert.Equal(0, sim.Level.Sides[1].MidTextureOffsetX);
        Assert.Equal(consumed ? 0 : special, sim.Level.Lines[0].Special);
        Assert.Equal(special, source.Lines[0].Special);
    }

    [Theory]
    [InlineData(48)]
    [InlineData(428)]
    public void ThreeDimensionalMidTextureIsExcluded(int special)
    {
        var sim = AuthoritySimulation.Start(Room(special, mid3D: true)); sim.Tick();
        Assert.NotEqual(0, sim.Level.Sides[0].TopTextureOffsetX);
        Assert.NotEqual(0, sim.Level.Sides[0].BottomTextureOffsetX);
        Assert.Equal(0, sim.Level.Sides[0].MidTextureOffsetX);
        Assert.Equal(0, sim.Level.Sides[1].TopTextureOffsetX);
    }

    [Theory]
    [InlineData(48)]
    [InlineData(425)]
    public void SaveContinuationPreservesRatesAndDoesNotCreateDuplicates(int special)
    {
        var source = Room(special);
        var original = AuthoritySimulation.Start(source); original.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(source); restored.RestoreState(state);
        for (var i = 0; i < 3; i++)
        {
            original.Tick(); restored.Tick();
            Assert.Single(restored.CaptureState().TextureScrolls!);
            Assert.Equal(original.Checksum, restored.Checksum);
        }
    }

    [Theory]
    [InlineData(MapDataFormat.HexenBinary, 48)]
    [InlineData(MapDataFormat.UdmfText, 48)]
    [InlineData(MapDataFormat.HexenBinary, 425)]
    [InlineData(MapDataFormat.UdmfText, 425)]
    public void DoomMappingsDoNotReinterpretOtherMapFormats(MapDataFormat format, int special)
    {
        var source = Room(special);
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = format, Sectors = source.Sectors,
            Sides = source.Sides, Lines = source.Lines });
        Assert.Empty(sim.CaptureState().TextureScrolls!);
        Assert.Equal(special, sim.Level.Lines[0].Special);
    }

    [Fact]
    public void RuntimeWallRateChangeAndRemovalAffectTranslatedScroller()
    {
        var sim = AuthoritySimulation.Start(Room(48));
        sim.SetWallTextureScroll(99, 0, 2, -3, WallScrollParts.All); sim.Tick();
        Assert.Equal(2, sim.Level.Sides[0].TopTextureOffsetX);
        Assert.Equal(-3, sim.Level.Sides[0].TopTextureOffsetY);
        sim.SetWallTextureScroll(99, 0, 0, 0, WallScrollParts.All); sim.Tick();
        Assert.Equal(2, sim.Level.Sides[0].TopTextureOffsetX);
        Assert.Empty(sim.CaptureState().TextureScrolls!);
        Assert.Equal(48, sim.Level.Lines[0].Special);
    }
}
