using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WallTransformSaveTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide { TopTextureOffsetX = 1.25, TopTextureOffsetY = -2.5,
            MidTextureOffsetX = 3.75, MidTextureOffsetY = -4.25, BottomTextureOffsetX = 5.5, BottomTextureOffsetY = 6.75,
            TopTextureScaleX = -1, TopTextureScaleY = 2, MidTextureScaleX = 0, MidTextureScaleY = 0.5,
            BottomTextureScaleX = 3, BottomTextureScaleY = -4 }],
    });

    [Fact]
    public void BinaryRoundTripRestoresAllWallFieldsExactly()
    {
        var sim = Room();
        var expected = sim.CaptureState().Walls;
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Equal(expected, state.Walls);
        sim.Level.Sides[0].MidTextureScaleX = 9;
        sim.Level.Sides[0].TopTextureOffsetY = 10;
        sim.RestoreState(state);
        Assert.Equal(expected, sim.CaptureState().Walls);
    }

    [Fact]
    public void VersionSixLeavesCurrentWallStateUntouched()
    {
        var sim = Room();
        var bytes = SimSavegame.Write(new SimSaveState());
        Assert.Equal(6, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Null(state.Walls);
        var before = sim.CaptureState().Walls;
        sim.RestoreState(state);
        Assert.Equal(before, sim.CaptureState().Walls);
    }

    [Fact]
    public void EveryTruncatedWallArchiveIsRejected()
    {
        var bytes = SimSavegame.Write(Room());
        for (var length = 0; length < bytes.Length; length++)
            Assert.False(SimSavegame.TryRead(bytes.AsSpan(0, length), out _, out _));
    }

    [Fact]
    public void NonfiniteWallPayloadIsRejected()
    {
        var bytes = SimSavegame.Write(new SimSaveState { Walls = Room().CaptureState().Walls });
        BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(bytes.Length - 96), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-wall-nonfinite", error);
    }

    [Fact]
    public void WrongWallCountFailsBeforeClockOrWallsChange()
    {
        var sim = Room();
        var state = new SimSaveState { Tic = 99, Walls = [] };
        var before = sim.CaptureState();
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before.Tic, sim.Thinkers.Clock.Tic);
        Assert.Equal(before.Walls, sim.CaptureState().Walls);
    }

    [Fact]
    public void NonfiniteStateCannotBeWrittenOrApplied()
    {
        var sim = Room();
        var wall = sim.CaptureState().Walls![0] with { TopX = double.PositiveInfinity };
        var state = new SimSaveState { Walls = [wall] };
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }
}
