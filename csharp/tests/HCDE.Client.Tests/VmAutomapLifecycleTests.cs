namespace HCDE.Client.Tests;

public class VmAutomapLifecycleTests
{
    [Fact]
    public void ComputeShowAllEnabled_UsesPercentageThreshold()
    {
        Assert.True(VmAutomapLifecycle.ComputeShowAllEnabled(50, 10, 5));
    }

    [Fact]
    public void ToggleBigState_Flips()
    {
        Assert.True(VmAutomapLifecycle.ToggleBigState(false));
    }

    [Fact]
    public void ShouldSaveScaleOnGoBig()
    {
        Assert.True(VmAutomapLifecycle.ShouldSaveScaleOnGoBig(true));
    }

    [Fact]
    public void ComputeMapCenter_FromExtents()
    {
        var (x, y) = VmAutomapLifecycle.ComputeMapCenter(10, 20, 4, 6);
        Assert.Equal(12, x);
        Assert.Equal(23, y);
    }

    [Fact]
    public void ShouldRequireAutomapForClearMarks()
    {
        Assert.False(VmAutomapLifecycle.ShouldRequireAutomapForClearMarks(true, false));
    }

    [Fact]
    public void ShouldRequireAutomapForGoBigCcmd()
    {
        Assert.True(VmAutomapLifecycle.ShouldRequireAutomapForGoBigCcmd(true, true));
    }

    [Fact]
    public void ResetFollowOldLocationX_IsMaxValue()
    {
        Assert.Equal(double.MaxValue, VmAutomapLifecycle.ResetFollowOldLocationX());
    }

    [Fact]
    public void ShouldDeactivateAutomapOnStop()
    {
        Assert.True(VmAutomapLifecycle.ShouldDeactivateAutomapOnStop());
    }

    [Fact]
    public void ShouldRejectToggleWhenNoAutomap()
    {
        Assert.True(VmAutomapLifecycle.ShouldRejectToggleWhenNoAutomap(false, false));
    }

    [Fact]
    public void ShouldClearPanButtonsOnOpen()
    {
        Assert.True(VmAutomapLifecycle.ShouldClearPanButtonsOnOpen(true));
    }
}
