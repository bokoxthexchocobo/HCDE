namespace HCDE.Client;

/// <summary><c>DrawPaletteTester</c> layout from <c>d_main.cpp</c> (1008–1028).</summary>
public static class VmDMainPaletteTester
{
    public const int PaletteGrid = 16;

    public static int ComputeBlockSize(int screenHeight) => screenHeight / 50;

    public static int ComputeCellCount() => PaletteGrid * PaletteGrid;

    public static (int row, int col) ResolveCellCoordinates(int linearIndex)
    {
        var row = linearIndex / PaletteGrid;
        var col = linearIndex % PaletteGrid;
        return (row, col);
    }

    public static bool UsesTranslationPalette(int paletteNumber) => paletteNumber > 1;
}
