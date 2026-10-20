namespace HCDE.Client;

/// <summary>Native <c>v_colortables.cpp</c> translucency table builder.</summary>
public static class VmColorBlendTables
{
    public static VmColorBlendTableSet Build(ReadOnlySpan<VmPaletteEntry> palette, VmPaletteIndexPicker picker)
    {
        var rgb32k = new byte[32 * 32 * 32];
        for (var r = 0; r < 32; r++)
        for (var g = 0; g < 32; g++)
        for (var b = 0; b < 32; b++)
            rgb32k[(r * 32 + g) * 32 + b] = picker(Expand5(r), Expand5(g), Expand5(b), palette);

        var rgb256k = new byte[64 * 64 * 64];
        for (var r = 0; r < 64; r++)
        for (var g = 0; g < 64; g++)
        for (var b = 0; b < 64; b++)
            rgb256k[(r * 64 + g) * 64 + b] = picker(Expand6(r), Expand6(g), Expand6(b), palette);

        var col2Rgb8 = new uint[65, 256];
        var col2Rgb8Inverse = new uint[65, 256];
        var col2Rgb8LessPrecision = new uint[65][];
        var col2Rgb8_2 = new uint[63, 256];

        for (var x = 0; x < 65; x++)
        for (var y = 0; y < 256; y++)
        {
            var entry = palette[y];
            col2Rgb8[x, y] = BlendChannel(entry.R, x, 20) |
                               BlendChannel(entry.G, x, 0) |
                               BlendChannel(entry.B, x, 10);
            col2Rgb8Inverse[x, y] = (BlendChannel(255 - entry.R, x, 20) |
                                     BlendChannel(255 - entry.G, x, 0) |
                                     BlendChannel(255 - entry.B, x, 10)) & 0x3feffbffU;
        }

        for (var x = 1; x < 64; x++)
        {
            col2Rgb8LessPrecision[x] = new uint[256];
            for (var y = 0; y < 256; y++)
            {
                var value = col2Rgb8[x, y] & 0x3feffbffU;
                col2Rgb8_2[x - 1, y] = value;
                col2Rgb8LessPrecision[x][y] = value;
            }
        }

        col2Rgb8LessPrecision[0] = CopyRow(col2Rgb8, 0);
        col2Rgb8LessPrecision[64] = CopyRow(col2Rgb8, 64);

        return new VmColorBlendTableSet(rgb32k, rgb256k, col2Rgb8, col2Rgb8Inverse, col2Rgb8LessPrecision);
    }

    private static uint[] CopyRow(uint[,] table, int row)
    {
        var copy = new uint[256];
        for (var y = 0; y < 256; y++)
            copy[y] = table[row, y];
        return copy;
    }

    private static uint BlendChannel(int channel, int weight, int shift) =>
        (uint)(((channel * weight) >> 4) << shift);

    private static int Expand5(int value) => (value << 3) | (value >> 2);
    private static int Expand6(int value) => (value << 2) | (value >> 4);
}

public readonly record struct VmPaletteEntry(byte R, byte G, byte B);

public delegate byte VmPaletteIndexPicker(int r, int g, int b, ReadOnlySpan<VmPaletteEntry> palette);

public sealed class VmColorBlendTableSet
{
    public VmColorBlendTableSet(
        byte[] rgb32k,
        byte[] rgb256k,
        uint[,] col2Rgb8,
        uint[,] col2Rgb8Inverse,
        uint[][] col2Rgb8LessPrecision)
    {
        Rgb32k = rgb32k;
        Rgb256k = rgb256k;
        Col2Rgb8 = col2Rgb8;
        Col2Rgb8Inverse = col2Rgb8Inverse;
        Col2Rgb8LessPrecision = col2Rgb8LessPrecision;
    }

    public byte[] Rgb32k { get; }
    public byte[] Rgb256k { get; }
    public uint[,] Col2Rgb8 { get; }
    public uint[,] Col2Rgb8Inverse { get; }
    public uint[][] Col2Rgb8LessPrecision { get; }
}
