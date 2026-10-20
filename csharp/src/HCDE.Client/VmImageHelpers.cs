namespace HCDE.Client;

/// <summary>Native <c>imagehelpers.h</c> paletted flip/remap helpers used by flats.</summary>
public static class VmImageHelpers
{
    public const int LuminanceConversion = 1;

    public static byte[] GetRemap(bool wantLuminance, bool srcIsGrayscale = false)
    {
        if (wantLuminance)
            return srcIsGrayscale ? VmPaletteTables.GrayRamp : VmPaletteTables.GrayscaleMap;
        return srcIsGrayscale ? VmPaletteTables.GrayMap : VmPaletteTables.GameRemap;
    }

    public static void FlipSquareBlockRemap(Span<byte> block, int size, ReadOnlySpan<byte> remap)
    {
        for (var row = 0; row < size; row++)
        {
            var corner = row * size + row;
            var count = size - row;
            for (var column = 0; column < count; column++)
            {
                ref var a = ref block[corner + column];
                ref var b = ref block[corner + column * size];
                var temporary = remap[a];
                a = remap[b];
                b = temporary;
            }
        }
    }

    public static void FlipNonSquareBlockRemap(
        Span<byte> destination,
        ReadOnlySpan<byte> source,
        int width,
        int height,
        int sourcePitch,
        ReadOnlySpan<byte> remap)
    {
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            destination[x * height + y] = remap[source[x + y * sourcePitch]];
    }

    public static int Luminance(int r, int g, int b) => (r * 77 + g * 143 + b * 37) >> 8;

    public static byte RgbToPalettePrecise(bool wantLuminance, int r, int g, int b, int a = 255) =>
        wantLuminance
            ? (byte)(Luminance(r, g, b) * a / 255)
            : VmPaletteColorMatcher.Default.Pick(r, g, b);

    public static byte RgbToPalette(bool wantLuminance, int r, int g, int b, int a = 255)
    {
        if (wantLuminance)
            return (byte)(Luminance(r, g, b) * a / 255);
        return a < 128 ? (byte)0 : VmRgb256kTable.Lookup(r, g, b);
    }
}

/// <summary>Seam: native <c>ColorMatcher.Pick</c> for precise palette mapping.</summary>
public sealed class VmPaletteColorMatcher
{
    public static VmPaletteColorMatcher Default { get; } = new();

    public byte Pick(int r, int g, int b) => VmRgb256kTable.Lookup(r, g, b);
}

internal static class VmRgb256kTable
{
    public static byte Lookup(int r, int g, int b) => (byte)(((r >> 2) + (g >> 2) + (b >> 2)) % 256);
}

internal static class VmPaletteTables
{
    public static byte[] GameRemap { get; } = CreateIdentity();
    public static byte[] GrayMap { get; } = CreateIdentity();
    public static byte[] GrayRamp { get; } = CreateIdentity();
    public static byte[] GrayscaleMap { get; } = CreateIdentity();

    private static byte[] CreateIdentity()
    {
        var table = new byte[256];
        for (var index = 0; index < 256; index++)
            table[index] = (byte)index;
        return table;
    }
}
