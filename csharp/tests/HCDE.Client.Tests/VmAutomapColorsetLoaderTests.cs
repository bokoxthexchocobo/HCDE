namespace HCDE.Client.Tests;

public class VmAutomapColorsetLoaderTests
{
    [Fact]
    public void DecodeTriplet_MarksUnusedAsInvalid()
    {
        var c = VmAutomapColorsetLoader.DecodeTriplet(1, 0, 0);
        Assert.False(c.IsValid);
    }

    [Fact]
    public void ComputeAlmostBackgroundRgb_WrapsNegativeChannels()
    {
        var rgb = VmAutomapColorsetLoader.ComputeAlmostBackgroundRgb(10, 10, 10);
        Assert.Equal(26, rgb.r);
    }

    [Fact]
    public void LineStyleToColorIndex_MapsOneSidedWall()
    {
        Assert.Equal(VmAutomapColorsetIndex.WallColor, VmAutomapColorsetLoader.LineStyleToColorIndex(1));
    }
}
