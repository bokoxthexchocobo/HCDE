namespace HCDE.Client;

/// <summary>Native <c>emptytexture.cpp</c> placeholder image source.</summary>
public static class VmEmptyTextureImage
{
    public const int PlaceholderLumpLength = 8;

    public static VmEmptyTextureImageSource TryCreateFromLump(ReadOnlySpan<byte> lumpData, int lumpNum)
    {
        if (!TryValidateLump(lumpData))
            throw new InvalidDataException("Empty texture lump must be eight zero bytes.");
        return new VmEmptyTextureImageSource(lumpNum);
    }

    public static VmEmptyTextureImageSource CreateDefault(int lumpNum = 0) => new(lumpNum);

    public static bool TryValidateLump(ReadOnlySpan<byte> lumpData)
    {
        if (lumpData.Length != PlaceholderLumpLength)
            return false;
        for (var index = 0; index < PlaceholderLumpLength; index++)
        {
            if (lumpData[index] != 0)
                return false;
        }

        return true;
    }

    public static ReadOnlyMemory<byte> CreatePalettedPixels(VmEmptyTextureImageSource source)
    {
        _ = source;
        return new byte[] { 0 };
    }
}

public sealed class VmEmptyTextureImageSource
{
    public VmEmptyTextureImageSource(int sourceLump)
    {
        SourceLump = sourceLump;
        Width = 1;
        Height = 1;
        Masked = true;
        UseGamePalette = true;
    }

    public int SourceLump { get; }
    public int Width { get; }
    public int Height { get; }
    public bool Masked { get; }
    public bool UseGamePalette { get; }
}
