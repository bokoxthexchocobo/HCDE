using System.Buffers.Binary;
using System.IO.Compression;

namespace HCDE.Client;

/// <summary>Native <c>m_png.cpp</c> <c>M_ReadIDAT</c> (8-bit, non-interlaced subset).</summary>
internal static class VmPngIdatReader
{
    public static bool TryRead8NonInterlaced(
        ReadOnlySpan<byte> lump,
        int idatOffset,
        Span<byte> pixels,
        int width,
        int height,
        int pitch,
        byte colorType)
    {
        if (colorType is not (0 or 3))
            return false;
        if (!TryCollectIdat(lump, idatOffset, out var compressed))
            return false;
        using var zlib = new ZLibStream(new MemoryStream(compressed, writable: false), CompressionMode.Decompress);
        var rowBytes = width;
        var previous = new byte[rowBytes];
        var filtered = new byte[rowBytes];
        var destRow = 0;
        for (var y = 0; y < height; y++)
        {
            var filter = ReadByte(zlib);
            if (filter < 0)
                return false;
            if (ReadFully(zlib, filtered) != rowBytes)
                return false;
            var dest = pixels.Slice(destRow, rowBytes);
            UnfilterRow(rowBytes, dest, filtered, previous, 1, (byte)filter);
            previous = dest.ToArray();
            destRow += pitch;
        }

        return destRow == height * pitch;
    }

    private static int ReadByte(Stream stream)
    {
        Span<byte> one = stackalloc byte[1];
        return stream.Read(one) == 1 ? one[0] : -1;
    }

    private static int ReadFully(Stream stream, Span<byte> buffer)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var chunk = stream.Read(buffer[read..]);
            if (chunk == 0)
                return read;
            read += chunk;
        }

        return read;
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
}
