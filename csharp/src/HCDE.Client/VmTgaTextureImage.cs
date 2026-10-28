using System.Buffers.Binary;

namespace HCDE.Client;

/// <summary>Native <c>tgatexture.cpp</c> TGA decode (paletted + RGB paths).</summary>
public static class VmTgaTextureImage
{
    public const int HeaderSize = 18;

    public static bool TryCreate(ReadOnlySpan<byte> lump, out VmTgaHeader header, out bool masked)
    {
        masked = false;
        if (!TryParseHeader(lump, out header))
            return false;
        masked = (header.ImageDescriptor & 15) == 8 &&
                 (header.BitsPerPixel == 32 || (header.ImageType == 1 && header.ColorMapEntrySize == 32));
        return true;
    }

    public static byte[] CreatePalettedPixels(ReadOnlySpan<byte> lump, VmTgaHeader header, bool luminanceConversion = false)
    {
        var type = header.ImageType & 7;
        return type switch
        {
            1 => DecodePaletted(lump, header, luminanceConversion),
            2 => header.BitsPerPixel switch
            {
                15 or 16 => DecodeRgb16(lump, header, luminanceConversion),
                24 => DecodeRgb24(lump, header, luminanceConversion),
                32 => DecodeRgb32(lump, header, luminanceConversion),
                _ => Array.Empty<byte>(),
            },
            3 => DecodeGrayscale(lump, header, luminanceConversion),
            _ => Array.Empty<byte>(),
        };
    }

    public static bool TryParseHeader(ReadOnlySpan<byte> lump, out VmTgaHeader header)
    {
        header = default;
        if (lump.Length < HeaderSize)
            return false;
        header = VmTgaHeader.Read(lump);
        if (lump[1] is not (0 or 1))
            return false;
        if (header.Width <= 0 || header.Height <= 0 || header.Width > 2048 || header.Height > 2048)
            return false;
        if (header.BitsPerPixel is not (8 or 15 or 16 or 24 or 32))
            return false;
        if (header.ImageType <= 0 || header.ImageType > 11)
            return false;
        if (header.ImageType is >= 4 and <= 8)
            return false;
        if ((header.ImageDescriptor & 16) != 0)
            return false;
        return true;
    }

    public static byte[] DecodePaletted(
        ReadOnlySpan<byte> lump,
        VmTgaHeader header,
        bool luminanceConversion,
        VmPaletteColorMatcher? picker = null)
    {
        picker ??= VmPaletteColorMatcher.Default;
        var paletteMap = ReadPaletteMap(lump, header, luminanceConversion, picker);
        var bytesPerPixel = header.BitsPerPixel >> 3;
        var pixelBytes = header.Width * header.Height * bytesPerPixel;
        var buffer = new byte[pixelBytes];
        var imageOffset = GetImageDataOffset(header);
        var source = lump[imageOffset..];
        if (header.ImageType < 4)
            source[..Math.Min(pixelBytes, source.Length)].CopyTo(buffer);
        else
            ReadCompressed(source, buffer, bytesPerPixel);

        var pixels = new byte[header.Width * header.Height];
        var pitch = header.Width * bytesPerPixel;
        var pointer = 0;
        if ((header.ImageDescriptor & 32) == 0)
        {
            pointer = (header.Height - 1) * pitch;
            pitch = -pitch;
        }

        for (var y = 0; y < header.Height; y++)
        {
            var row = pointer + y * pitch;
            for (var x = 0; x < header.Width; x++)
            {
                var index = buffer[row + x * bytesPerPixel];
                pixels[x * header.Height + y] = paletteMap[index];
            }
        }

        return pixels;
    }

    public static byte[] DecodeRgb16(ReadOnlySpan<byte> lump, VmTgaHeader header, bool luminanceConversion)
    {
        var buffer = ReadPixelBuffer(lump, header);
        var pixels = new byte[header.Width * header.Height];
        WalkOrientedPixels(header, buffer, 2, (x, y, offset) =>
        {
            var word = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset, 2));
            pixels[x * header.Height + y] = VmImageHelpers.RgbToPalette(
                luminanceConversion,
                (byte)(((word >> 10) & 0x1F) * 8),
                (byte)(((word >> 5) & 0x1F) * 8),
                (byte)((word & 0x1F) * 8));
        });
        return pixels;
    }

    public static byte[] DecodeRgb32(ReadOnlySpan<byte> lump, VmTgaHeader header, bool luminanceConversion)
    {
        var buffer = ReadPixelBuffer(lump, header);
        var pixels = new byte[header.Width * header.Height];
        var hasAlpha = (header.ImageDescriptor & 15) == 8;
        WalkOrientedPixels(header, buffer, 4, (x, y, offset) =>
        {
            pixels[x * header.Height + y] = VmImageHelpers.RgbToPalette(
                luminanceConversion,
                buffer[offset + 2],
                buffer[offset + 1],
                buffer[offset],
                hasAlpha ? buffer[offset + 3] : (byte)255);
        });
        return pixels;
    }

    public static byte[] DecodeGrayscale(ReadOnlySpan<byte> lump, VmTgaHeader header, bool luminanceConversion)
    {
        var buffer = ReadPixelBuffer(lump, header);
        var remap = VmImageHelpers.GetRemap(luminanceConversion, srcIsGrayscale: true);
        var pixels = new byte[header.Width * header.Height];
        var step = header.BitsPerPixel >> 3;
        WalkOrientedPixels(header, buffer, step, (x, y, offset) =>
        {
            var value = step == 2 ? buffer[offset + 1] : buffer[offset];
            pixels[x * header.Height + y] = remap[value];
        });
        return pixels;
    }

    public static byte[] DecodeRgb24(ReadOnlySpan<byte> lump, VmTgaHeader header, bool luminanceConversion)
    {
        var buffer = ReadPixelBuffer(lump, header);
        var pixels = new byte[header.Width * header.Height];
        WalkOrientedPixels(header, buffer, 3, (x, y, offset) =>
            pixels[x * header.Height + y] = VmImageHelpers.RgbToPalette(
                luminanceConversion, buffer[offset + 2], buffer[offset + 1], buffer[offset]));
        return pixels;
    }

    private static byte[] ReadPixelBuffer(ReadOnlySpan<byte> lump, VmTgaHeader header)
    {
        var bytesPerPixel = header.BitsPerPixel >> 3;
        var pixelBytes = header.Width * header.Height * bytesPerPixel;
        var buffer = new byte[pixelBytes];
        var source = lump[GetImageDataOffset(header)..];
        if (header.ImageType < 4)
            source[..Math.Min(pixelBytes, source.Length)].CopyTo(buffer);
        else
            ReadCompressed(source, buffer, bytesPerPixel);
        return buffer;
    }

    private static void WalkOrientedPixels(
        VmTgaHeader header,
        byte[] buffer,
        int bytesPerPixel,
        Action<int, int, int> visit)
    {
        var pitch = header.Width * bytesPerPixel;
        var pointer = (header.ImageDescriptor & 32) == 0 ? (header.Height - 1) * pitch : 0;
        if ((header.ImageDescriptor & 32) == 0)
            pitch = -pitch;
        for (var y = 0; y < header.Height; y++)
        {
            var row = pointer + y * pitch;
            for (var x = 0; x < header.Width; x++)
                visit(x, y, row + x * bytesPerPixel);
        }
    }

    private static int GetImageDataOffset(VmTgaHeader header)
    {
        var offset = HeaderSize + header.IdLength;
        if (header.HasColorMap)
            offset += header.ColorMapLength * ((header.ColorMapEntrySize + 7) >> 3);
        return offset;
    }

    private static byte[] ReadPaletteMap(
        ReadOnlySpan<byte> lump,
        VmTgaHeader header,
        bool luminanceConversion,
        VmPaletteColorMatcher picker)
    {
        var paletteMap = new byte[256];
        if (!header.HasColorMap)
            return paletteMap;
        var offset = HeaderSize + header.IdLength;
        for (var index = header.ColorMapFirst; index < header.ColorMapFirst + header.ColorMapLength && index < 256; index++)
        {
            byte r, g, b, a = 255;
            switch (header.ColorMapEntrySize)
            {
                case 15 or 16:
                    var word = BinaryPrimitives.ReadUInt16LittleEndian(lump[(offset + (index - header.ColorMapFirst) * 2)..]);
                    r = (byte)((word & 0x001F) << 3);
                    g = (byte)((word & 0x03E0) >> 2);
                    b = (byte)((word & 0x7C00) >> 7);
                    break;
                case 24:
                    var rgbOffset = offset + (index - header.ColorMapFirst) * 3;
                    b = lump[rgbOffset];
                    g = lump[rgbOffset + 1];
                    r = lump[rgbOffset + 2];
                    break;
                case 32:
                    var rgbaOffset = offset + (index - header.ColorMapFirst) * 4;
                    b = lump[rgbaOffset];
                    g = lump[rgbaOffset + 1];
                    r = lump[rgbaOffset + 2];
                    a = lump[rgbaOffset + 3];
                    break;
                default:
                    r = g = b = 0;
                    break;
            }

            paletteMap[index] = luminanceConversion
                ? (byte)(VmImageHelpers.Luminance(r, g, b) * a / 255)
                : picker.Pick(r, g, b);
        }

        return paletteMap;
    }

    private static void ReadCompressed(ReadOnlySpan<byte> source, Span<byte> buffer, int bytesPerPixel)
    {
        Span<byte> run = stackalloc byte[4];
        var remainingPixels = buffer.Length / bytesPerPixel;
        var writeOffset = 0;
        var readOffset = 0;
        while (remainingPixels > 0 && readOffset < source.Length)
        {
            var packet = source[readOffset++];
            if ((packet & 128) != 0)
            {
                packet &= 127;
                source[readOffset..(readOffset + bytesPerPixel)].CopyTo(run);
                readOffset += bytesPerPixel;
                var count = Math.Min(remainingPixels, packet + 1);
                for (var index = 0; index < count; index++)
                {
                    run[..bytesPerPixel].CopyTo(buffer[writeOffset..]);
                    writeOffset += bytesPerPixel;
                }

                remainingPixels -= count;
            }
            else
            {
                var count = Math.Min(remainingPixels, packet + 1);
                var byteCount = count * bytesPerPixel;
                source[readOffset..(readOffset + byteCount)].CopyTo(buffer[writeOffset..]);
                readOffset += byteCount;
                writeOffset += byteCount;
                remainingPixels -= count;
            }
        }
    }
}

public readonly struct VmTgaHeader
{
    public byte IdLength { get; init; }
    public bool HasColorMap { get; init; }
    public byte ImageType { get; init; }
    public short ColorMapFirst { get; init; }
    public short ColorMapLength { get; init; }
    public byte ColorMapEntrySize { get; init; }
    public short Width { get; init; }
    public short Height { get; init; }
    public byte BitsPerPixel { get; init; }
    public byte ImageDescriptor { get; init; }

    public static VmTgaHeader Read(ReadOnlySpan<byte> lump)
    {
        return new VmTgaHeader
        {
            IdLength = lump[0],
            HasColorMap = lump[1] == 1,
            ImageType = lump[2],
            ColorMapFirst = BinaryPrimitives.ReadInt16LittleEndian(lump[3..]),
            ColorMapLength = BinaryPrimitives.ReadInt16LittleEndian(lump[5..]),
            ColorMapEntrySize = lump[7],
            Width = BinaryPrimitives.ReadInt16LittleEndian(lump[12..]),
            Height = BinaryPrimitives.ReadInt16LittleEndian(lump[14..]),
            BitsPerPixel = lump[16],
            ImageDescriptor = lump[17],
        };
    }
}
