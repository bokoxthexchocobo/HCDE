namespace HCDE.Client;

/// <summary>
/// Doom/Strife/Raven automap palettes, <c>AM_ClearColorsets</c>, and
/// <c>AM_initColors</c> through the Strife case from <c>am_map.cpp</c> (<b>601–750</b>).
/// Chunk <b>1125</b>. The Raven <c>switch</c> arm is <b>752–755</b>.
/// </summary>
public static class VmAmMapPaletteInit1125Slice
{
    public const int ColorCount = 26;
    public const byte UnusedMarker = 1;

    public const int GameDoom = 1;
    public const int GameHeretic = 2;
    public const int GameHexen = 4;
    public const int GameStrife = 8;
    public const int GameChex = 16;
    public const int GameRaven = GameHeretic | GameHexen;
    public const int GameDoomChex = GameDoom | GameChex;

    public const int ColorsetDoom = 1;
    public const int ColorsetStrife = 2;
    public const int ColorsetRaven = 3;

    public enum InitKind
    {
        CopyOverlayMod,
        OverlayCvars,
        CopyStandardMod,
        StandardCvars,
        DoomPalette,
        StrifePalette,
        RavenResolvedPastHull
    }

    public static (byte R, byte G, byte B) Triplet(ReadOnlySpan<byte> palette, int index)
    {
        var j = index * 3;
        return (palette[j], palette[j + 1], palette[j + 2]);
    }

    public static bool IsUnused(ReadOnlySpan<byte> palette, int index)
    {
        var (r, g, b) = Triplet(palette, index);
        return r == UnusedMarker && g == 0 && b == 0;
    }

    public static byte[] DoomColors() =>
    [
        0x00, 0x00, 0x00,
        0xff, 0xff, 0xff,
        0xfc, 0x00, 0x00,
        0x80, 0x80, 0x80,
        0xbc, 0x78, 0x48,
        0xfc, 0xfc, 0x00,
        0xbc, 0x78, 0x48,
        0x74, 0xfc, 0x6c,
        0x74, 0xfc, 0x6c,
        0x74, 0xfc, 0x6c,
        0x74, 0xfc, 0x6c,
        0x74, 0xfc, 0x6c,
        0x74, 0xfc, 0x6c,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        0x4c, 0x4c, 0x4c,
        0x80, 0x80, 0x80,
        0x6c, 0x6c, 0x6c,
        0xfc, 0xfc, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        0x10, 0x10, 0x10,
        0x40, 0x40, 0x40
    ];

    public static byte[] StrifeColors() =>
    [
        0x00, 0x00, 0x00,
        239, 239, 0,
        199, 195, 195,
        119, 115, 115,
        55, 59, 91,
        119, 115, 115,
        55, 59, 91,
        187, 59, 0,
        219, 171, 0,
        219, 171, 0,
        0xfc, 0x00, 0x00,
        0xfc, 0x00, 0x00,
        0xfc, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        0x4c, 0x4c, 0x4c,
        0x80, 0x80, 0x80,
        0x6c, 0x6c, 0x6c,
        119, 115, 115,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        0x10, 0x10, 0x10,
        0x40, 0x40, 0x40
    ];

    public static byte[] RavenColors() =>
    [
        0x6c, 0x54, 0x40,
        0xff, 0xff, 0xff,
        75, 50, 16,
        88, 93, 86,
        208, 176, 133,
        103, 59, 31,
        208, 176, 133,
        236, 236, 236,
        236, 236, 236,
        236, 236, 236,
        236, 236, 236,
        236, 236, 236,
        236, 236, 236,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        75, 50, 16,
        0x00, 0x00, 0x00,
        0x00, 0x00, 0x00,
        103, 59, 31,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        UnusedMarker, 0x00, 0x00,
        0x10, 0x10, 0x10,
        0x50, 0x50, 0x50
    ];

    public static (bool OverlayDefined, bool StandardDefined) ClearColorsets() => (false, false);

    public static InitKind ChooseInit(
        bool overlayed,
        bool customColors,
        bool overlayModDefined,
        bool standardModDefined,
        int colorSet,
        int gameType)
    {
        if (overlayed)
        {
            if (customColors && overlayModDefined)
                return InitKind.CopyOverlayMod;
            return InitKind.OverlayCvars;
        }

        if (customColors && standardModDefined)
            return InitKind.CopyStandardMod;

        var set = colorSet;
        if (set == -1)
        {
            if ((gameType & GameDoomChex) != 0)
                set = ColorsetDoom;
            else if ((gameType & GameStrife) != 0)
                set = ColorsetStrife;
            else if ((gameType & GameRaven) != 0)
                set = ColorsetRaven;
        }

        if (set == ColorsetDoom)
            return InitKind.DoomPalette;
        if (set == ColorsetStrife)
            return InitKind.StrifePalette;
        if (set == ColorsetRaven)
            return InitKind.RavenResolvedPastHull;
        return InitKind.StandardCvars;
    }

    public static bool PaletteShowLocks(InitKind kind)
    {
        if (kind == InitKind.DoomPalette)
            return false;
        if (kind == InitKind.StrifePalette)
            return false;
        throw new ArgumentOutOfRangeException(nameof(kind), kind, "Show-locks for this kind is outside 601–750.");
    }
}
