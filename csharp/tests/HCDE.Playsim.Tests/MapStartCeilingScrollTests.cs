using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapStartCeilingScrollTests
{
    private static PlayLevel Map(params LevelLine[] lines) => new()
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }, new LevelSector { Index = 1, CeilingHeight = 128 }],
        Lines = lines,
    };

    [Fact]
    public void BiasedRatesUseArgumentsThreeAndFourAndConsumeSpecial()
    {
        var source = Map(new LevelLine { Special = 224, Arg0 = 7, Arg3 = 160, Arg4 = 112, PlayerUse = true });
        var sim = AuthoritySimulation.Start(source);
        sim.Tick();
        Assert.Equal(-1, sim.Level.Sectors[0].CeilingTextureOffsetX);
        Assert.Equal(-0.5, sim.Level.Sectors[0].CeilingTextureOffsetY);
        Assert.Equal(0, sim.Level.Lines[0].Special);
        Assert.Equal(224, source.Lines[0].Special);
        Assert.Equal(0, sim.Level.Sectors[1].CeilingTextureOffsetX);
    }

    [Fact]
    public void LineDirectionFlagAndDuplicateScrollersAccumulate()
    {
        var sim = AuthoritySimulation.Start(Map(
            new LevelLine { Special = 224, Arg0 = 7, Arg1 = 4, X2 = 64, Y2 = 32 },
            new LevelLine { Special = 224, Arg0 = 7, Arg3 = 144, Arg4 = 128 }));
        sim.Tick();
        Assert.Equal(-2.5, sim.Level.Sectors[0].CeilingTextureOffsetX);
        Assert.Equal(1, sim.Level.Sectors[0].CeilingTextureOffsetY);
        Assert.Equal(2, sim.CaptureState().TextureScrolls!.Count);
    }

    [Fact]
    public void TagZeroSelectsUntaggedSectors()
    {
        var sim = AuthoritySimulation.Start(Map(new LevelLine { Special = 224, Arg3 = 160, Arg4 = 128 }));
        sim.Tick();
        Assert.Equal(0, sim.Level.Sectors[0].CeilingTextureOffsetX);
        Assert.Equal(-1, sim.Level.Sectors[1].CeilingTextureOffsetX);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ControllerUsesHeightDeltaAndRetainsAcceleration(int flags)
    {
        var sim = AuthoritySimulation.Start(ControlledMap(flags));
        sim.Floors[1] = 2; sim.Tick();
        Assert.Equal(-2, sim.Level.Sectors[0].CeilingTextureOffsetX);
        sim.Tick();
        Assert.Equal(flags == 1 ? -2 : -4, sim.Level.Sectors[0].CeilingTextureOffsetX);
        sim.Floors[1] = 0; sim.Tick();
        Assert.Equal(flags == 1 ? 0 : -4, sim.Level.Sectors[0].CeilingTextureOffsetX);
    }

    private static PlayLevel ControlledMap(int flags)
    {
        return new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }, new LevelSector { Index = 1, CeilingHeight = 128 }],
            Lines = [new LevelLine { Special = 224, Arg0 = 7, Arg1 = flags, Arg3 = 160, Arg4 = 128, SideFront = 0 }],
            Sides = [new LevelSide { Sector = 1 }],
        };
    }

    [Fact]
    public void ArchiveRestoresControllerHistoryAndVelocity()
    {
        var original = AuthoritySimulation.Start(ControlledMap(2));
        original.Ceilings[1] += 2; original.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(ControlledMap(2));
        restored.RestoreState(state);
        for (var i = 0; i < 5; i++)
        {
            original.Floors[1] += i % 3 - 1; restored.Floors[1] = original.Floors[1];
            original.Tick(); restored.Tick();
            Assert.Equal(original.Level.Sectors[0].CeilingTextureOffsetX, restored.Level.Sectors[0].CeilingTextureOffsetX);
            Assert.Equal(original.CaptureState().TextureScrolls, restored.CaptureState().TextureScrolls);
        }
    }

    [Fact]
    public void VersionTenPreservesExistingCeilingController()
    {
        var sim = AuthoritySimulation.Start(ControlledMap(1));
        var bytes = SimSavegame.Write(new SimSaveState { TextureScrolls = [], IncludesCarryScrolls = true });
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state); sim.Floors[1] = 1; sim.Tick();
        Assert.Equal(-1, sim.Level.Sectors[0].CeilingTextureOffsetX);
    }

    [Fact]
    public void MissingControllerSideFailsExplicitly()
    {
        Assert.Throws<InvalidOperationException>(() => AuthoritySimulation.Start(Map(new LevelLine { Special = 224, Arg1 = 1 })));
    }

    [Fact]
    public void InvalidSavedControllerFailsBeforeClockMutation()
    {
        var sim = AuthoritySimulation.Start(ControlledMap(1));
        var state = new SimSaveState { Tic = 99, IncludesCarryScrolls = true, IncludesCeilingControls = true,
            TextureScrolls = [new SimTextureScroll(0, 13, -1, 0, Control: 99)] };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Thinkers.Clock.Tic);
    }
}
