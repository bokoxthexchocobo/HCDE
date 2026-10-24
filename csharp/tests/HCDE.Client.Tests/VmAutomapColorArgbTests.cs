namespace HCDE.Client.Tests;

public class VmAutomapColorArgbTests
{
    [Fact]
    public void FromRgb_PacksAlpha()
    {
        var c = VmAutomapColorArgb.FromRgb(1, 2, 3);
        Assert.True(c.IsValid);
        Assert.NotEqual(0u, c.Rgb);
    }

    [Fact]
    public void Invalid_IsNotValid()
    {
        Assert.False(VmAutomapColorArgb.Invalid.IsValid);
    }
}
