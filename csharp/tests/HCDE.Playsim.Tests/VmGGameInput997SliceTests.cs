namespace HCDE.Playsim.Tests;

public class VmGGameInput997SliceTests
{
    [Fact]
    public void TurboClampAndScale()
    {
        var low = VmGGameInput997Slice.ApplyTurboToMovementTables(
            turboPercent: 5f,
            normForward0: 100,
            normForward1: 200,
            normSide0: 50,
            normSide1: 75);
        Assert.Equal(10f, low.ClampedTurboPercent);
        Assert.False(low.AppliedNormScale);

        var scaled = VmGGameInput997Slice.ApplyTurboToMovementTables(
            turboPercent: 200f,
            normForward0: 100,
            normForward1: 100,
            normSide0: 50,
            normSide1: 50);
        Assert.True(scaled.AppliedNormScale);
        Assert.Equal(200, scaled.ForwardMove0);
        Assert.Equal(100, scaled.SideMove0);
    }

    [Fact]
    public void SaveCountClamps()
    {
        Assert.Equal(0, VmGGameInput997Slice.NormalizeAutosaveCount(-3));
        Assert.Equal(1, VmGGameInput997Slice.NormalizeQuicksaveRotationCount(0));
    }

    [Fact]
    public void SmoothMouseDelta()
    {
        Assert.Equal(0.0, VmGGameInput997Slice.SmoothMouseDelta(0.0, smoothCurve: 1));
        Assert.Equal(32.0, VmGGameInput997Slice.SmoothMouseDelta(32.0, smoothCurve: 0));
        var smoothed = VmGGameInput997Slice.SmoothMouseDelta(128.0, smoothCurve: 1);
        Assert.True(smoothed > 64.0);
    }

    [Fact]
    public void TurnspeedsConsoleFillRules()
    {
        var current = new VmGGameInput997Slice.TurnSpeedAssignment
        {
            WalkFast = 10,
            SprintFast = 20,
            WalkSlow = 5,
            SprintSlow = 5,
        };

        var oneArg = VmGGameInput997Slice.ApplyTurnspeedsConsoleArgs(current, [640]);
        Assert.Equal(640, oneArg.WalkFast);
        Assert.Equal(1280, oneArg.SprintFast);
        Assert.Equal(320, oneArg.WalkSlow);
        Assert.Equal(320, oneArg.SprintSlow);

        var fourArgs = VmGGameInput997Slice.ApplyTurnspeedsConsoleArgs(
            current,
            [1, 2, 3, 4]);
        Assert.Equal(4, fourArgs.SprintSlow);
    }
}
