namespace HCDE.Playsim.Tests;

public class VmALightsThinkerTests
{
    [Fact]
    public void ResolveLightLevel_ClampsToMin()
    {
        Assert.Equal(32, VmALightsFireFlicker.ResolveLightLevel(40, 16, 32));
    }

    [Fact]
    public void FlickerInitialCount_UsesPrMask()
    {
        Assert.Equal(65, VmALightsFlicker.InitialCount(64));
    }

    [Fact]
    public void NextCountWhenDark_MasksMinTime()
    {
        Assert.Equal(8, VmALightsLightFlash.NextCountWhenDark(7, 7));
    }

    [Fact]
    public void ResolveMinLightWhenEqual_Zeroes()
    {
        Assert.Equal(0, VmALightsStrobe.ResolveMinLightWhenEqual(128, 128));
    }
}
