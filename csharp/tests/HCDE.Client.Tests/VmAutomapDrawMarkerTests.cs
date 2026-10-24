namespace HCDE.Client.Tests;

public class VmAutomapDrawMarkerTests
{
    [Fact]
    public void NormalizeHorizontalFlip_FlipsWhenNegativeScale()
    {
        var (flip, scale) = VmAutomapDrawMarker.NormalizeHorizontalFlip(false, -2);
        Assert.True(flip);
        Assert.Equal(2, scale);
    }

    [Fact]
    public void MapToFramebuffer_AppliesFrameOffset()
    {
        var (x, y) = VmAutomapDrawMarker.MapToFramebuffer(1, 2, 3, 10, 20, v => (int)v, v => (int)v);
        Assert.Equal(11, x);
        Assert.Equal(25, y);
    }
}
