using System.Buffers.Binary;

namespace HCDE.Client;

/// <summary>Native <c>rawpagetexture.cpp</c> 320x200 raw page detection and column-major remap.</summary>
public static class VmRawPageTextureImage
{
    public const int PageWidth = 320;
    public const int PageHeight = 200;
    public const int PageSize = PageWidth * PageHeight;

    public static bool TryCreate(ReadOnlySpan<byte> lump, out int width, out int height)
    {
        width = height = 0;
        if (!CheckIfRaw(lump, PageSize))
            return false;
        width = PageWidth;
        height = PageHeight;
        return true;
    }

    public static byte[] CreatePalettedPixels(ReadOnlySpan<byte> lump, bool luminanceConversion = false)
    {
        if (lump.Length < PageSize)
            throw new InvalidDataException("Raw page too small.");
        var remap = VmImageHelpers.GetRemap(luminanceConversion);
        var pixels = new byte[PageSize];
        var dest = 0;
        var destAdvance = PageHeight;
        var destRewind = PageWidth * PageHeight - 1;
        var source = 0;
        for (var y = PageHeight; y > 0; y--)
        {
            for (var x = PageWidth; x > 0; x--)
            {
                pixels[dest] = remap[lump[source++]];
                dest += destAdvance;
            }

            dest -= destRewind;
        }

        return pixels;
    }

    public static bool CheckIfRaw(ReadOnlySpan<byte> data, int desiredSize)
    {
        if (data.Length != desiredSize)
            return false;
        var height = BinaryPrimitives.ReadInt16LittleEndian(data[2..]);
        var width = BinaryPrimitives.ReadInt16LittleEndian(data);
        if (height <= 0 || height >= 510 || width <= 0 || width >= 15997)
            return true;
        var gapAtStart = true;
        for (var x = 0; x < width; x++)
        {
            var offset = BinaryPrimitives.ReadUInt32LittleEndian(data[(8 + x * 4)..]);
            if (offset == (uint)width * 4 + 8)
                gapAtStart = false;
            else if (offset >= desiredSize - 1)
                return true;
            else
            {
                var walk = (int)offset;
                while (walk < desiredSize)
                {
                    if (data[walk] == 255)
                        return true;
                    walk += data[walk + 1] + 4;
                }

                if (walk >= desiredSize)
                    return true;
            }
        }

        return gapAtStart;
    }
}
