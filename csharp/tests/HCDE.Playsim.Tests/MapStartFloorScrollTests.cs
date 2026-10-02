using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapStartFloorScrollTests
{
    private static PlayLevel Map(int flags, int mode, short tag = 7) => new()
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { Tag = tag, CeilingHeight = 128 }, new LevelSector { Index = 1, CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { Special = 223, Arg0 = tag, Arg1 = flags, Arg2 = mode,
            Arg3 = 160, Arg4 = 112, SideFront = 0, X2 = 64, Y2 = 32 }],
    };

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void BiasedRatesAndNativeModePredicates(int mode)
    {
        var source = Map(0, mode);
        var sim = AuthoritySimulation.Start(source);
        sim.Tick(); sim.Tick();
        Assert.Equal(mode == 1 ? 0 : -2, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(mode == 1 ? 0 : -1, sim.Level.Sectors[0].FloorTextureOffsetY);
        Assert.Equal(mode > 0 ? 1 : 0, sim.SectorScrollX[0]);
        Assert.Equal(mode > 0 ? -0.5 : 0, sim.SectorScrollY[0]);
        Assert.Equal(0, sim.Level.Lines[0].Special);
        Assert.Equal(223, source.Lines[0].Special);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 0)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    public void ControllerDrivesTextureAndCarryIndependently(int flags, int mode)
    {
        var sim = AuthoritySimulation.Start(Map(flags, mode));
        sim.Floors[1] = 2; sim.Tick(); sim.Tick();
        var distance = flags == 1 ? 2 : 4;
        Assert.Equal(mode == 1 ? 0 : -distance, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(mode > 0 && flags != 1 ? 2 : 0, sim.SectorScrollX[0]);
        Assert.Equal(0, sim.Level.Sectors[0].CeilingTextureOffsetX);
        sim.Floors[1] = 0; sim.Tick();
        Assert.Equal(mode == 1 || flags == 1 ? 0 : -4, sim.Level.Sectors[0].FloorTextureOffsetX);
    }

    [Fact]
    public void LineRatesTagZeroAndDuplicateScrollersAccumulate()
    {
        var map = Map(4, 2, 0);
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = map.Format, Sectors = map.Sectors,
            Sides = map.Sides, Lines = [map.Lines[0], map.Lines[0]] });
        sim.Tick();
        foreach (var sector in sim.Level.Sectors)
        {
            Assert.Equal(-4, sector.FloorTextureOffsetX);
            Assert.Equal(2, sector.FloorTextureOffsetY);
        }
        Assert.Equal(4, sim.SectorScrollX[0]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void SavedFloorControllersContinueExactly(int flags)
    {
        var original = AuthoritySimulation.Start(Map(flags, 2));
        original.Ceilings[1] += 2; original.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(Map(flags, 2));
        restored.RestoreState(state);
        for (var i = 0; i < 5; i++)
        {
            original.Floors[1] += i % 3 - 1; restored.Floors[1] = original.Floors[1];
            original.Tick(); restored.Tick();
            Assert.Equal(original.Level.Sectors[0].FloorTextureOffsetX, restored.Level.Sectors[0].FloorTextureOffsetX);
            Assert.Equal(original.SectorScrollX, restored.SectorScrollX);
            Assert.Equal(original.CaptureState().TextureScrolls, restored.CaptureState().TextureScrolls);
            Assert.Equal(original.Checksum, restored.Checksum);
        }
    }

    [Fact]
    public void VersionElevenPreservesExistingFloorController()
    {
        var sim = AuthoritySimulation.Start(Map(1, 0));
        var bytes = SimSavegame.Write(new SimSaveState { TextureScrolls = [], IncludesCarryScrolls = true, IncludesCeilingControls = true });
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state); sim.Floors[1] = 1; sim.Tick();
        Assert.Equal(-1, sim.Level.Sectors[0].FloorTextureOffsetX);
    }

    [Fact]
    public void InvalidControllerCannotMutateClock()
    {
        var sim = AuthoritySimulation.Start(Map(1, 0));
        var state = new SimSaveState { Tic = 99, IncludesCarryScrolls = true, IncludesCeilingControls = true,
            IncludesFloorControls = true, TextureScrolls = [new SimTextureScroll(0, 16, 1, 0, Control: 99)] };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Thinkers.Clock.Tic);
    }

    [Fact]
    public void ControlledFloorHonorsRotationWithoutRotatingCarry()
    {
        var sim = AuthoritySimulation.Start(Map(1, 2));
        sim.Level.Sectors[0].FloorTextureAngle = 0x40000000;
        sim.Floors[1] = 2; sim.Tick();
        Assert.Equal(-1, sim.Level.Sectors[0].FloorTextureOffsetX, 6);
        Assert.Equal(2, sim.Level.Sectors[0].FloorTextureOffsetY, 6);
        Assert.Equal(2, sim.SectorScrollX[0]);
        Assert.Equal(-1, sim.SectorScrollY[0]);
    }

    [Fact]
    public void ControllerModesAffectChecksumBeforeAnyMotion()
    {
        var displacement = AuthoritySimulation.Start(Map(1, 0));
        var acceleration = AuthoritySimulation.Start(Map(2, 0));
        displacement.Tick(); acceleration.Tick();
        Assert.Equal(displacement.Level.Sectors[0].FloorTextureOffsetX, acceleration.Level.Sectors[0].FloorTextureOffsetX);
        Assert.NotEqual(displacement.Checksum, acceleration.Checksum);
    }
}
