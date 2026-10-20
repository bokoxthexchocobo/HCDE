using System.Buffers.Binary;

namespace HCDE.Client;

/// <summary>Native <c>pngtexture.cpp</c> header validation and paletted/grayscale decode.</summary>
public static class VmPngTextureImage
{
    private const uint SigPart1 = 0x89504E47;
    private const uint SigPart2 = 0x0D0A1A0A;
    private const uint IhdrLength = 13;
    private const uint IhdrType = 0x49484452;

    public static bool TryCreate(ReadOnlySpan<byte> lump, out VmPngInfo info)
    {
        if (lump.Length < 33)
        {
            info = default;
            return false;
        }
        if (BinaryPrimitives.ReadUInt32BigEndian(lump) != SigPart1)
        {
            info = default;
            return false;
        }
        if (BinaryPrimitives.ReadUInt32BigEndian(lump[4..]) != SigPart2)
        {
            info = default;
            return false;
        }
        if (BinaryPrimitives.ReadUInt32BigEndian(lump[8..]) != IhdrLength)
        {
            info = default;
            return false;
        }
        if (BinaryPrimitives.ReadUInt32BigEndian(lump[12..]) != IhdrType)
        {
            info = default;
            return false;
        }

        var width = BinaryPrimitives.ReadInt32BigEndian(lump[16..]);
        var height = BinaryPrimitives.ReadInt32BigEndian(lump[20..]);
        var bitDepth = lump[24];
        var colorType = lump[25];
        var compression = lump[26];
        var filter = lump[27];
        var interlace = lump[28];

        if (compression != 0 || filter != 0 || interlace > 1)
        {
            info = default;
            return false;
        }
        if (((1 << colorType) & 0x5D) == 0)
        {
            info = default;
            return false;
        }
        if (((1 << bitDepth) & 0x116) == 0)
        {
            info = default;
            return false;
        }

        if (lump.Length >= 37)
        {
            var nextLen = BinaryPrimitives.ReadUInt32BigEndian(lump[29..]);
            if (nextLen == 0)
            {
                var nextType = lump.Length >= 37 ? BinaryPrimitives.ReadUInt32BigEndian(lump[33..]) : 0;
                if (nextType == 0x49454E44)
                {
                    info = default;
                    return false;
                }
            }
        }

        var idatOffset = FindFirstIdatOffset(lump);
        if (width <= 0 || height <= 0 || idatOffset <= 0)
        {
            info = default;
            return false;
        }

        info = new VmPngInfo
        {
            Width = width,
            Height = height,
            BitDepth = bitDepth,
            ColorType = colorType,
            Compression = compression,
            Filter = filter,
            Interlace = interlace,
            IdatOffset = idatOffset
        };
        return true;
    }

    public static byte[] CreatePalettedPixels(ReadOnlySpan<byte> lump, VmPngInfo info, bool luminanceConversion = false)
    {
        if (info.Interlace != 0 || info.BitDepth != 8 || info.ColorType is not (0 or 3))
            throw new NotSupportedException("Only 8-bit non-interlaced grayscale or paletted PNGs are ported.");

        var pixels = new byte[info.Width * info.Height];
        if (!VmPngIdatReader.TryRead8NonInterlaced(lump, info.IdatOffset, pixels, info.Width, info.Height, info.Width, info.ColorType))
            throw new InvalidDataException("Failed to decode PNG IDAT.");

        var alphatex = luminanceConversion;
        if (info.ColorType == 0)
        {
            if (info.Width == info.Height)
            {
                if (!alphatex)
                    VmImageHelpers.FlipSquareBlockRemap(pixels, info.Width, VmImageHelpers.GetRemap(false, true));
                else
                    VmImageHelpers.FlipSquareBlock(pixels, info.Width);
            }
            else if (!alphatex)
            {
                pixels = FlipNonSquareRemap(pixels, info.Width, info.Height, VmImageHelpers.GetRemap(false, true));
            }

            return pixels;
        }

        var paletteMap = BuildPaletteMap(lump, alphatex);
        if (info.Width == info.Height)
            VmImageHelpers.FlipSquareBlockRemap(pixels, info.Width, paletteMap);
        else
            pixels = FlipNonSquareRemap(pixels, info.Width, info.Height, paletteMap);
        return pixels;
    }

    private static byte[] FlipNonSquareRemap(ReadOnlySpan<byte> source, int width, int height, ReadOnlySpan<byte> remap)
    {
        var destination = new byte[width * height];
        VmImageHelpers.FlipNonSquareBlockRemap(destination, source, width, height, width, remap);
        return destination;
    }

    private static byte[] BuildPaletteMap(ReadOnlySpan<byte> lump, bool alphatex)
    {
        var palette = new byte[768];
        var paletteSize = 0;
        var offset = 33;
        while (offset + 12 <= lump.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(lump[offset..]);
            var type = BinaryPrimitives.ReadUInt32BigEndian(lump[(offset + 4)..]);
            offset += 8;
            if (type == 0x504C5445)
            {
                var count = Math.Min(length / 3, 256);
                lump.Slice(offset, count * 3).CopyTo(palette);
                paletteSize = count;
                break;
            }

            if (type == 0x49444154 || type == 0x49454E44)
                break;
            offset += length + 4;
        }

        if (paletteSize == 0)
            throw new InvalidDataException("Paletted PNG missing PLTE.");

        var map = new byte[256];
        for (var index = 0; index < paletteSize; index++)
        {
            map[index] = VmImageHelpers.RgbToPalettePrecise(
                alphatex, palette[index * 3], palette[index * 3 + 1], palette[index * 3 + 2]);
        }

        return map;
    }

    private static int FindFirstIdatOffset(ReadOnlySpan<byte> lump)
    {
        var offset = 8;
        while (offset + 12 <= lump.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(lump[offset..]);
            var type = BinaryPrimitives.ReadUInt32BigEndian(lump[(offset + 4)..]);
            if (type == 0x49444154)
                return offset;
            if (type == 0x49454E44)
                return -1;
            offset += 8 + length + 4;
        }

        return -1;
    }
}

public readonly struct VmPngInfo
{
    public int Width { get; init; }
    public int Height { get; init; }
    public byte BitDepth { get; init; }
    public byte ColorType { get; init; }
    public byte Compression { get; init; }
    public byte Filter { get; init; }
    public byte Interlace { get; init; }
    public int IdatOffset { get; init; }
}
