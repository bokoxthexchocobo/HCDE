namespace HCDE.Client;

/// <summary>Native <c>flattexture.cpp</c> flat image source.</summary>
public static class VmFlatTextureImage
{
    public static VmFlatTextureDescriptor CreateFromLump(int lumpNum, int lumpArea)
    {
        var bits = GetDimensionBits(lumpArea);
        return new VmFlatTextureDescriptor(lumpNum, 1 << bits, 1 << bits, UseGamePalette: true, Masked: false, Translucent: false);
    }

    public static VmFlatTextureDescriptor TryCreate(int lumpNum) => CreateFromLump(lumpNum, 64 * 64);

    public static byte[] CreatePalettedPixels(ReadOnlySpan<byte> lumpData, VmFlatTextureDescriptor descriptor, int conversion)
    {
        var pixels = new byte[descriptor.Width * descriptor.Height];
        var read = Math.Min(pixels.Length, lumpData.Length);
        lumpData[..read].CopyTo(pixels);
        if (read < pixels.Length)
            pixels.AsSpan(read).Fill(0xBB);
        var wantLuminance = conversion == VmImageHelpers.LuminanceConversion;
        VmImageHelpers.FlipSquareBlockRemap(pixels, descriptor.Width, VmImageHelpers.GetRemap(wantLuminance));
        return pixels;
    }

    public static int GetDimensionBits(int area)
    {
        switch (area)
        {
            case 64 * 64: return 6;
            case 8 * 8: return 3;
            case 16 * 16: return 4;
            case 32 * 32: return 5;
            case 128 * 128: return 7;
            case 256 * 256: return 8;
            default: return 6;
        }
    }
}

public readonly record struct VmFlatTextureDescriptor(
    int SourceLump,
    int Width,
    int Height,
    bool UseGamePalette,
    bool Masked,
    bool Translucent);
