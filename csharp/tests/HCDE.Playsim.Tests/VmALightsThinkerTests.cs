namespace HCDE.Playsim.Tests;

public class VmALightsThinkerTests
{
    [Fact]
    public void ResolveLightLevel_ClampsToMin()
    {
        Assert.Equal(32, VmALightsFireFlicker.ResolveLightLevel(40, 16, 32));
    }

    [Fact]
    public void FireFlickerFromSector_UsesMinPlus16()
    {
        var (max, min) = VmALightsConstruct.FireFlickerFromSector(100, 20);
        Assert.Equal((100, 36), (max, min));
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

    [Fact]
    public void GlowTick_ReversesAtMinLight()
    {
        var (light, dir) = VmALightsGlow.Tick(-1, 40, 36, 100);
        Assert.Equal(1, dir);
        Assert.Equal(40, light);
    }

    [Fact]
    public void Glow2Interpolates_MidCycle()
    {
        Assert.Equal(150, VmALightsGlow2.InterpolateLight(100, 200, 5, 10));
    }

    [Fact]
    public void PhasedTick_RampsDuringFirstHalf()
    {
        Assert.True(VmALightsPhased.TickLightLevel(6, 100) > 100);
    }

    [Fact]
    public void PhasedNextPhase_WrapsAtZero()
    {
        Assert.Equal(63, VmALightsPhased.NextPhase(0));
    }

    [Fact]
    public void Glow2Tick_SwapsEndpointsWhenRepeating()
    {
        var (_, light, tics, start, end) = VmALightsGlow2.Tick(9, 10, 100, 200, false);
        Assert.Equal(200, start);
        Assert.Equal(100, end);
        Assert.Equal(0, tics);
        Assert.Equal(200, light);
    }
}
