using System.Buffers.Binary;

namespace HCDE.Client;

/// <summary>Native <c>pcxtexture.cpp</c>.</summary>
public static class VmPcxTextureImage
{
    public const int HeaderSize = 128;

    public static bool TryCreate(ReadOnlySpan<byte> lump, out VmPcxHeader header, out int width, out int height)
    {
        if (!TryParseHeader(lump, out header))
        {
            width = height = 0;
            return false;
        }

        width = header.XMax - header.XMin + 1;
        height = header.YMax - header.YMin + 1;
        return true;
    }

    public static bool TryParseHeader(ReadOnlySpan<byte> lump, out VmPcxHeader header)
    {
        header = default;
        if (lump.Length < HeaderSize)
            return false;
        header = VmPcxHeader.Read(lump);
        if (header.Manufacturer != 10 || header.Encoding != 1)
            return false;
        if (header.Version is not (0 or 2 or 3 or 4 or 5))
            return false;
        if (header.BitsPerPixel is not (1 or 4 or 8))
            return false;
        if (header.BitsPerPixel == 1 && header.NumColorPlanes is not (1 or 4))
            return false;
        if (header.BitsPerPixel == 8 &&
            header.BytesPerScanLine != ((header.XMax - header.XMin + 2) & ~1))
            return false;
        var padding = header.Padding.Span;
        for (var index = 0; index < padding.Length; index++)
        {
            if (padding[index] != 0)
                return false;
        }

        return true;
    }

    public static byte[] DecodePaletted8(ReadOnlySpan<byte> lump, VmPcxHeader header)
    {
        var width = header.XMax - header.XMin + 1;
        var height = header.YMax - header.YMin + 1;
        var pixels = new byte[width * height];
        var palette = ReadPalette256(lump);
        var source = lump[HeaderSize..];
        var row = 0;
        var column = 0;
        var rleCount = 0;
        var rleValue = 0;
        while (row < height)
        {
            if (column >= header.BytesPerScanLine)
            {
                row++;
                column = 0;
                if (row >= height)
                    break;
            }

            if (rleCount == 0)
            {
                if (source.Length == 0)
                    break;
                rleValue = source[0];
                source = source[1..];
                rleCount = rleValue < 0xC0 ? 1 : rleValue - 0xC0;
                if (rleValue >= 0xC0)
                {
                    rleValue = source[0];
                    source = source[1..];
                }
            }

            rleCount--;
            if (column < width)
                pixels[row * width + column] = palette[rleValue];
            column++;
        }

        if (width == height)
            VmImageHelpers.FlipSquareBlockRemap(pixels, width, VmImageHelpers.GetRemap(false));
        else
        {
            var flipped = new byte[pixels.Length];
            VmImageHelpers.FlipNonSquareBlockRemap(flipped, pixels, width, height, width, VmImageHelpers.GetRemap(false));
            pixels = flipped;
        }

        return pixels;
    }

    private static byte[] ReadPalette256(ReadOnlySpan<byte> lump)
    {
        var palette = new byte[256];
        if (lump.Length < 769)
            return palette;
        var start = lump.Length - 769;
        if (lump[start] != 0x0C)
            return palette;
        for (var index = 0; index < 256; index++)
        {
            var offset = start + 1 + index * 3;
            palette[index] = VmImageHelpers.RgbToPalettePrecise(false, lump[offset], lump[offset + 1], lump[offset + 2]);
        }

        return palette;
    }
}

public readonly struct VmPcxHeader
{
    public byte Manufacturer { get; init; }
    public byte Version { get; init; }
    public byte Encoding { get; init; }
    public byte BitsPerPixel { get; init; }
    public ushort XMin { get; init; }
    public ushort YMin { get; init; }
    public ushort XMax { get; init; }
    public ushort YMax { get; init; }
    public ushort BytesPerScanLine { get; init; }
    public byte NumColorPlanes { get; init; }
    public ReadOnlyMemory<byte> Padding { get; init; }

    public static VmPcxHeader Read(ReadOnlySpan<byte> lump)
    {
        return new VmPcxHeader
        {
            Manufacturer = lump[0],
            Version = lump[1],
            Encoding = lump[2],
            BitsPerPixel = lump[3],
            XMin = BinaryPrimitives.ReadUInt16LittleEndian(lump[4..]),
            YMin = BinaryPrimitives.ReadUInt16LittleEndian(lump[6..]),
            XMax = BinaryPrimitives.ReadUInt16LittleEndian(lump[8..]),
            YMax = BinaryPrimitives.ReadUInt16LittleEndian(lump[10..]),
            BytesPerScanLine = BinaryPrimitives.ReadUInt16LittleEndian(lump[66..]),
            NumColorPlanes = lump[65],
            Padding = lump[74..128].ToArray(),
        };
    }
}
