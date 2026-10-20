namespace HCDE.Client;

/// <summary>Native <c>buildtexture.cpp</c> paletted remap from raw tile pixels.</summary>
public static class VmBuildTextureImage
{
    public static byte[] CreatePalettedPixels(
        ReadOnlySpan<byte> rawPixels,
        int width,
        int height,
        ReadOnlySpan<byte> remap,
        bool luminanceConversion = false)
    {
        if (rawPixels.Length < width * height)
            throw new ArgumentException("Raw pixel buffer too small.");
        var pixels = new byte[width * height];
        for (var index = 0; index < pixels.Length; index++)
            pixels[index] = remap[rawPixels[index]];
        return pixels;
    }
}
