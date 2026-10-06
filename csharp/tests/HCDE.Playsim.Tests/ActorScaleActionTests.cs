using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorScaleActionTests
{
    [Theory]
    [InlineData(2, 0, false, 2)]
    [InlineData(2, 0, true, 0)]
    [InlineData(-2, -3, false, -3)]
    [InlineData(0, 0, false, 0)]
    [InlineData(1, 1, false, 1)]
    public void ScaleSemanticsAndSaveRoundTrip(double x, double y, bool useZero, double expectedY)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var radius = actor.Radius; var height = actor.Height;
        ActorPropertyActions.SetScale(actor, x, y, useZero: useZero);
        Assert.Equal(x, actor.ScaleX); Assert.Equal(expectedY, actor.ScaleY);
        Assert.Equal(radius, actor.Radius); Assert.Equal(height, actor.Height);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.ScaleX = actor.ScaleY = 9; sim.RestoreState(state);
        Assert.Equal(x, actor.ScaleX); Assert.Equal(expectedY, actor.ScaleY);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void FrameActionScalesTargetAndNullSelectionIsNoop()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var target = sim.AddBot(100, 0, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7);
        caller.States.Configure(caller, [new(-1, 0), new(-1, 0, Action: self =>
            ActorPropertyActions.SetScale(self, 2, 3, AcsActorPointer.Target))], 0);
        caller.States.Enter(caller, 1);
        ActorPropertyActions.SetScale(target, 9, pointerSelector: AcsActorPointer.Null);
        Assert.Equal(2, target.ScaleX); Assert.Equal(3, target.ScaleY); Assert.Equal(1, caller.ScaleX);
    }

    [Fact]
    public void DirectOverrideIsSavedAndChecksumTracksScale()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var other = Room(); other.AddBot(0, 0).Brain = null;
        actor.ScaleX = 1.23456789; actor.ScaleY = -0.25;
        sim.Tick(); other.Tick(); Assert.NotEqual(sim.Checksum, other.Checksum);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.ScaleX = actor.ScaleY = 1; sim.RestoreState(state);
        Assert.Equal(1.23456789, actor.ScaleX); Assert.Equal(-0.25, actor.ScaleY);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void InvalidInputsAndMemoryFailBeforeMutation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.SetScale(actor, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.SetScale(actor, 2, double.PositiveInfinity));
        Assert.Equal(1, actor.ScaleX); Assert.Equal(1, actor.ScaleY);
        var state = sim.CaptureState(); state.Actors.Single().Scale = new(1, double.NaN); actor.Health = 42;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(42, actor.Health);
    }

    [Theory]
    [InlineData(0, 77)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var bytes = Saved(); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(Start(bytes) + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-scale-", error);
    }

    [Theory]
    [InlineData(12)]
    [InlineData(20)]
    public void NonfiniteWireScaleIsRejected(int offset)
    {
        var bytes = Saved(); BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(Start(bytes) + offset), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-scale-value", error);
    }

    [Fact]
    public void LegacyAbsentRecordRestoresDefaultScale()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.ScaleX = 3; sim.RestoreState(state); Assert.Equal(1, actor.ScaleX);
    }

    private static int Start(byte[] bytes) => bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
    private static byte[] Saved()
    { var sim = Room(); ActorPropertyActions.SetScale(sim.AddBot(0, 0), 2); return SimSavegame.Write(sim); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
