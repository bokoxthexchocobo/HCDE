namespace HCDE.Client;

/// <summary>
/// Automap display CVARs, margin clamp, follow/grid/texture commands, and default colors from
/// <c>am_map.cpp</c> (**151–300**). Chunk **1122**.
/// </summary>
public static class VmAmMapDisplay1122Slice
{
    public const int GameActionNothing = 0;
    public const int MarkColorGrey = 2;
    public const string DefaultMarkFont = "AMMNUMx";

    public static int ClampEmptySpaceMargin(int margin)
    {
        if (margin < 0)
            return 0;
        if (margin > 90)
            return 90;
        return margin;
    }

    public static bool ToggleFollow(bool followPlayer)
    {
        if (followPlayer)
            return false;
        return true;
    }

    public static bool ToggleGrid(bool showGrid)
    {
        if (showGrid)
            return false;
        return true;
    }

    public static bool ToggleTexture(bool textured)
    {
        if (textured)
            return false;
        return true;
    }

    public static bool ShouldResetFollowLocation(bool hasLevel, bool hasAutomap) =>
        hasLevel && hasAutomap;

    public static int? TryAddMark(bool hasLevel, bool hasAutomap, Func<int>? addMark)
    {
        if (!hasLevel || !hasAutomap || addMark is null)
            return null;
        var mark = addMark();
        return mark >= 0 ? mark : null;
    }

    public static bool TryClearMarks(bool hasLevel, bool hasAutomap, Func<bool>? clearMarks) =>
        hasLevel && hasAutomap && clearMarks?.Invoke() == true;

    public static bool ShouldGoBig(bool hasLevel, bool hasAutomap) => hasLevel && hasAutomap;

    public static bool ShouldToggleMap(int gameAction) => gameAction == GameActionNothing;

    public static bool TrySetZoomDirection(int argc, string? argument, out float zoomDirection)
    {
        if (argc < 2 || !float.TryParse(argument, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out zoomDirection))
        {
            zoomDirection = 0;
            return false;
        }

        return true;
    }

    public sealed class DisplayDefaults
    {
        public bool ShowSecrets { get; init; } = true;
        public bool ShowMonsters { get; init; } = true;
        public bool ShowItems { get; init; }
        public bool ShowTime { get; init; } = true;
        public bool ShowTotalTime { get; init; }
        public bool ShowLevelName { get; init; } = true;
        public int ColorSet { get; init; } = -1;
        public bool CustomColors { get; init; } = true;
        public int MapSecrets { get; init; } = 1;
        public int DrawMapBack { get; init; } = 1;
        public bool ShowKeys { get; init; } = true;
        public int ShowTriggerLines { get; init; }
        public int ShowThingSprites { get; init; }
        public bool ShowKeysAlways { get; init; }
        public bool FollowPlayer { get; init; } = true;
        public bool PortalOverlay { get; init; } = true;
        public bool ShowGrid { get; init; }
        public float ZoomDirection { get; init; }
        public string MarkFont { get; init; } = DefaultMarkFont;
        public int MarkColor { get; init; } = MarkColorGrey;
    }

    public static IReadOnlyDictionary<string, uint> DefaultColors() => new Dictionary<string, uint>
    {
        ["back"] = 0x6c5440,
        ["you"] = 0xfce8d8,
        ["wall"] = 0x2c1808,
        ["secretwall"] = 0x000000,
        ["specialwall"] = 0xffffff,
        ["tswall"] = 0x888888,
        ["fdwall"] = 0x887058,
        ["cdwall"] = 0x4c3820,
        ["efwall"] = 0x665555,
        ["thing"] = 0xfcfcfc,
        ["grid"] = 0x8b5a2b,
        ["xhair"] = 0x808080,
        ["notseen"] = 0x6c6c6c,
        ["locked"] = 0x007800,
        ["intralevel"] = 0x0000ff,
        ["interlevel"] = 0xff0000,
        ["secretsector"] = 0xff00ff,
        ["unexploredsecret"] = 0xff00ff,
        ["thingfriend"] = 0xfcfcfc,
        ["thingmonster"] = 0xfcfcfc,
        ["thingncmonster"] = 0xfcfcfc,
        ["thingitem"] = 0xfcfcfc,
        ["thingcitem"] = 0xfcfcfc,
        ["sectorfill"] = 0x4e3621,
        ["portal"] = 0x404040,
        ["ovyou"] = 0xfce8d8,
        ["ovwall"] = 0x00ff00,
        ["ovsecretwall"] = 0x008844,
        ["ovspecialwall"] = 0xffffff,
        ["ovotherwalls"] = 0x008844
    };

    public const float DefaultSectorFillAlpha = 0.4f;
}
