namespace HCDE.Playsim.Tests;

public class VmPMobjWaterLevelRefreshTests
{
    [Fact]
    public void UpdateWaterLevel_RunsSplashWhenRequested()
    {
        var splashed = false;
        Assert.False(VmPMobjWaterLevelRefresh.UpdateWaterLevel(true, false, 0,
            () => splashed = true, null, () => { }));
        Assert.True(splashed);
    }

    [Fact]
    public void PlayDiveOrSurfaceSounds_InvokesVirtual()
    {
        var called = false;
        VmPMobjWaterLevelRefresh.CallPlayDiveOrSurfaceSounds(true, () => called = true);
        Assert.True(called);
    }
}
