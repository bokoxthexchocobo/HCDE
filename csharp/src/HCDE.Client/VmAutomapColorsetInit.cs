namespace HCDE.Client;

/// <summary>`AM_ClearColorsets` / `AM_initColors` from <c>am_map.cpp</c> (628–759).</summary>
public static class VmAutomapColorsetInit
{
    [Flags]
    public enum GameKind
    {
        None = 0,
        DoomChex = 1,
        Strife = 2,
        Raven = 4,
    }

    public enum ColorsetInitSource
    {
        OverlayCustomMod,
        OverlayCvars,
        StandardCustomMod,
        StandardCvars,
        PredefinedDoom,
        PredefinedStrife,
        PredefinedRaven,
    }

    public static void ClearModColorsets(ref bool modDefined, ref bool overlayDefined)
    {
        modDefined = false;
        overlayDefined = false;
    }

    public static int ResolvePresetColorset(int amColorset, GameKind gameKind)
    {
        if (amColorset != -1)
            return amColorset;

        if ((gameKind & GameKind.DoomChex) != 0)
            return 1;
        if ((gameKind & GameKind.Strife) != 0)
            return 2;
        if ((gameKind & GameKind.Raven) != 0)
            return 3;
        return -1;
    }

    public static ColorsetInitSource PlanInit(
        bool overlayed,
        bool customColors,
        bool modDefined,
        bool overlayModDefined,
        int amColorset,
        GameKind gameKind)
    {
        if (overlayed)
        {
            if (customColors && overlayModDefined)
                return ColorsetInitSource.OverlayCustomMod;
            return ColorsetInitSource.OverlayCvars;
        }

        if (customColors && modDefined)
            return ColorsetInitSource.StandardCustomMod;

        var set = ResolvePresetColorset(amColorset, gameKind);
        return set switch
        {
            1 => ColorsetInitSource.PredefinedDoom,
            2 => ColorsetInitSource.PredefinedStrife,
            3 => ColorsetInitSource.PredefinedRaven,
            _ => ColorsetInitSource.StandardCvars,
        };
    }

    public static bool PredefinedUsesRavenGrid(int presetSet) => presetSet == 3;

    public static ReadOnlySpan<byte> GetPredefinedPalette(int presetSet) => presetSet switch
    {
        2 => StrifeColors,
        3 => RavenColors,
        _ => ReadOnlySpan<byte>.Empty,
    };

    // Mirrors `StrifeColors[]` in am_map.cpp (628–655).
    public static ReadOnlySpan<byte> StrifeColors => new byte[]
    {
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
        1, 0, 0,
        1, 0, 0,
        0x4c, 0x4c, 0x4c,
        0x80, 0x80, 0x80,
        0x6c, 0x6c, 0x6c,
        119, 115, 115,
        1, 0, 0,
        1, 0, 0,
        1, 0, 0,
        1, 0, 0,
        1, 0, 0,
        0x10, 0x10, 0x10,
        0x40, 0x40, 0x40,
    };

    // Mirrors `RavenColors[]` in am_map.cpp (657–684).
    public static ReadOnlySpan<byte> RavenColors => new byte[]
    {
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
        1, 0, 0,
        1, 0, 0,
        75, 50, 16,
        0x00, 0x00, 0x00,
        0x00, 0x00, 0x00,
        103, 59, 31,
        1, 0, 0,
        1, 0, 0,
        1, 0, 0,
        1, 0, 0,
        1, 0, 0,
        0x10, 0x10, 0x10,
        0x50, 0x50, 0x50,
    };
}
