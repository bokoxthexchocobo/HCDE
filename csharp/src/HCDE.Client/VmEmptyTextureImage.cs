namespace HCDE.Client;

/// <summary>Native <c>emptytexture.cpp</c> placeholder image source.</summary>
public static class VmEmptyTextureImage
{
    public const int PlaceholderLumpLength = 8;

    public static bool TryCreateFromLump(ReadOnlySpan<byte> lumpData, out VmEmptyTextureImageDescriptor? descriptor)
    {
        descriptor = null;
        if (lumpData.Length != PlaceholderLumpLength)
            return false;
        for (var index = 0; index < PlaceholderLumpLength; index++)
        {
            if (lumpData[index] != 0)
                return false;
        }

        descriptor = new VmEmptyTextureImageDescriptor(1, 1, Masked: true, UseGamePalette: true);
        return true;
    }

    public static VmEmptyTextureImageDescriptor CreateDefault() =>
        new(1, 1, Masked: true, UseGamePalette: true);

    public static byte CreatePalettedPixel() => 0;
}

public readonly record struct VmEmptyTextureImageDescriptor(int Width, int Height, bool Masked, bool UseGamePalette);
