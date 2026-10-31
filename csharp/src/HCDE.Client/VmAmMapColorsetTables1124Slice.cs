namespace HCDE.Client;

/// <summary>
/// <c>initFromColors</c>, white colorset, linedef color indexes, CVAR tables, and
/// <c>am_restorecolors</c> from <c>am_map.cpp</c> (<b>451–600</b>). Chunk <b>1124</b>.
/// </summary>
public static class VmAmMapColorsetTables1124Slice
{
    public const int ColorCount = 26;
    public const int LineStyleCount = 14;
    public const byte UnusedMarker = 1;

    public const int WallColor = 2;
    public const int TsWallColor = 3;
    public const int FdWallColor = 4;
    public const int CdWallColor = 5;
    public const int EfWallColor = 6;
    public const int SpecialWallColor = 13;
    public const int SecretWallColor = 14;
    public const int NotSeenColor = 17;
    public const int LockedColor = 18;
    public const int IntraTeleportColor = 19;
    public const int InterTeleportColor = 20;
    public const int UnexploredSecretColor = 22;
    public const int PortalColor = 24;

    public static uint PackOpaqueRgb(int r, int g, int b)
    {
        return (uint)((255 << 24) | ((r & 255) << 16) | ((g & 255) << 8) | (b & 255));
    }

    public static bool IsUnusedTriplet(byte r, byte g, byte b)
    {
        return r == UnusedMarker && g == 0 && b == 0;
    }

    public static uint[] InitFromColorTriplets(
        ReadOnlySpan<byte> colors,
        bool showLocks,
        out float fillAlpha,
        out bool displayLocks,
        out bool forceBackground)
    {
        if (colors.Length < ColorCount * 3)
            throw new ArgumentException("initFromColors walks AM_NUM_COLORS RGB triplets.", nameof(colors));

        var packed = new uint[ColorCount];
        for (var i = 0; i < ColorCount; i++)
        {
            var j = i * 3;
            if (IsUnusedTriplet(colors[j], colors[j + 1], colors[j + 2]))
                packed[i] = 0;
            else
                packed[i] = PackOpaqueRgb(colors[j], colors[j + 1], colors[j + 2]);
        }

        fillAlpha = 0f;
        displayLocks = showLocks;
        forceBackground = false;
        return packed;
    }

    public static uint[] SetWhite()
    {
        var packed = new uint[ColorCount];
        packed[0] = PackOpaqueRgb(0, 0, 0);
        for (var i = 1; i < ColorCount; i++)
            packed[i] = PackOpaqueRgb(255, 255, 255);
        return packed;
    }

    public static bool IsValidIndex(IReadOnlyList<uint> colors, int index)
    {
        return colors[index] != 0;
    }

    public static int LineStyleColorIndex(int style)
    {
        return LineStyleColors()[style];
    }

    public static IReadOnlyList<int> LineStyleColors() =>
    [
        -1,
        WallColor,
        TsWallColor,
        FdWallColor,
        CdWallColor,
        EfWallColor,
        SpecialWallColor,
        SecretWallColor,
        NotSeenColor,
        LockedColor,
        IntraTeleportColor,
        InterTeleportColor,
        UnexploredSecretColor,
        PortalColor
    ];

    public static IReadOnlyList<string> StandardCvarNames() =>
    [
        "am_backcolor", "am_yourcolor", "am_wallcolor", "am_tswallcolor",
        "am_fdwallcolor", "am_cdwallcolor", "am_efwallcolor", "am_thingcolor",
        "am_thingcolor_item", "am_thingcolor_citem", "am_thingcolor_monster",
        "am_thingcolor_ncmonster", "am_thingcolor_friend", "am_specialwallcolor",
        "am_secretwallcolor", "am_gridcolor", "am_xhaircolor", "am_notseencolor",
        "am_lockedcolor", "am_intralevelcolor", "am_interlevelcolor",
        "am_secretsectorcolor", "am_unexploredsecretcolor", "am_sectorfillcolor",
        "am_portalcolor"
    ];

    public static IReadOnlyList<string> OverlayCvarNames() =>
    [
        "am_backcolor", "am_ovyourcolor", "am_ovwallcolor", "am_ovotherwallscolor",
        "am_ovfdwallcolor", "am_ovcdwallcolor", "am_ovefwallcolor", "am_ovthingcolor",
        "am_ovthingcolor_item", "am_ovthingcolor_citem", "am_ovthingcolor_monster",
        "am_ovthingcolor_ncmonster", "am_ovthingcolor_friend", "am_ovspecialwallcolor",
        "am_ovsecretwallcolor", "am_gridcolor", "am_xhaircolor", "am_ovunseencolor",
        "am_ovlockedcolor", "am_ovtelecolor", "am_ovinterlevelcolor",
        "am_ovsecretsectorcolor", "am_ovunexploredsecretcolor", "am_ovsectorfillcolor",
        "am_ovportalcolor"
    ];

    public static int RestoreColorDefaults(Action<string> resetCvar, Action resetSectorFillAlpha, Action resetOverlayFillAlpha)
    {
        var resets = 0;
        foreach (var name in StandardCvarNames())
        {
            resetCvar(name);
            resets++;
        }

        resetSectorFillAlpha();
        foreach (var name in OverlayCvarNames())
        {
            resetCvar(name);
            resets++;
        }

        resetOverlayFillAlpha();
        return resets;
    }

    public static (byte R, byte G, byte B) DoomBackgroundTriplet() => (0x00, 0x00, 0x00);
}
