namespace HCDE.Client;

/// <summary>Automap color tables and <c>AMColorset</c> helpers from <c>am_map.cpp</c>.</summary>
public static partial class VmAutomapCustomCvarCallbacks
{
    public const byte UnusedColorMarker = 1;

    public static int StandardCvarTableLength => 25;

    public static int OverlayCvarTableLength => 25;

    public static int AlmostBackgroundColorIndex => 25;

    public static bool ShouldInitColorFromCvar(int index, int almostBackgroundIndex) =>
        index < almostBackgroundIndex;

    public static bool IsUnusedPaletteTriplet(byte r, byte g, byte b) =>
        r == UnusedColorMarker && g == 0 && b == 0;

    public static uint PackRgb(byte r, byte g, byte b) =>
        0xff000000u | ((uint)r << 16) | ((uint)g << 8) | b;

    public static void ApplySetWhite(Span<uint> colors)
    {
        if (colors.Length == 0)
            return;
        colors[0] = PackRgb(0, 0, 0);
        var white = PackRgb(255, 255, 255);
        for (var i = 1; i < colors.Length; i++)
            colors[i] = white;
    }

    public static void DecodePaletteTriplets(
        ReadOnlySpan<byte> rgbBytes,
        Span<uint> colors,
        Span<bool> valid,
        bool showLocks,
        out float fillAlpha)
    {
        fillAlpha = 0f;
        var slot = 0;
        for (var j = 0; j < rgbBytes.Length && slot < colors.Length; j += 3, slot++)
        {
            var r = rgbBytes[j];
            var g = rgbBytes[j + 1];
            var b = rgbBytes[j + 2];
            if (IsUnusedPaletteTriplet(r, g, b))
            {
                valid[slot] = false;
                colors[slot] = 0;
            }
            else
            {
                valid[slot] = true;
                colors[slot] = PackRgb(r, g, b);
            }
        }

        for (; slot < valid.Length; slot++)
            valid[slot] = false;
    }

    public static bool OverlayTableReusesStandardGrid(int slotIndex) =>
        slotIndex == 15 || slotIndex == 16;

    public static bool OverlayTableUsesBackgroundForSlotZero(int slotIndex) => slotIndex == 0;

    public static ReadOnlySpan<uint> OverlayColorDefaults => new uint[]
    {
        0xfffce8d8,
        0xff00ff00,
        0xff008844,
        0xffffffff,
        0xff008844,
        0xff008844,
        0xff008844,
        0xff008844,
        0xff008844,
        0xff00226e,
        0xffffff00,
        0xffffff00,
        0xff00ffff,
        0xff00ffff,
        0xffe88800,
        0xffe88800,
        0xffe88800,
        0xffe88800,
        0xffe88800,
        0xffe88800,
        0xff000000,
        0xff004022
    };

    public static ReadOnlySpan<string> StandardColorCvarNames => new[]
    {
        "am_backcolor",
        "am_yourcolor",
        "am_wallcolor",
        "am_tswallcolor",
        "am_fdwallcolor",
        "am_cdwallcolor",
        "am_efwallcolor",
        "am_thingcolor",
        "am_thingcolor_item",
        "am_thingcolor_citem",
        "am_thingcolor_monster",
        "am_thingcolor_ncmonster",
        "am_thingcolor_friend",
        "am_specialwallcolor",
        "am_secretwallcolor",
        "am_gridcolor",
        "am_xhaircolor",
        "am_notseencolor",
        "am_lockedcolor",
        "am_intralevelcolor",
        "am_interlevelcolor",
        "am_secretsectorcolor",
        "am_unexploredsecretcolor",
        "am_sectorfillcolor",
        "am_portalcolor"
    };

    public static ReadOnlySpan<string> OverlayColorCvarNames => new[]
    {
        "am_backcolor",
        "am_ovyourcolor",
        "am_ovwallcolor",
        "am_ovotherwallscolor",
        "am_ovfdwallcolor",
        "am_ovcdwallcolor",
        "am_ovefwallcolor",
        "am_ovthingcolor",
        "am_ovthingcolor_item",
        "am_ovthingcolor_citem",
        "am_ovthingcolor_monster",
        "am_ovthingcolor_ncmonster",
        "am_ovthingcolor_friend",
        "am_ovspecialwallcolor",
        "am_ovsecretwallcolor",
        "am_gridcolor",
        "am_xhaircolor",
        "am_ovunseencolor",
        "am_ovlockedcolor",
        "am_ovtelecolor",
        "am_ovinterlevelcolor",
        "am_ovsecretsectorcolor",
        "am_ovunexploredsecretcolor",
        "am_ovsectorfillcolor",
        "am_ovportalcolor"
    };

    public static bool TryGetColorAtIndex(ReadOnlySpan<uint> colors, int index, out uint rgb)
    {
        if (index < 0 || index >= colors.Length)
        {
            rgb = 0;
            return false;
        }

        rgb = colors[index];
        return true;
    }
}
