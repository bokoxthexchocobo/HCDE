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
