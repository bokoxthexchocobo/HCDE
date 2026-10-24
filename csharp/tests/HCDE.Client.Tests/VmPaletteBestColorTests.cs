namespace HCDE.Client.Tests;

public class VmPaletteBestColorTests
{
    [Fact]
    public void BestColor_PicksClosestRgb()
    {
        uint[] palette = [0x00000000, 0x000000FF, 0x0000FF00];
        Assert.Equal(1, VmPaletteBestColor.BestColor(palette, 255, 0, 0, 0, palette.Length));
    }
}
