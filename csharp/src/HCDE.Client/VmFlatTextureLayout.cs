namespace HCDE.Client;

/// <summary>Compatibility shim over <see cref="VmFlatTextureImage"/>.</summary>
public static class VmFlatTextureLayout
{
    public static bool TryGetDimensionsFromArea(int area, out int width, out int height)
    {
        var bits = VmFlatTextureImage.GetDimensionBits(area);
        width = height = 1 << bits;
        return true;
    }

    public static byte[] ReadPalettedPixels(ReadOnlySpan<byte> lump, int width, int height)
    {
        var descriptor = new VmFlatTextureDescriptor(0, width, height, true, false, false);
        return VmFlatTextureImage.CreatePalettedPixels(lump, descriptor, conversion: 0);
    }
}
