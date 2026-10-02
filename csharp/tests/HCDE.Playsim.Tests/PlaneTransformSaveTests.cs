using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlaneTransformSaveTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128, FloorTextureOffsetX = 1.25,
            FloorTextureOffsetY = -2.5, CeilingTextureOffsetX = 3.75, CeilingTextureOffsetY = -4.5,
            FloorTextureScaleX = 0, FloorTextureScaleY = -2, CeilingTextureScaleX = 3, CeilingTextureScaleY = 0.5,
            FloorTextureBaseOffsetY = 6.25, CeilingTextureBaseOffsetY = -7.5,
            FloorTextureAngle = 123, CeilingTextureAngle = 456,
            FloorTextureBaseAngle = uint.MaxValue, CeilingTextureBaseAngle = 789 }],
        Sides = [new LevelSide { MidTextureOffsetX = 9 }],
    });

    [Fact]
    public void CurrentArchiveRestoresAllPlaneAndWallFieldsExactly()
    {
        var sim = Room(); var expected = sim.CaptureState();
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(15, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Equal(expected.Planes, state.Planes);
        sim.Level.Sectors[0].FloorTextureAngle = 0;
        sim.Level.Sectors[0].CeilingTextureScaleY = 10;
        sim.Level.Sides[0].MidTextureOffsetX = 0;
        sim.RestoreState(state);
        Assert.Equal(expected.Planes, sim.CaptureState().Planes);
        Assert.Equal(expected.Walls, sim.CaptureState().Walls);
    }

    [Fact]
    public void VersionSevenLeavesPlaneStateUntouched()
    {
        var sim = Room(); var captured = sim.CaptureState();
        var bytes = SimSavegame.Write(new SimSaveState { Walls = captured.Walls });
        Assert.Equal(7, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Null(state.Planes);
        sim.RestoreState(state);
        Assert.Equal(captured.Planes, sim.CaptureState().Planes);
    }

    [Fact]
    public void TruncationsAndNonfinitePlanesAreRejected()
    {
        var bytes = SimSavegame.Write(Room());
        for (var length = 0; length < bytes.Length; length++)
            Assert.False(SimSavegame.TryRead(bytes.AsSpan(0, length), out _, out _));
        var healthSize = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(bytes.Length - healthSize - 100), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-plane-nonfinite", error);
    }

    [Fact]
    public void CountMismatchFailsBeforeWallsOrClockChange()
    {
        var sim = Room(); var before = sim.CaptureState();
        var wall = before.Walls![0] with { MidX = 99 };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(new SimSaveState { Tic = 99, Walls = [wall], Planes = [] }));
        Assert.Equal(before.Tic, sim.Thinkers.Clock.Tic);
        Assert.Equal(before.Walls, sim.CaptureState().Walls);
    }
}
