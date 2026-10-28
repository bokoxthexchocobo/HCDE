using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmQoiTextureImageTests
{
    [Fact]
    public void TryCreate_ValidatesQoiHeader()
    {
        var lump = BuildMinimalQoi();
        Assert.True(VmQoiTextureImage.TryCreate(lump, out var header));
        Assert.Equal(1, header.Width);
        Assert.Equal(1, header.Height);
    }

    [Fact]
    public void CreatePalettedPixels_DecodesSingleOpaquePixel()
    {
        var lump = BuildMinimalQoi();
        Assert.True(VmQoiTextureImage.TryCreate(lump, out var header));
        var pixels = VmQoiTextureImage.CreatePalettedPixels(lump, header);
        Assert.Single(pixels);
    }

    private static byte[] BuildMinimalQoi()
    {
        var lump = new byte[22];
        "qoif"u8.CopyTo(lump);
        BinaryPrimitives.WriteInt32BigEndian(lump.AsSpan(4), 1);
        BinaryPrimitives.WriteInt32BigEndian(lump.AsSpan(8), 1);
        lump[12] = 4;
        lump[13] = 0;
        lump[14] = 0xfe;
        lump[15] = 10;
        lump[16] = 20;
        lump[17] = 30;
        lump[18] = 0;
        lump[19] = 0;
        lump[20] = 0;
        lump[21] = 1;
        return lump;
    }
}
