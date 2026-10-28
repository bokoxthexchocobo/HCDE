namespace HCDE.Client;

/// <summary>`AMColorset` helpers from <c>am_map.cpp</c> (426–514, 597–626).</summary>
public static class VmAutomapColorsetLoader
{
    public const byte UnusedColorMarker = 1;

    public static bool IsUnusedColorTriplet(byte r, byte g, byte b) =>
        r == UnusedColorMarker && g == 0 && b == 0;

    public static VmAutomapColorArgb DecodeTriplet(byte r, byte g, byte b) =>
        IsUnusedColorTriplet(r, g, b) ? VmAutomapColorArgb.Invalid : VmAutomapColorArgb.FromRgb(r, g, b);

    public static (int r, int g, int b) ComputeAlmostBackgroundRgb(int backR, int backG, int backB)
    {
        var r = backR - 16;
        var g = backG - 16;
        var b = backB - 16;
        if (r < 0) r += 32;
        if (g < 0) g += 32;
        if (b < 0) b += 32;
        return (r, g, b);
    }

    public static int LineStyleToColorIndex(int amls) => amls switch
    {
        1 => VmAutomapColorsetIndex.WallColor,
        2 => 3, // TSWallColor slot in native table
        3 => 4,
        4 => 5,
        5 => 6,
        6 => 13,
        7 => 14,
        8 => 17,
        9 => 18,
        10 => 19,
        11 => 20,
        12 => 22,
        13 => 24,
        _ => VmAutomapColorsetIndex.WallColor,
    };
}
