using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmPcxTextureImageTests
{
    [Fact]
    public void TryCreate_ValidatesHeaderAndDimensions()
    {
        var lump = BuildPaletted2x2();
        Assert.True(VmPcxTextureImage.TryCreate(lump, out var header, out var width, out var height));
        Assert.Equal(2, width);
        Assert.Equal(2, height);
        Assert.Equal(2, header.BytesPerScanLine);
    }

    [Fact]
    public void DecodePaletted8_FlipsToColumnMajor()
    {
        var lump = BuildPaletted2x2();
        Assert.True(VmPcxTextureImage.TryCreate(lump, out var header, out _, out _));
        var pixels = VmPcxTextureImage.DecodePaletted8(lump, header);
        Assert.Equal(4, pixels.Length);
    }

    [Fact]
    public void CreatePalettedPixels_1Bit_ProducesFourPixels()
    {
        var header = new byte[VmPcxTextureImage.HeaderSize];
        header[0] = 10;
        header[2] = 1;
        header[3] = 1;
        header[65] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(10), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(66), 1);
        var lump = header.Concat(new byte[] { 0xFF, 0x00 }).ToArray();
        Assert.True(VmPcxTextureImage.TryParseHeader(lump, out var parsed));
        var pixels = VmPcxTextureImage.CreatePalettedPixels(lump, parsed);
        Assert.Equal(4, pixels.Length);
    }

    private static byte[] BuildPaletted2x2()
    {
        var header = new byte[VmPcxTextureImage.HeaderSize];
        header[0] = 10;
        header[1] = 5;
        header[2] = 1;
        header[3] = 8;
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4), 0);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(6), 0);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(10), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(66), 2);
        header[65] = 1;
        var pixels = new byte[] { 0, 1, 2, 3 };
        var paletteTail = new byte[769];
        paletteTail[0] = 0x0C;
        for (var index = 0; index < 256; index++)
        {
            paletteTail[1 + index * 3] = (byte)index;
            paletteTail[2 + index * 3] = (byte)index;
            paletteTail[3 + index * 3] = (byte)index;
        }

        return header.Concat(pixels).Concat(paletteTail).ToArray();
    }
}
