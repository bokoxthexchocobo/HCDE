namespace HCDE.Gamedata.Tests;

public class VmEMapInfoPropertyTailTests
{
    [Fact]
    public void IsNoFlatSky2_DetectsMarker()
    {
        Assert.True(VmEMapInfoPropertyTail.IsNoFlatSky2("-NOFLAT-"));
    }

    [Fact]
    public void SkySpeedFromDelta_ScalesByTicRate()
    {
        Assert.Equal(3.5f, VmEMapInfoPropertyTail.SkySpeedFromDelta(100));
    }

    [Fact]
    public void ApplyUnevenLight_ClearsWhenDisabled()
    {
        var v = 1;
        var h = 2;
        VmEMapInfoPropertyTail.ApplyUnevenLight(false, ref v, ref h);
        Assert.Equal(0, v);
        Assert.Equal(0, h);
    }
}
