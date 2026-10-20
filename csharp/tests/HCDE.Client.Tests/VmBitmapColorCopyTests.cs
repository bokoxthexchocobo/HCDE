namespace HCDE.Client.Tests;

public class VmBitmapColorCopyTests
{
    [Fact]
    public void CopyRgbToBgra_None_WritesBgraOrder()
    {
        Span<byte> output = stackalloc byte[4];
        VmBitmapColorCopy.CopyRgbToBgra(output, [10, 20, 30], 1, 3, VmBitmapBlendMode.None);
        Assert.Equal(new byte[] { 30, 20, 10, 255 }, output.ToArray());
    }

    [Fact]
    public void CopyRgbToBgra_IceMap_UsesHexenIcePalette()
    {
        Span<byte> output = stackalloc byte[4];
        VmBitmapColorCopy.CopyRgbToBgra(output, [128, 128, 128], 1, 3, VmBitmapBlendMode.IceMap);
        Assert.Equal(VmBitmapColorCopy.IcePaletteBlue[8], output[0]);
        Assert.Equal(VmBitmapColorCopy.IcePaletteGreen[8], output[1]);
        Assert.Equal(VmBitmapColorCopy.IcePaletteRed[8], output[2]);
    }
}
