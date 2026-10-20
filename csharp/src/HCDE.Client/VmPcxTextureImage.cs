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

    public static byte[] CreatePalettedPixels(ReadOnlySpan<byte> lump, VmPcxHeader header, bool luminanceConversion = false)
    {
        var width = header.XMax - header.XMin + 1;
        var height = header.YMax - header.YMin + 1;
        var bitCount = header.BitsPerPixel * header.NumColorPlanes;
        var alphatex = luminanceConversion;
        if (bitCount < 24)
        {
            var pixels = new byte[width * height];
            if (bitCount < 8)
            {
                if (bitCount == 1)
                {
                    ReadPcx1Bit(pixels, lump, header, width, height);
                    return FlipWithMap(pixels, width, height, [0, 255]);
                }

                var paletteMap = new byte[16];
                var embedded = header.EmbeddedPalette.Span;
                for (var index = 0; index < 16; index++)
                    paletteMap[index] = VmImageHelpers.RgbToPalettePrecise(
                        alphatex, embedded[index * 3], embedded[index * 3 + 1], embedded[index * 3 + 2]);
                ReadPcx4Bits(pixels, lump, header, width, height);
                return FlipWithMap(pixels, width, height, paletteMap);
            }

            var palette256 = ReadPalette256(lump, alphatex);
            ReadPcx8Bits(pixels, lump, header, width, height);
            return FlipWithMap(pixels, width, height, palette256);
        }

        var rgb = new byte[width * height * 3];
        ReadPcx24Bits(rgb, lump, header, width, height, planes: 3);
        var output = new byte[width * height];
        var row = 0;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++, row++)
            output[row] = VmImageHelpers.RgbToPalette(alphatex, rgb[row * 3], rgb[row * 3 + 1], rgb[row * 3 + 2]);
        return output;
    }

    public static byte[] DecodePaletted8(ReadOnlySpan<byte> lump, VmPcxHeader header) =>
        CreatePalettedPixels(lump, header);

    private static byte[] FlipWithMap(byte[] pixels, int width, int height, ReadOnlySpan<byte> paletteMap)
    {
        if (width == height)
        {
            VmImageHelpers.FlipSquareBlockRemap(pixels, width, paletteMap);
            return pixels;
        }

        var flipped = new byte[pixels.Length];
        VmImageHelpers.FlipNonSquareBlockRemap(flipped, pixels, width, height, width, paletteMap);
        return flipped;
    }

    private static ReadOnlySpan<byte> PixelSource(ReadOnlySpan<byte> lump) => lump[HeaderSize..];

    private static void ReadPcx1Bit(byte[] pixels, ReadOnlySpan<byte> lump, VmPcxHeader header, int width, int height)
    {
        var source = PixelSource(lump);
        var rleCount = 0;
        var rleValue = 0;
        for (var y = 0; y < height; y++)
        {
            var bytes = header.BytesPerScanLine;
            var column = 0;
            while (bytes-- > 0)
            {
                if (rleCount == 0)
                {
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
                for (var bit = 7; bit >= 0; bit--)
                {
                    if (column < width)
                        pixels[y * width + column++] = (byte)((rleValue & (1 << bit)) > 0 ? 1 : 0);
                }
            }
        }
    }

    private static void ReadPcx4Bits(byte[] pixels, ReadOnlySpan<byte> lump, VmPcxHeader header, int width, int height)
    {
        var source = PixelSource(lump);
        var line = new byte[header.BytesPerScanLine];
        for (var y = 0; y < height; y++)
        {
            Array.Clear(pixels, y * width, width);
            for (var plane = 0; plane < 4; plane++)
            {
                var rleCount = 0;
                var rleValue = 0;
                var bytes = header.BytesPerScanLine;
                var lineOffset = 0;
                while (bytes-- > 0)
                {
                    if (rleCount == 0)
                    {
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
                    line[lineOffset++] = (byte)rleValue;
                }

                for (var x = 0; x < width; x++)
                {
                    if ((line[x / 8] & (128 >> (x % 8))) != 0)
                        pixels[y * width + x] += (byte)(1 << plane);
                }
            }
        }
    }

    private static void ReadPcx8Bits(byte[] pixels, ReadOnlySpan<byte> lump, VmPcxHeader header, int width, int height)
    {
        var source = PixelSource(lump);
        for (var y = 0; y < height; y++)
        {
            var ptr = y * width;
            var bytes = header.BytesPerScanLine;
            var rleCount = 0;
            var rleValue = 0;
            var column = 0;
            while (bytes-- > 0)
            {
                if (rleCount == 0)
                {
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
                    pixels[ptr + column++] = (byte)rleValue;
            }
        }
    }

    private static void ReadPcx24Bits(byte[] pixels, ReadOnlySpan<byte> lump, VmPcxHeader header, int width, int height, int planes)
    {
        var source = PixelSource(lump);
        for (var y = 0; y < height; y++)
        {
            var rowBase = y * width * planes;
            for (var plane = 0; plane < planes; plane++)
            {
                var pointer = rowBase + plane;
                var bytes = header.BytesPerScanLine;
                var rleCount = 0;
                var rleValue = 0;
                while (bytes-- > 0)
                {
                    if (rleCount == 0)
                    {
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
                    pixels[pointer] = (byte)rleValue;
                    pointer += planes;
                }
            }
        }
    }

    private static byte[] ReadPalette256(ReadOnlySpan<byte> lump, bool alphatex)
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
            palette[index] = VmImageHelpers.RgbToPalettePrecise(
                alphatex, lump[offset], lump[offset + 1], lump[offset + 2]);
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
    public ReadOnlyMemory<byte> EmbeddedPalette { get; init; }

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
            EmbeddedPalette = lump[16..64].ToArray(),
        };
    }
}
