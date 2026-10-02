using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WallModelScrollTests
{
    private static PlayLevel Room(int special = 222, int flags = 0, int id = 7, MapDataFormat format = MapDataFormat.UdmfText,
        double targetX = 64, double targetY = 0, bool mid3D = false, bool duplicate = false)
    {
        var source = new LevelLine { Special = special, Tag = format == MapDataFormat.DoomBinary ? id : 99,
            Arg0 = id, Arg1 = flags, SideFront = 0, SideBack = -1, X2 = 32, Y2 = 16 };
        var target = new LevelLine { Tag = 7, SideFront = 1, SideBack = mid3D ? 2 : -1,
            Flags = mid3D ? LevelLine.MidTex3DFlag : 0, X2 = targetX, Y2 = targetY };
        return new PlayLevel { Format = format,
            Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { Index = 1, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 1 }, new LevelSide { Index = 2, Sector = 0 }],
            Lines = duplicate ? [source, target, new LevelLine { Special = special, Tag = source.Tag, Arg0 = id,
                Arg1 = flags, SideFront = 0, X2 = 32, Y2 = 16 }] : [source, target] };
    }

    [Theory]
    [InlineData(64, 0, -1, 0.5)]
    [InlineData(0, 64, -0.5, -1)]
    [InlineData(-64, 0, 1, -0.5)]
    [InlineData(0, -64, 0.5, 1)]
    public void ProjectionUsesTargetDirectionAndScrollsAllFrontParts(double x, double y, double dx, double dy)
    {
        var source = Room(targetX: x, targetY: y);
        var sim = AuthoritySimulation.Start(source); sim.Tick();
        var side = sim.Level.Sides[1];
        Assert.Equal(dx, side.TopTextureOffsetX, 6); Assert.Equal(dy, side.TopTextureOffsetY, 6);
        Assert.Equal(dx, side.MidTextureOffsetX, 6); Assert.Equal(dy, side.BottomTextureOffsetY, 6);
        Assert.Equal(0, sim.Level.Sides[0].TopTextureOffsetX);
        Assert.Equal(0, sim.Level.Lines[0].Special); Assert.Equal(222, source.Lines[0].Special);
    }

    [Fact]
    public void DiagonalProjectionUsesWallLength()
    {
        var sim = AuthoritySimulation.Start(Room(targetX: 64, targetY: 64)); sim.Tick();
        Assert.Equal(-1.5 / Math.Sqrt(2), sim.Level.Sides[1].TopTextureOffsetX, 6);
        Assert.Equal(-0.5 / Math.Sqrt(2), sim.Level.Sides[1].TopTextureOffsetY, 6);
    }

    [Fact]
    public void ZeroIdScrollsSourceWallOnly()
    {
        var sim = AuthoritySimulation.Start(Room(id: 0)); sim.Tick();
        Assert.Equal(-Math.Sqrt(1.25), sim.Level.Sides[0].TopTextureOffsetX, 6);
        Assert.Equal(0, sim.Level.Sides[0].TopTextureOffsetY, 6);
        Assert.Equal(0, sim.Level.Sides[1].TopTextureOffsetX);
    }

    [Theory]
    [InlineData(222, 1, MapDataFormat.UdmfText, false)]
    [InlineData(222, 2, MapDataFormat.UdmfText, true)]
    [InlineData(222, 3, MapDataFormat.HexenBinary, true)]
    [InlineData(249, 0, MapDataFormat.DoomBinary, false)]
    [InlineData(218, 0, MapDataFormat.DoomBinary, true)]
    public void ControllerAndAccelerationUseSourceSector(int special, int flags, MapDataFormat format, bool accelerating)
    {
        var sim = AuthoritySimulation.Start(Room(special, flags, format: format));
        sim.Floors[0] = 2; sim.Tick(); sim.Tick();
        Assert.Equal(accelerating ? -4 : -2, sim.Level.Sides[1].TopTextureOffsetX, 6);
        sim.Floors[1] = 3; sim.Tick();
        Assert.Equal(accelerating ? -6 : -2, sim.Level.Sides[1].TopTextureOffsetX, 6);
    }

    [Fact]
    public void DoomConstantModelAndThreeDimensionalMidTextureRule()
    {
        var sim = AuthoritySimulation.Start(Room(254, format: MapDataFormat.DoomBinary, mid3D: true)); sim.Tick();
        Assert.Equal(-1, sim.Level.Sides[1].TopTextureOffsetX, 6);
        Assert.Equal(-1, sim.Level.Sides[1].BottomTextureOffsetX, 6);
        Assert.Equal(0, sim.Level.Sides[1].MidTextureOffsetX);
        Assert.Equal(0, sim.Level.Sides[0].TopTextureOffsetX);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void SavedControllersContinueAndMatchChecksums(int flags)
    {
        var original = AuthoritySimulation.Start(Room(flags: flags));
        original.Floors[0] = 2; original.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(Room(flags: flags)); restored.RestoreState(state);
        for (var i = 0; i < 4; i++)
        {
            original.Floors[0] += i % 2; restored.Floors[0] = original.Floors[0];
            original.Tick(); restored.Tick();
            Assert.Equal(original.Checksum, restored.Checksum);
            Assert.Equal(original.CaptureState().TextureScrolls, restored.CaptureState().TextureScrolls);
        }
    }

    [Fact]
    public void VersionTwelveRetainsExistingWallControllers()
    {
        var sim = AuthoritySimulation.Start(Room(flags: 1));
        var bytes = SimSavegame.Write(new SimSaveState { TextureScrolls = [], IncludesCarryScrolls = true,
            IncludesCeilingControls = true, IncludesFloorControls = true });
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state); sim.Floors[0] = 2; sim.Tick();
        Assert.Equal(-2, sim.Level.Sides[1].TopTextureOffsetX, 6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void RuntimeWallUpdatesAllDuplicatesAndZeroRemovesThem(int flags)
    {
        var sim = AuthoritySimulation.Start(Room(flags: flags, duplicate: true));
        sim.SetWallTextureScroll(7, 0, 3, 0, WallScrollParts.All);
        if (flags != 0) sim.Floors[0] = 1;
        sim.Tick(); Assert.Equal(6, sim.Level.Sides[1].TopTextureOffsetX);
        Assert.Equal(2, sim.CaptureState().TextureScrolls!.Count);
        sim.SetWallTextureScroll(7, 0, 0, 0, WallScrollParts.All); sim.Tick();
        Assert.Equal(6, sim.Level.Sides[1].TopTextureOffsetX);
        Assert.Empty(sim.CaptureState().TextureScrolls!);
    }

    [Fact]
    public void ZeroLengthTargetFailsExplicitly()
    {
        Assert.Throws<InvalidOperationException>(() => AuthoritySimulation.Start(Room(targetX: 0, targetY: 0)));
    }

    [Fact]
    public void MissingTargetIdConsumesSpecialWithoutCreatingThinkers()
    {
        var sim = AuthoritySimulation.Start(Room(id: 123));
        Assert.Equal(0, sim.Level.Lines[0].Special);
        Assert.Empty(sim.CaptureState().TextureScrolls!);
    }

    [Theory]
    [InlineData(99, 0)]
    [InlineData(1, 99)]
    public void InvalidSavedWallOrControllerFailsBeforeClockMutation(int side, int control)
    {
        var sim = AuthoritySimulation.Start(Room(flags: 1));
        var state = new SimSaveState { Tic = 99, IncludesCarryScrolls = true, IncludesCeilingControls = true,
            IncludesFloorControls = true, IncludesWallControls = true,
            TextureScrolls = [new SimTextureScroll(side, 17, 1, 0, Control: control)] };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Thinkers.Clock.Tic);
    }
}
