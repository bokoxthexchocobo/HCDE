using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MultipleSectorTagTests
{
    private static PlayLevel Load()
    {
        const string text = """
            namespace = "ZDoom";
            vertex { x = 0; y = 0; } vertex { x = 64; y = 0; }
            sector { id = 7; moreids = "8 8 90000"; heightceiling = 128; }
            sidedef { sector = 0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; special = 223; arg0 = 8; arg2 = 2; arg3 = 160; arg4 = 128; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        return LevelBuilder.FromUdmf(map, "MAP01");
    }

    [Fact]
    public void StartupAndRuntimeScrollersUseExtraTagsWithoutDuplicates()
    {
        var sim = AuthoritySimulation.Start(Load()); sim.Tick();
        Assert.Equal(-1, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(2, sim.CaptureState().TextureScrolls!.Count);
        Assert.True(LineSpecials.ExecuteFloorSpecial(sim, 223, 90000, 64, 0, 2, 0)); sim.Tick();
        Assert.Equal(-3, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(2, sim.SectorScrollX[0]);
        Assert.Equal(2, sim.CaptureState().TextureScrolls!.Count);
    }

    [Fact]
    public void SharedSectorTargetingAndTextureActionsMatchExtraTags()
    {
        var sim = AuthoritySimulation.Start(Load());
        Assert.Equal(new[] { 0 }, LineSpecials.TargetSectors(sim, null, 8));
        Assert.True(SectorTexturePanning.Execute(sim, 187, 90000, 12, 0, 0, 0));
        Assert.Equal(12, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.True(SectorTextureScale.Execute(sim, 189, 8, 2, 0, 1, 0));
        Assert.Equal(0.5, sim.Level.Sectors[0].FloorTextureScaleX);
    }

    [Fact]
    public void CopySuppressionRecognizesAdditionalSourceTag()
    {
        var map = Load();
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = map.Format, Sectors = map.Sectors,
            Sides = map.Sides, Lines = [map.Lines[0], new LevelLine { Special = 58, Arg0 = 8, Arg1 = 6, SideFront = 0 }] });
        sim.Tick(); Assert.Equal(-1, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(2, sim.CaptureState().TextureScrolls!.Count);
    }

    [Fact]
    public void TagZeroRuntimeUpdatesFollowNativeStoredTagMembership()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        Assert.True(LineSpecials.ExecuteFloorSpecial(sim, 223, 0, 32, 0, 1, 0));
        Assert.True(LineSpecials.ExecuteFloorSpecial(sim, 223, 0, 32, 0, 1, 0)); sim.Tick();
        Assert.Equal(2, sim.SectorScrollX[0]);
        Assert.Equal(2, sim.CaptureState().TextureScrolls!.Count);
    }

    [Fact]
    public void CopyTagZeroDoesNotSuppressUntaggedDestination()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = MapDataFormat.UdmfText,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Sides = [new LevelSide()],
            Lines = [new LevelLine { Special = 223, Arg3 = 160, Arg4 = 128 },
                new LevelLine { Special = 58, Arg1 = 2 }] });
        sim.Tick(); Assert.Equal(-2, sim.Level.Sectors[0].FloorTextureOffsetX);
    }

    [Fact]
    public void SaveContinuationUsesSameStaticTagMetadata()
    {
        var source = Load(); var original = AuthoritySimulation.Start(source); original.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(source); restored.RestoreState(state);
        original.Tick(); restored.Tick(); Assert.Equal(original.Checksum, restored.Checksum);
        Assert.Equal(new[] { 8, 90000 }, restored.Level.Sectors[0].AdditionalTags);
    }
}
