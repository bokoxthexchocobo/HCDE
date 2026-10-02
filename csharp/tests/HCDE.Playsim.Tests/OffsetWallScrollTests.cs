using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class OffsetWallScrollTests
{
    private static PlayLevel Room(int mask = 7, int flags = 0, int divisor = 4, int id = 7,
        int special = 225, MapDataFormat format = MapDataFormat.UdmfText) => new()
    {
        Format = format,
        Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { Index = 1, CeilingHeight = 128 }],
        Sides = [new LevelSide { MidTextureOffsetX = 8, MidTextureOffsetY = -4 },
            new LevelSide { Index = 1, Sector = 1 }],
        Lines = [new LevelLine { Special = special, Tag = id, Arg0 = mask, Arg1 = id, Arg2 = flags,
            Arg3 = divisor, SideFront = 0, SideBack = -1 },
            new LevelLine { Tag = 7, SideFront = 1, SideBack = -1 }],
    };

    [Theory]
    [InlineData(-1, -8)]
    [InlineData(0, -8)]
    [InlineData(4, -2)]
    public void SourceMiddleOffsetsDetermineRatesAndDivisorHasMinimumOne(int divisor, int dx)
    {
        var sim = AuthoritySimulation.Start(Room(divisor: divisor)); sim.Tick();
        Assert.Equal(dx, sim.Level.Sides[1].TopTextureOffsetX);
        Assert.Equal(dx / 2.0, sim.Level.Sides[1].TopTextureOffsetY);
        Assert.Equal(0, sim.Level.Lines[0].Special);
        // Unlike model scrolling, ID matching includes the source line.
        Assert.Equal(8 + dx, sim.Level.Sides[0].MidTextureOffsetX);
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(5, 5)]
    [InlineData(6, 6)]
    [InlineData(7, 7)]
    [InlineData(8, 7)]
    public void PartSelectionMatchesNative(int mask, int selected)
    {
        var sim = AuthoritySimulation.Start(Room(mask: mask)); sim.Tick();
        Assert.Equal((selected & 1) != 0 ? -2 : 0, sim.Level.Sides[1].TopTextureOffsetX);
        Assert.Equal((selected & 2) != 0 ? -2 : 0, sim.Level.Sides[1].MidTextureOffsetX);
        Assert.Equal((selected & 4) != 0 ? -2 : 0, sim.Level.Sides[1].BottomTextureOffsetX);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SourceSectorControlsDisplacementAndAcceleration(int flags)
    {
        var sim = AuthoritySimulation.Start(Room(mask: 1, flags: flags));
        sim.Floors[0] = 2; sim.Tick(); sim.Tick();
        Assert.Equal(flags == 1 ? -4 : -8, sim.Level.Sides[1].TopTextureOffsetX);
        Assert.Equal(0, sim.Level.Sides[1].MidTextureOffsetX);
    }

    [Theory]
    [InlineData(255, -8)]
    [InlineData(1024, -1)]
    [InlineData(1025, -2)]
    [InlineData(1026, -2)]
    public void DoomTranslationsUseNativeDivisorAndController(int special, int dx)
    {
        var sim = AuthoritySimulation.Start(Room(special: special, format: MapDataFormat.DoomBinary));
        if (special >= 1025) sim.Floors[0] = 2;
        sim.Tick();
        Assert.Equal(special == 255 ? 0 : dx, sim.Level.Sides[1].MidTextureOffsetX);
        Assert.Equal(8 + dx, sim.Level.Sides[0].MidTextureOffsetX);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 1)]
    [InlineData(6, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    [InlineData(4, 2)]
    [InlineData(5, 2)]
    [InlineData(6, 2)]
    public void EveryPartialControllerMaskSurvivesSaveAndRuntimeUpdates(int mask, int flags)
    {
        var source = Room(mask: mask, flags: flags);
        var original = AuthoritySimulation.Start(source); original.Floors[0] = 2; original.Tick();
        original.SetWallTextureScroll(7, 0, 3, 1, (WallScrollParts)mask);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(source); restored.RestoreState(state);
        for (var i = 0; i < 3; i++)
        {
            original.Floors[0]++; restored.Floors[0] = original.Floors[0];
            original.Tick(); restored.Tick();
            Assert.Equal(original.Checksum, restored.Checksum);
            Assert.Equal(original.CaptureState().TextureScrolls, restored.CaptureState().TextureScrolls);
        }
        original.SetWallTextureScroll(7, 0, 0, 0, (WallScrollParts)mask);
        Assert.Empty(original.CaptureState().TextureScrolls!);
    }

    [Fact]
    public void VersionThirteenRetainsPartialWallControllers()
    {
        var sim = AuthoritySimulation.Start(Room(mask: 1, flags: 1));
        var bytes = SimSavegame.Write(new SimSaveState { TextureScrolls = [], IncludesCarryScrolls = true,
            IncludesCeilingControls = true, IncludesFloorControls = true, IncludesWallControls = true });
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state); sim.Floors[0] = 2; sim.Tick();
        Assert.Equal(-4, sim.Level.Sides[1].TopTextureOffsetX);
    }

    [Fact]
    public void ZeroIdSelectsSourceOnlyAndStartupRatesRemainFixed()
    {
        var sim = AuthoritySimulation.Start(Room(id: 0));
        sim.Level.Sides[0].MidTextureOffsetX = 100; sim.Tick();
        Assert.Equal(98, sim.Level.Sides[0].MidTextureOffsetX);
        Assert.Equal(0, sim.Level.Sides[1].MidTextureOffsetX);
    }

    [Fact]
    public void InvalidPartialWallControllerCannotMutateClock()
    {
        var sim = AuthoritySimulation.Start(Room(mask: 1, flags: 1));
        var state = new SimSaveState { Tic = 99, IncludesCarryScrolls = true, IncludesCeilingControls = true,
            IncludesFloorControls = true, IncludesWallControls = true, IncludesWallParts = true,
            TextureScrolls = [new SimTextureScroll(99, 19, 1, 0, Control: 0)] };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Thinkers.Clock.Tic);
    }
}
