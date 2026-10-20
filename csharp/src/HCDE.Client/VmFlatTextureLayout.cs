namespace HCDE.Client;

/// <summary>Native <c>flattexture.cpp</c> flat dimension selection from lump area.</summary>
public static class VmFlatTextureLayout
{
    public static bool TryGetDimensionsFromArea(int area, out int width, out int height)
    {
        int bits;
        switch (area)
        {
            case 64 * 64:
                bits = 6;
                break;
            case 8 * 8:
                bits = 3;
                break;
            case 16 * 16:
                bits = 4;
                break;
            case 32 * 32:
                bits = 5;
                break;
            case 128 * 128:
                bits = 7;
                break;
            case 256 * 256:
                bits = 8;
                break;
            default:
                bits = 6;
                break;
        }

        width = height = 1 << bits;
        return true;
    }

    public static byte[] ReadPalettedPixels(ReadOnlySpan<byte> lump, int width, int height)
    {
        var pixels = new byte[width * height];
        var read = Math.Min(pixels.Length, lump.Length);
        lump[..read].CopyTo(pixels);
        if (read < pixels.Length)
            pixels.AsSpan(read).Fill(0xBB);
        return pixels;
    }
}
