using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomPlaneScrollTests
{
    private static PlayLevel Map(params LevelLine[] lines) => new()
    {
        Format = MapDataFormat.DoomBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 },
            new LevelSector { Index = 1, Tag = 8, CeilingHeight = 128 },
            new LevelSector { Index = 2, CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 1 },
            new LevelSide { Index = 2, Sector = 2 }],
        Lines = lines,
    };

    private static LevelLine Scroll(int special) => new()
    {
        Special = special, Tag = 7, SideFront = 2, X1 = 16, Y1 = 16, X2 = 80, Y2 = -16,
        Arg0 = 99, Arg1 = 0, Arg3 = 999, Arg4 = 999,
    };

    [Theory]
    [InlineData(214, 0, 2)]
    [InlineData(215, 1, 2)]
    [InlineData(216, 2, 2)]
    [InlineData(217, 3, 2)]
    [InlineData(245, 0, 1)]
    [InlineData(246, 1, 1)]
    [InlineData(247, 2, 1)]
    [InlineData(248, 3, 1)]
    [InlineData(250, 0, 0)]
    [InlineData(251, 1, 0)]
    [InlineData(252, 2, 0)]
    [InlineData(253, 3, 0)]
    public void NativeDoomMappingsSelectPlaneCarryAndController(int special, int component, int controller)
    {
        var source = Map(Scroll(special));
        var sim = AuthoritySimulation.Start(source);
        if (controller != 0) sim.Floors[2] = 2;
        sim.Tick(); sim.Tick();
        var distance = controller == 2 ? 8 : 4;
        Assert.Equal(component is 1 or 3 ? -distance : 0, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(component is 1 or 3 ? -distance / 2 : 0, sim.Level.Sectors[0].FloorTextureOffsetY);
        Assert.Equal(component == 0 ? -distance : 0, sim.Level.Sectors[0].CeilingTextureOffsetX);
        Assert.Equal(component >= 2 ? controller == 0 ? 2 : controller == 1 ? 0 : 4 : 0, sim.SectorScrollX[0]);
        Assert.Equal(0, sim.Level.Lines[0].Special);
        Assert.Equal(special, source.Lines[0].Special);
        Assert.Equal(0, sim.Level.Sectors[1].FloorTextureOffsetX);
    }

    [Theory]
    [InlineData(352, 1)]
    [InlineData(353, 2)]
    [InlineData(354, 6)]
    public void DoomCopySpecialsUseNativeMasksAndDoomTag(int special, int mask)
    {
        var sim = AuthoritySimulation.Start(Map(Scroll(250), Scroll(253),
            new LevelLine { Special = special, Tag = 7, Arg0 = 99, SideFront = 1 }));
        sim.Tick();
        Assert.Equal((mask & 1) != 0 ? -2 : 0, sim.Level.Sectors[1].CeilingTextureOffsetX);
        Assert.Equal((mask & 2) != 0 ? -2 : 0, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal((mask & 4) != 0 ? 2 : 0, sim.SectorScrollX[1]);
        Assert.All(sim.Level.Lines, line => Assert.Equal(0, line.Special));
    }

    [Theory]
    [InlineData(58)]
    [InlineData(223)]
    [InlineData(224)]
    public void OtherDoomSpecialsAreNotConsumedAsPlaneScrollers(int special)
    {
        var sim = AuthoritySimulation.Start(Map(Scroll(special)));
        Assert.Equal(special, sim.Level.Lines[0].Special);
        Assert.Empty(sim.CaptureState().TextureScrolls!);
    }

    [Fact]
    public void TranslatedControllerCopiesSurviveSaveAndRuntimeUpdate()
    {
        var source = Map(Scroll(217), new LevelLine { Special = 354, Tag = 7, SideFront = 1 });
        var original = AuthoritySimulation.Start(source);
        original.Floors[2] = 2; original.Tick();
        Assert.True(LineSpecials.ExecuteFloorSpecial(original, 223, 8, 32, 0, 2, 0));
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(source); restored.RestoreState(state);
        for (var i = 0; i < 3; i++)
        {
            original.Floors[2]++; restored.Floors[2] = original.Floors[2];
            original.Tick(); restored.Tick();
            Assert.Equal(original.Checksum, restored.Checksum);
            Assert.Equal(original.CaptureState().TextureScrolls, restored.CaptureState().TextureScrolls);
        }
    }

    [Theory]
    [InlineData(MapDataFormat.HexenBinary, 250)]
    [InlineData(MapDataFormat.HexenBinary, 354)]
    [InlineData(MapDataFormat.UdmfText, 250)]
    [InlineData(MapDataFormat.UdmfText, 354)]
    public void DoomTranslationDoesNotApplyToHexenOrUdmfActions(MapDataFormat format, int special)
    {
        var map = Map(Scroll(special));
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = format, Sectors = map.Sectors,
            Sides = map.Sides, Lines = map.Lines });
        Assert.Equal(special, sim.Level.Lines[0].Special);
        Assert.Empty(sim.CaptureState().TextureScrolls!);
    }
}
