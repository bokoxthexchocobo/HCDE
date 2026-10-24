namespace HCDE.Client.Tests;

public class VmBuildTextureImageTests
{
    [Fact]
    public void CreatePalettedPixels_AppliesRemapTable()
    {
        var raw = new byte[] { 0, 1, 2, 3 };
        var remap = new byte[] { 10, 20, 30, 40 };
        var pixels = VmBuildTextureImage.CreatePalettedPixels(raw, 2, 2, remap);
        Assert.Equal([10, 20, 30, 40], pixels);
    }
}
