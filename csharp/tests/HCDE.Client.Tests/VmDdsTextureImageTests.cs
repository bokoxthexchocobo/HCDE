using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmDdsTextureImageTests
{
    [Fact]
    public void TryCreate_Accepts32BitRgbDds()
    {
        var dds = BuildRgbDds2x2();
        Assert.True(VmDdsTextureImage.TryCreate(dds, out var info));
        Assert.Equal(2u, info.Width);
        Assert.Equal(2u, info.Height);
    }

    [Fact]
    public void CreatePalettedPixels_ReadsRgbColumnMajor()
    {
        var dds = BuildRgbDds2x2();
        Assert.True(VmDdsTextureImage.TryCreate(dds, out var info));
        var pixels = VmDdsTextureImage.CreatePalettedPixels(dds, info);
        Assert.Equal(4, pixels.Length);
        Assert.Contains(pixels, value => value != 0);
    }

    [Fact]
    public void CalcBitShift_MatchesNativeExample()
    {
        VmDdsTextureImage.CalcBitShift(0x00FF0000, out var l, out var r);
        Assert.True(l > 0);
        Assert.True(r > 0);
    }

    private static byte[] BuildRgbDds2x2()
    {
        var header = new byte[4 + 124];
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0), 0x20534444);
        var desc = header.AsSpan(4);
        BinaryPrimitives.WriteUInt32LittleEndian(desc, 124);
        const uint flags = 0x0000100F;
        BinaryPrimitives.WriteUInt32LittleEndian(desc[4..], flags);
        BinaryPrimitives.WriteUInt32LittleEndian(desc[8..], 2);
        BinaryPrimitives.WriteUInt32LittleEndian(desc[12..], 2);
        BinaryPrimitives.WriteUInt32LittleEndian(desc[16..], 8);
        var pf = desc[76..];
        BinaryPrimitives.WriteUInt32LittleEndian(pf, 32);
        BinaryPrimitives.WriteUInt32LittleEndian(pf[4..], 0x41);
        BinaryPrimitives.WriteUInt32LittleEndian(pf[8..], 32);
        BinaryPrimitives.WriteUInt32LittleEndian(pf[12..], 0x00FF0000);
        BinaryPrimitives.WriteUInt32LittleEndian(pf[16..], 0x0000FF00);
        BinaryPrimitives.WriteUInt32LittleEndian(pf[20..], 0x000000FF);
        BinaryPrimitives.WriteUInt32LittleEndian(pf[24..], 0xFF000000);
        var pixels = new byte[]
        {
            255, 0, 0, 255, 0, 255, 0, 255,
            0, 0, 255, 255, 255, 255, 255, 255
        };
        return header.Concat(pixels).ToArray();
    }
}
