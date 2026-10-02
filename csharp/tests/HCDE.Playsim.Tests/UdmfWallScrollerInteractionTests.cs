using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UdmfWallScrollerInteractionTests
{
    private static PlayLevel Load(string properties, int flags = 0)
    {
        var text = $$"""
            namespace = "ZDoom";
            vertex { x = 0; y = 0; }
            vertex { x = 32; y = 0; }
            sector { heightceiling = 128; }
            sector { heightceiling = 128; }
            sidedef { sector = 0; }
            sidedef { sector = 1; {{properties}} }
            linedef { v1 = 0; v2 = 1; sidefront = 0; special = 222; arg0 = 7; arg1 = {{flags}}; }
            linedef { v1 = 0; v2 = 1; sidefront = 1; id = 7; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        return LevelBuilder.FromUdmf(map, "MAP01");
    }

    [Fact]
    public void UdmfAndLineModelCreateSeparateThinkersAndSumRates()
    {
        var sim = AuthoritySimulation.Start(Load("xscroll = 0.5;")); sim.Tick();
        Assert.Equal(-0.5, sim.Level.Sides[1].TopTextureOffsetX);
        Assert.Equal(-0.5, sim.Level.Sides[1].MidTextureOffsetX);
        Assert.Equal(2, sim.CaptureState().TextureScrolls!.Count);
        Assert.Equal(0.5, sim.CaptureState().TextureScrolls![0].Dx);
    }

    [Fact]
    public void NativeCreationDefaultsAllPartsForEveryParsedPartEntry()
    {
        var sim = AuthoritySimulation.Start(Load("xscroll = 0.5; xscrolltop = 1; xscrollmid = 2;")); sim.Tick();
        Assert.Equal(2.5, sim.Level.Sides[1].TopTextureOffsetX);
        Assert.Equal(2.5, sim.Level.Sides[1].MidTextureOffsetX);
        Assert.Equal(2.5, sim.Level.Sides[1].BottomTextureOffsetX);
        Assert.Equal(4, sim.CaptureState().TextureScrolls!.Count);
    }

    [Fact]
    public void UdmfScrollerDoesNotReplaceWallControllerAndBothSurviveArchive()
    {
        var source = Load("xscroll = 0.5;", flags: 2);
        var original = AuthoritySimulation.Start(source); original.Floors[0] = 2; original.Tick();
        Assert.Equal(-1.5, original.Level.Sides[1].TopTextureOffsetX);
        Assert.Equal(2, original.CaptureState().TextureScrolls!.Count);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(source); restored.RestoreState(state);
        original.Tick(); restored.Tick();
        Assert.Equal(-3, original.Level.Sides[1].TopTextureOffsetX);
        Assert.Equal(original.Checksum, restored.Checksum);
    }
}
