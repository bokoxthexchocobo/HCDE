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
}

public class VmAutomapInitColorsTests
{
    [Fact]
    public void ResolveAutoColorset_PicksDoomForDoomChex()
    {
        Assert.Equal(VmAutomapInitColors.ColorsetDoom,
            VmAutomapInitColors.ResolveAutoColorset(VmAutomapInitColors.ColorsetCustom, VmAutomapInitColors.GameDoomChex));
    }
}
