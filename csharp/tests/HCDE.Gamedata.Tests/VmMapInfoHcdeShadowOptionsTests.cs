namespace HCDE.Gamedata.Tests;

public class VmMapInfoHcdeShadowOptionsTests
{
    [Fact]
    public void ClampShadowProfile_Bounds()
    {
        Assert.Equal(-1, VmMapInfoHcdeShadowOptions.ClampShadowProfile(-5));
        Assert.Equal(7, VmMapInfoHcdeShadowOptions.ClampShadowProfile(99));
    }

    [Fact]
    public void NormalizeShadowQualityCap_PicksNearestTier()
    {
        Assert.Equal(1024, VmMapInfoHcdeShadowOptions.NormalizeShadowQualityCap(900));
    }
}
