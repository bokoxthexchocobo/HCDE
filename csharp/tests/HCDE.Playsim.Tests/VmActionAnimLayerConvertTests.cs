namespace HCDE.Playsim.Tests;

public class VmActionAnimLayerConvertTests
{
    [Fact]
    public void UseModelAnimNoneWhenLayerMissing_WhenNoCurAnim() =>
        Assert.True(VmActionAnimLayerConvert.UseModelAnimNoneWhenLayerMissing(true, false));

    [Fact]
    public void TryBuildPrecalculatedPrev_RequiresTypes() =>
        Assert.True(VmActionAnimLayerConvert.TryBuildPrecalculatedPrev(true, true, true));

    [Fact]
    public void TryBuildInterpolatedPrev_RequiresTypes() =>
        Assert.True(VmActionAnimLayerConvert.TryBuildInterpolatedPrev(true, true, true));

    [Fact]
    public void ClearPrevAnimVariant_WhenUnsupportedPrev() =>
        Assert.True(VmActionAnimLayerConvert.ClearPrevAnimVariant(true, true, false, false));

    [Fact]
    public void InterpolatedPrevFields_PassesThrough() =>
        Assert.Equal((0.5f, 1, 2), VmActionAnimLayerConvert.InterpolatedPrevFields(0.5f, 1, 2));
}
