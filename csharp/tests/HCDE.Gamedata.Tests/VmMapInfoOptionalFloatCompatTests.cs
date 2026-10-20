namespace HCDE.Gamedata.Tests;

public class VmMapInfoOptionalFloatCompatTests
{
    [Fact]
    public void TryParseLegacyValue_PromotesNumberToFloat()
    {
        var r = VmMapInfoOptionalFloatCompat.TryParseLegacyValue(false, 0, true, 42);
        Assert.True(r.Matched);
        Assert.Equal(42, r.Value);
    }

    [Fact]
    public void ShouldEmitCompatWarning_OnlyOnce()
    {
        var warned = false;
        Assert.True(VmMapInfoOptionalFloatCompat.ShouldEmitCompatWarning(ref warned));
        Assert.False(VmMapInfoOptionalFloatCompat.ShouldEmitCompatWarning(ref warned));
    }

    [Fact]
    public void ApplyHexenSkySpeedScale_DividesWhenHack()
    {
        Assert.Equal(1, VmMapInfoOptionalFloatCompat.ApplyHexenSkySpeedScale(256, true));
    }

    [Fact]
    public void SkySpeedFromTics_ScalesByTicRate()
    {
        Assert.Equal(3.5f, VmMapInfoOptionalFloatCompat.SkySpeedFromTics(100));
    }
}
