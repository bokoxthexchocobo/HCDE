namespace HCDE.Client.Tests;

public class VmBitmapColorCopyTests
{
    [Fact]
    public void CopyRgbToBgra_None_WritesBgraOrder()
    {
        Span<byte> output = stackalloc byte[4];
        VmBitmapColorCopy.CopyRgbToBgra(output, [10, 20, 30], 1, 3);
        Assert.Equal(new byte[] { 30, 20, 10, 255 }, output.ToArray());
    }

    [Fact]
    public void CopyRgbToBgra_IceMap_UsesHexenIcePalette()
    {
        Span<byte> output = stackalloc byte[4];
        VmBitmapColorCopy.CopyRgbToBgra(output, [128, 128, 128], 1, 3, new VmBitmapCopyInfo { Blend = VmBitmapBlendMode.IceMap });
        Assert.Equal(VmBitmapColorCopy.IcePaletteBlue[8], output[0]);
        Assert.Equal(VmBitmapColorCopy.IcePaletteGreen[8], output[1]);
        Assert.Equal(VmBitmapColorCopy.IcePaletteRed[8], output[2]);
    }

    [Fact]
    public void CopyRgbToBgra_Desaturate_BlendsTowardGray()
    {
        Span<byte> output = stackalloc byte[4];
        VmBitmapColorCopy.CopyRgbToBgra(output, [200, 40, 40], 1, 3, new VmBitmapCopyInfo { Blend = VmBitmapBlendMode.Desaturate31 });
        Assert.True(output[2] < 200);
        Assert.True(output[2] > 40);
    }

    [Fact]
    public void CopyRgbToBgra_Modulate_ScalesChannels()
    {
        Span<byte> output = stackalloc byte[4];
        VmBitmapColorCopy.CopyRgbToBgra(output, [255, 128, 64], 1, 3,
            new VmBitmapCopyInfo { Blend = VmBitmapBlendMode.Modulate, BlendColor = [2048, 2048, 2048, 4096] });
        Assert.Equal(32, output[0]);
        Assert.Equal(64, output[1]);
        Assert.Equal(127, output[2]);
    }
}
