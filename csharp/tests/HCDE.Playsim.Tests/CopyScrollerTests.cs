using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CopyScrollerTests
{
    private static LevelLine Copy(int mask = 7, int tag = 7, int side = 1) =>
        new() { Special = 58, Arg0 = tag, Arg1 = mask, SideFront = side };

    private static LevelLine Scroll(int special, int flags = 0, int mode = 2, int tag = 7) =>
        new() { Special = special, Arg0 = tag, Arg1 = flags, Arg2 = mode,
            Arg3 = 160, Arg4 = 128, SideFront = 2 };

    private static PlayLevel Map(params LevelLine[] lines) => new()
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 },
            new LevelSector { Index = 1, Tag = 8, CeilingHeight = 128 },
            new LevelSector { Index = 2, CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 1 },
            new LevelSide { Index = 2, Sector = 2 }],
        Lines = lines,
    };

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(-1)]
    public void MaskBitsIndependentlySelectCeilingFloorAndCarry(int mask)
    {
        var sim = AuthoritySimulation.Start(Map(Scroll(224), Copy(mask), Scroll(223)));
        sim.Tick();
        Assert.Equal((mask & 1) != 0 ? -1 : 0, sim.Level.Sectors[1].CeilingTextureOffsetX);
        Assert.Equal((mask & 2) != 0 ? -1 : 0, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal((mask & 4) != 0 ? 1 : 0, sim.SectorScrollX[1]);
        Assert.Equal(-1, sim.Level.Sectors[0].CeilingTextureOffsetX);
        Assert.Equal(-1, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(1, sim.SectorScrollX[0]);
        Assert.All(sim.Level.Lines, line => Assert.Equal(0, line.Special));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CopyDiscoveryIsIndependentOfLineOrder(bool copyFirst)
    {
        var source = Map(copyFirst ? [Copy(), Scroll(223)] : [Scroll(223), Copy()]);
        var sim = AuthoritySimulation.Start(source);
        sim.Tick();
        Assert.Equal(-1, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(1, sim.SectorScrollX[1]);
        Assert.Contains(source.Lines, line => line.Special == 58);
        Assert.Contains(source.Lines, line => line.Special == 223);
    }

    [Fact]
    public void TargetAlreadySharingSourceTagDoesNotDuplicate()
    {
        var sim = AuthoritySimulation.Start(Map(Scroll(223), Copy(side: 0)));
        sim.Tick();
        Assert.Equal(-1, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(1, sim.SectorScrollX[0]);
        Assert.Equal(0, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(2, sim.CaptureState().TextureScrolls!.Count);
    }

    [Fact]
    public void CopyCanMatchScrollerWithNoOriginallyTaggedSector()
    {
        var sim = AuthoritySimulation.Start(Map(Scroll(223, tag: 99), Copy(tag: 99)));
        sim.Tick();
        Assert.Equal(-1, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(1, sim.SectorScrollX[1]);
        Assert.Equal(0, sim.Level.Sectors[0].FloorTextureOffsetX);
    }

    [Fact]
    public void DuplicateCopyLinesAndSourceScrollersEachAppend()
    {
        var sim = AuthoritySimulation.Start(Map(Scroll(223), Copy(), Copy(), Scroll(223)));
        sim.Tick();
        Assert.Equal(-4, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(4, sim.SectorScrollX[1]);
        Assert.Equal(-2, sim.Level.Sectors[0].FloorTextureOffsetX);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FloorModeStillGatesCopiedComponents(int mode)
    {
        var sim = AuthoritySimulation.Start(Map(Scroll(223, mode: mode), Copy()));
        sim.Tick();
        Assert.Equal(mode == 1 ? 0 : -1, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(mode > 0 ? 1 : 0, sim.SectorScrollX[1]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CopiesKeepOriginalControllerAndSurviveArchive(int flags)
    {
        var original = AuthoritySimulation.Start(Map(Scroll(223, flags), Copy(), Scroll(224, flags)));
        original.Floors[2] = 2; original.Tick();
        Assert.Equal(-2, original.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(-2, original.Level.Sectors[1].CeilingTextureOffsetX);
        Assert.Equal(2, original.SectorScrollX[1]);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = AuthoritySimulation.Start(Map(Scroll(223, flags), Copy(), Scroll(224, flags)));
        restored.RestoreState(state);
        original.Floors[1] = 3; restored.Floors[1] = 3;
        for (var i = 0; i < 4; i++)
        {
            original.Tick(); restored.Tick();
            Assert.Equal(flags == 1 ? -2 : -2 * (i + 2), original.Level.Sectors[1].FloorTextureOffsetX);
            Assert.Equal(original.CaptureState().TextureScrolls, restored.CaptureState().TextureScrolls);
            Assert.Equal(original.Checksum, restored.Checksum);
        }
    }

    [Fact]
    public void CopyDoesNotRecursivelyCopyAnotherCopyDestination()
    {
        var sim = AuthoritySimulation.Start(Map(Scroll(223), Copy(), Copy(tag: 8, side: 2)));
        sim.Tick();
        Assert.Equal(-1, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(0, sim.Level.Sectors[2].FloorTextureOffsetX);
    }

    [Fact]
    public void InvalidFrontSideFailsExplicitly()
    {
        Assert.Throws<InvalidOperationException>(() => AuthoritySimulation.Start(Map(Copy(side: -1))));
    }

    [Fact]
    public void DoomBinarySpecialFiftyEightIsPreserved()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = MapDataFormat.DoomBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Lines = [Copy(side: -1)] });
        Assert.Equal(58, sim.Level.Lines[0].Special);
        Assert.Empty(sim.CaptureState().TextureScrolls!);
    }

    [Fact]
    public void CopiedTextureUsesDestinationRotationAndCarryUsesSourceDirection()
    {
        var sim = AuthoritySimulation.Start(Map(Scroll(223), Copy()));
        sim.Level.Sectors[1].FloorTextureAngle = 0x40000000;
        sim.Tick();
        Assert.Equal(0, sim.Level.Sectors[1].FloorTextureOffsetX, 6);
        Assert.Equal(1, sim.Level.Sectors[1].FloorTextureOffsetY, 6);
        Assert.Equal(1, sim.SectorScrollX[1]);
        Assert.Equal(0, sim.SectorScrollY[1]);
    }
}
