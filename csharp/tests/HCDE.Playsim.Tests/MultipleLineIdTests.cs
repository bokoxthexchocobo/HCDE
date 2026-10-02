using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MultipleLineIdTests
{
    private static PlayLevel Load(int special = 0)
    {
        var text = $$"""
            namespace = "ZDoom";
            vertex { x = 0; y = 0; } vertex { x = 32; y = 0; }
            sector { heightceiling = 128; }
            sidedef { sector = 0; offsetx = 4; }
            sidedef { sector = 0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; special = {{special}};
                arg0 = {{(special == 225 ? 7 : 8)}}; arg1 = {{(special == 225 ? 8 : 0)}}; }
            linedef { v1 = 0; v2 = 1; sidefront = 1; id = 7; moreids = "8 8 9"; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        return LevelBuilder.FromUdmf(map, "MAP01");
    }

    [Fact]
    public void SharedIdLookupReturnsEachLineOnceAndKeepsPrimary()
    {
        var sim = AuthoritySimulation.Start(Load());
        Assert.Same(sim.Level.Lines[1], AcsLineActivation.FirstLineFromId(sim, 8));
        Assert.Single(AcsLineActivation.LinesFromId(sim, 8));
        Assert.Equal(7, sim.Level.Lines[1].Tag);
    }

    [Theory]
    [InlineData(222, -1)]
    [InlineData(225, -4)]
    public void ModelAndOffsetStartupScrollersSelectAdditionalIds(int special, int rate)
    {
        var sim = AuthoritySimulation.Start(Load(special)); sim.Tick();
        Assert.Equal(rate, sim.Level.Sides[1].MidTextureOffsetX);
        Assert.Single(sim.CaptureState().TextureScrolls!);
    }

    [Fact]
    public void RuntimeWallAndTextureActionsUseExtraIdsWithoutDuplicating()
    {
        var sim = AuthoritySimulation.Start(Load());
        Assert.True(WallTextureOffset.Execute(sim, 53, 8, 65536, 0, 0, 7));
        Assert.True(WallTextureScale.Execute(sim, 56, 9, 131072, 65536, 0, 7));
        sim.SetWallTextureScroll(8, 0, 2, 0, WallScrollParts.All); sim.Tick();
        Assert.Equal(3, sim.Level.Sides[1].MidTextureOffsetX);
        Assert.Equal(2, sim.Level.Sides[1].MidTextureScaleX);
        Assert.Single(sim.CaptureState().TextureScrolls!);
    }

    [Fact]
    public void SimulationCopiesRetainReadOnlyIdsAndSaveContinuation()
    {
        var source = Load(222);
        var original = AuthoritySimulation.Start(source); original.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(source); restored.RestoreState(state);
        original.Tick(); restored.Tick();
        Assert.Equal(original.Checksum, restored.Checksum);
        Assert.Equal(new[] { 8, 9 }, restored.Level.Lines[1].AdditionalIds);
        Assert.Equal(222, source.Lines[0].Special);
    }
}
