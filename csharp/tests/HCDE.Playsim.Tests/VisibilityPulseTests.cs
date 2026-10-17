using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VisibilityPulseTests
{
    [Theory]
    [InlineData(0.5, 1, 0.53125, 1)]
    [InlineData(0.5, 7, 0.53125, 7)]
    [InlineData(0.5, 0, 0.46875, 0)]
    [InlineData(0.5, -1, 0.46875, -1)]
    [InlineData(0.96875, 1, 1, -1)]
    [InlineData(1.5, 1, 1, -1)]
    [InlineData(0.28125, -1, 0.25, 1)]
    [InlineData(0.1, 0, 0.25, 1)]
    [InlineData(0, 1, 0.03125, 1)]
    [InlineData(2, -1, 1.96875, -1)]
    public void TickUsesNativeStepSignsAndDirectionalClamps(double initial, int direction, double expected, int expectedDirection)
    {
        var (_, actor) = Room(); actor.VisibilityPulse = true; actor.Alpha = initial; actor.VisibilityDirection = direction;
        actor.Tick(); Assert.Equal(expected, actor.Alpha); Assert.Equal(expectedDirection, actor.VisibilityDirection);
    }

    [Fact]
    public void CompleteCycleTakesFortyEightTicksAndDoesNotModifyRenderStyle()
    {
        var (_, actor) = Room(); actor.VisibilityPulse = true;
        var style = actor.RenderStyle;
        for (var i = 0; i < 24; i++) actor.Tick();
        Assert.Equal(0.25, actor.Alpha); Assert.Equal(1, actor.VisibilityDirection);
        for (var i = 0; i < 24; i++) actor.Tick();
        Assert.Equal(1, actor.Alpha); Assert.Equal(-1, actor.VisibilityDirection); Assert.Equal(style, actor.RenderStyle);
    }

    [Fact]
    public void DisabledEffectRetainsAlphaAndDirectionAndResumes()
    {
        var (_, actor) = Room(); actor.VisibilityPulse = true; actor.Tick(); actor.VisibilityPulse = false;
        var alpha = actor.Alpha; var direction = actor.VisibilityDirection;
        actor.Tick(); Assert.Equal(alpha, actor.Alpha); Assert.Equal(direction, actor.VisibilityDirection);
        actor.VisibilityPulse = true; actor.Tick(); Assert.Equal(alpha - 1.0 / 32, actor.Alpha);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnabledOrDisabledMidcycleStateRoundTripsAndOldSavesResetIt(bool enabled)
    {
        var (sim, actor) = Room(); var old = SimSavegame.Write(sim, true);
        actor.VisibilityPulse = enabled; actor.VisibilityDirection = -3; actor.Alpha = 0.625;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(330, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.VisibilityPulse = !enabled; actor.VisibilityDirection = 0; actor.Alpha = 1;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(enabled, actor.VisibilityPulse); Assert.Equal(-3, actor.VisibilityDirection); Assert.Equal(0.625, actor.Alpha);
        Assert.Equal(checksum, sim.Checksum); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, old); Assert.False(actor.VisibilityPulse); Assert.Equal(0, actor.VisibilityDirection);
    }

    [Fact]
    public void ContinuedPulseCycleMatchesAfterSaveRestore()
    {
        var (sim, actor) = Room(); actor.VisibilityPulse = true;
        for (var i = 0; i < 19; i++) actor.Tick();
        var bytes = SimSavegame.Write(sim, true);
        (double Alpha, int Direction) Next() { actor.Tick(); return (actor.Alpha, actor.VisibilityDirection); }
        var expected = Enumerable.Range(0, 60).Select(_ => Next()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(expected, Enumerable.Range(0, 60).Select(_ => Next()).ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothEffectAndDirectionAffectChecksum(bool direction)
    {
        var (sim, actor) = Room(); sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        if (direction) actor.VisibilityDirection = -1; else actor.VisibilityPulse = true;
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidSavedEffectIsRejected(int value)
    {
        var (sim, actor) = Room(); actor.VisibilityPulse = true; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-visibilitypulse-value", error);
    }

    [Theory]
    [InlineData(-129)]
    [InlineData(128)]
    public void VisibilityDirectionUsesNativeSignedByteRange(int value)
    {
        var (sim, actor) = Room();
        Assert.Throws<ArgumentOutOfRangeException>(() => actor.VisibilityDirection = value);
        actor.VisibilityPulse = true; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 12), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-visibilitypulse-value", error);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [] });
        var actor = sim.AddBot(0, 0); actor.Brain!.Enabled = false; actor.NoGravity = true;
        return (sim, actor);
    }
}
