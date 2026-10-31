namespace HCDE.Client;

/// <summary>
/// Overlay color defaults, <c>AMColor</c> packing, color-set names, and
/// <c>AMColorset::initFromCVars</c> almost-background from <c>am_map.cpp</c> (<b>301–450</b>).
/// Chunk <b>1123</b>.
/// </summary>
public static class VmAmMapOverlayColor1123Slice
{
    public const int AlmostBackgroundIndex = 25;
    public const int ColorCount = 26;
    public const float DefaultOverlaySectorFillAlpha = 0.2f;

    public static uint ForceOpaque(uint color)
    {
        return color | 0xFF000000u;
    }

    public static uint PackOpaqueRgb(int r, int g, int b)
    {
        return (uint)((255 << 24) | ((r & 255) << 16) | ((g & 255) << 8) | (b & 255));
    }

    public static bool IsValidColor(uint rgb)
    {
        if (rgb == 0)
            return false;
        return true;
    }

    public static (int R, int G, int B) DeriveAlmostBackground(uint background)
    {
        var r = (int)((background >> 16) & 0xff) - 16;
        var g = (int)((background >> 8) & 0xff) - 16;
        var b = (int)(background & 0xff) - 16;
        if (r < 0)
            r += 32;
        if (g < 0)
            g += 32;
        if (b < 0)
            b += 32;
        return (r, g, b);
    }

    public static uint[] InitFromCvars(
        IReadOnlyList<uint> cvarColors,
        float fillAlpha,
        out float storedFillAlpha,
        out bool displayLocks,
        out bool forceBackground)
    {
        if (cvarColors.Count < AlmostBackgroundIndex)
            throw new ArgumentException("initFromCVars stops before AlmostBackgroundColor.", nameof(cvarColors));

        var colors = new uint[ColorCount];
        for (var i = 0; i < AlmostBackgroundIndex; i++)
            colors[i] = ForceOpaque(cvarColors[i]);

        var almost = DeriveAlmostBackground(cvarColors[0]);
        colors[AlmostBackgroundIndex] = PackOpaqueRgb(almost.R, almost.G, almost.B);
        storedFillAlpha = fillAlpha;
        displayLocks = true;
        forceBackground = false;
        return colors;
    }

    public static IReadOnlyDictionary<string, uint> DefaultOverlayColors() => new Dictionary<string, uint>
    {
        ["ovlocked"] = 0x008844,
        ["ovefwall"] = 0x008844,
        ["ovfdwall"] = 0x008844,
        ["ovcdwall"] = 0x008844,
        ["ovunseen"] = 0x00226e,
        ["ovtele"] = 0xffff00,
        ["ovinterlevel"] = 0xffff00,
        ["ovsecretsector"] = 0x00ffff,
        ["ovunexploredsecret"] = 0x00ffff,
        ["ovthing"] = 0xe88800,
        ["ovthingfriend"] = 0xe88800,
        ["ovthingmonster"] = 0xe88800,
        ["ovthingncmonster"] = 0xe88800,
        ["ovthingitem"] = 0xe88800,
        ["ovthingcitem"] = 0xe88800,
        ["ovsectorfill"] = 0x000000,
        ["ovportal"] = 0x004022
    };

    public static IReadOnlyList<string> ColorNames() =>
    [
        "Background",
        "YourColor",
        "WallColor",
        "TwoSidedWallColor",
        "FloorDiffWallColor",
        "CeilingDiffWallColor",
        "ExtraFloorWallColor",
        "ThingColor",
        "ThingColor_Item",
        "ThingColor_CountItem",
        "ThingColor_Monster",
        "ThingColor_NocountMonster",
        "ThingColor_Friend",
        "SpecialWallColor",
        "SecretWallColor",
        "GridColor",
        "XHairColor",
        "NotSeenColor",
        "LockedColor",
        "IntraTeleportColor",
        "InterTeleportColor",
        "SecretSectorColor",
        "UnexploredSecretColor",
        "SectorFillColor",
        "PortalColor",
        "AlmostBackgroundColor"
    ];
}
