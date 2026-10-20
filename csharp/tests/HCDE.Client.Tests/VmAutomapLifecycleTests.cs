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
    public void ComputeDrawerDeltaSeconds_UsesMilliseconds()
    {
        Assert.Equal(0.016, VmAutomapLifecycle.ComputeDrawerDeltaSeconds(32, 16));
    }

    [Fact]
    public void ShouldApplyPanWhenNotFollowing()
    {
        Assert.True(VmAutomapLifecycle.ShouldApplyPanWhenNotFollowing(false));
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
}
