namespace HCDE.Gamedata.Tests;

public class VmMapInfoHcdeShadowMapOptionsTests
{
    [Fact]
    public void ApplyShadowProfile_ClampsHigh()
    {
        var o = new VmMapInfoHcdeShadowOverrides();
        VmMapInfoHcdeShadowMapOptions.ApplyShadowProfile(o, 99);
        Assert.Equal(7, o.ShadowProfile);
    }

    [Fact]
    public void ApplyShadowQualityCap_ClearsWhenNegative()
    {
        var o = new VmMapInfoHcdeShadowOverrides();
        VmMapInfoHcdeShadowMapOptions.ApplyShadowQualityCap(o, -5);
        Assert.Equal(-1, o.ShadowQualityCap);
    }

    [Fact]
    public void ApplyShadowMaxLightsCap_CapsAt1024()
    {
        var o = new VmMapInfoHcdeShadowOverrides();
        VmMapInfoHcdeShadowMapOptions.ApplyShadowMaxLightsCap(o, 5000);
        Assert.Equal(1024, o.ShadowMaxLightsCap);
    }
}
