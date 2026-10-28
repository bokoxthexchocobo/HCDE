using System.Buffers.Binary;
using System.IO.Compression;

namespace HCDE.Client;

/// <summary>Native <c>m_png.cpp</c> <c>M_ReadIDAT</c> for grayscale and paletted PNGs.</summary>
internal static class VmPngIdatReader
{
    private static readonly byte[] PassWidthShift = [3, 3, 2, 2, 1, 1, 0, 0];
    private static readonly byte[] PassHeightShift = [3, 3, 3, 2, 2, 1, 1, 0];
    private static readonly byte[] PassRowOffset = [0, 0, 4, 0, 2, 0, 1, 0];
    private static readonly byte[] PassColOffset = [0, 4, 0, 2, 0, 1, 0, 0];

    public static bool TryReadGrayOrPalette(
        ReadOnlySpan<byte> lump,
        int idatOffset,
        Span<byte> pixels,
        int width,
        int height,
        byte bitDepth,
        byte colorType,
        byte interlace)
    {
        if (colorType is not (0 or 3))
            return false;
        if (!TryInflateIdat(lump, idatOffset, out var inflated))
            return false;

        return interlace == 0
            ? DecodeNonInterlaced(inflated, pixels, width, height, bitDepth)
            : DecodeInterlaced(inflated, pixels, width, height, bitDepth);
    }

    private static bool TryInflateIdat(ReadOnlySpan<byte> lump, int start, out byte[] inflated)
    {
        if (!TryCollectIdat(lump, start, out var compressed))
        {
            inflated = [];
            return false;
        }

        using var zlib = new ZLibStream(new MemoryStream(compressed, writable: false), CompressionMode.Decompress);
        using var output = new MemoryStream();
        zlib.CopyTo(output);
        inflated = output.ToArray();
        return inflated.Length > 0;
    }

    private static bool DecodeNonInterlaced(ReadOnlySpan<byte> data, Span<byte> pixels, int width, int height, byte bitDepth)
    {
        var rowBytes = (width * bitDepth + 7) / 8;
        var previous = new byte[rowBytes];
        var offset = 0;
        for (var y = 0; y < height; y++)
        {
            if (offset >= data.Length)
                return false;
            var filter = data[offset++];
            if (offset + rowBytes > data.Length)
                return false;
            var filtered = data.Slice(offset, rowBytes);
            offset += rowBytes;
            var packed = new byte[rowBytes];
            UnfilterRow(rowBytes, packed, filtered, previous, 1, filter);
            var expanded = new byte[width];
            if (bitDepth < 8)
                UnpackPixels(width, rowBytes, bitDepth, packed, expanded, true);
            else
                packed.AsSpan(0, width).CopyTo(expanded);
            expanded.AsSpan(0, width).CopyTo(pixels.Slice(y * width, width));
            filtered.CopyTo(previous);
        }

        return offset == data.Length;
    }

    private static bool DecodeInterlaced(ReadOnlySpan<byte> data, Span<byte> pixels, int width, int height, byte bitDepth)
    {
        var offset = 0;
        for (var pass = 0; pass < 7; pass++)
        {
            var rowOffset = PassRowOffset[pass];
            var colOffset = PassColOffset[pass];
            var passWidth = (width + (1 << PassWidthShift[pass]) - 1 - colOffset) >> PassWidthShift[pass];
            var passHeight = (height + (1 << PassHeightShift[pass]) - 1 - rowOffset) >> PassHeightShift[pass];
            if (passWidth == 0 || passHeight == 0)
                continue;
            var rowBytes = (passWidth * bitDepth + 7) / 8;
            var previous = new byte[rowBytes];
            for (var py = 0; py < passHeight; py++)
            {
                if (offset >= data.Length)
                    return false;
                var filter = data[offset++];
                if (offset + rowBytes > data.Length)
                    return false;
                var filtered = data.Slice(offset, rowBytes);
                offset += rowBytes;
                var packed = new byte[rowBytes];
                UnfilterRow(rowBytes, packed, filtered, previous, 1, filter);
                var expanded = new byte[passWidth];
                if (bitDepth < 8)
                    UnpackPixels(passWidth, rowBytes, bitDepth, packed, expanded, true);
                else
                    packed.AsSpan(0, passWidth).CopyTo(expanded);
                filtered.CopyTo(previous);
                var destY = rowOffset + py * (1 << PassHeightShift[pass]);
                var destXStep = 1 << PassWidthShift[pass];
                for (var px = 0; px < passWidth; px++)
                {
                    var destX = colOffset + px * destXStep;
                    pixels[destY * width + destX] = expanded[px];
                }
            }
        }

        return offset == data.Length;
    }

    private static bool TryCollectIdat(ReadOnlySpan<byte> lump, int start, out byte[] compressed)
    {
        using var builder = new MemoryStream();
        var offset = start;
        while (offset + 12 <= lump.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(lump[offset..]);
            var type = BinaryPrimitives.ReadInt32BigEndian(lump[(offset + 4)..]);
            offset += 8;
            if (offset + length + 4 > lump.Length)
            {
                compressed = [];
                return false;
            }

            if (type == 0x49444154)
                builder.Write(lump.Slice(offset, length));
            else if (type == 0x49454E44)
                break;
            offset += length + 4;
        }

        compressed = builder.ToArray();
        return compressed.Length > 0;
    }

    private static void UnfilterRow(int width, Span<byte> dest, ReadOnlySpan<byte> row, ReadOnlySpan<byte> prev, int bpp, byte filter)
    {
        switch (filter)
        {
            case 1:
                for (var index = 0; index < bpp; index++)
                    dest[index] = row[index];
                for (var index = bpp; index < width; index++)
                    dest[index] = (byte)(row[index] + dest[index - bpp]);
                break;
            case 2:
                for (var index = 0; index < width; index++)
                    dest[index] = (byte)(row[index] + prev[index]);
                break;
            case 3:
                for (var index = 0; index < bpp; index++)
                    dest[index] = (byte)(row[index] + prev[index] / 2);
                for (var index = bpp; index < width; index++)
                    dest[index] = (byte)(row[index] + (dest[index - bpp] + prev[index]) / 2);
                break;
            case 4:
                for (var index = 0; index < bpp; index++)
                    dest[index] = (byte)(row[index] + prev[index]);
                for (var index = bpp; index < width; index++)
                {
                    var a = dest[index - bpp];
                    var b = prev[index];
                    var c = prev[index - bpp];
                    var pa = b - c;
                    var pb = a - c;
                    var pc = Math.Abs(pa + pb);
                    pa = Math.Abs(pa);
                    pb = Math.Abs(pb);
                    var predictor = pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
                    dest[index] = (byte)(row[index] + predictor);
                }
                break;
            default:
                row[..width].CopyTo(dest);
                break;
        }
    }

    private static void UnpackPixels(int width, int bytesPerRow, int bitDepth, ReadOnlySpan<byte> rowIn, Span<byte> rowOut, bool _)
    {
        var outIndex = width;
        var inIndex = bytesPerRow;
        switch (bitDepth)
        {
            case 1:
                var lastByte = width & 7;
                if (lastByte != 0)
                {
                    inIndex--;
                    var pack = rowIn[inIndex];
                    outIndex -= lastByte;
                    for (var bit = 0; bit < lastByte; bit++)
                        rowOut[outIndex + bit] = (byte)((pack >> (7 - bit)) & 1);
                }

                while (inIndex-- > 0)
                {
                    var pack = rowIn[inIndex];
                    outIndex -= 8;
                    for (var bit = 0; bit < 8; bit++)
                        rowOut[outIndex + bit] = (byte)((pack >> (7 - bit)) & 1);
                }

                break;
            case 2:
                lastByte = width & 3;
                if (lastByte != 0)
                {
                    inIndex--;
                    var pack = rowIn[inIndex];
                    outIndex -= lastByte;
                    rowOut[outIndex] = (byte)(pack >> 6);
                    if (lastByte >= 2) rowOut[outIndex + 1] = (byte)((pack >> 4) & 3);
                    if (lastByte == 3) rowOut[outIndex + 2] = (byte)((pack >> 2) & 3);
                }

                while (inIndex-- > 0)
                {
                    var pack = rowIn[inIndex];
                    outIndex -= 4;
                    rowOut[outIndex] = (byte)(pack >> 6);
                    rowOut[outIndex + 1] = (byte)((pack >> 4) & 3);
                    rowOut[outIndex + 2] = (byte)((pack >> 2) & 3);
                    rowOut[outIndex + 3] = (byte)(pack & 3);
                }

                break;
            case 4:
                lastByte = width & 1;
                if (lastByte != 0)
                {
                    inIndex--;
                    outIndex--;
                    rowOut[outIndex] = (byte)(rowIn[inIndex] >> 4);
                }

                while (inIndex-- > 0)
                {
                    var pack = rowIn[inIndex];
                    outIndex -= 2;
                    rowOut[outIndex] = (byte)(pack >> 4);
                    rowOut[outIndex + 1] = (byte)(pack & 15);
                }

                break;
        }
    }
}
