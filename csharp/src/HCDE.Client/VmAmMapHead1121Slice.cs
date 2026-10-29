namespace HCDE.Client;

/// <summary>
/// Automap constants and <c>am_cheat</c> / <c>am_overlay</c> gates from <c>am_map.cpp</c> (**1–150**).
/// Chunk **1121**. Pivot after <c>p_mobj.cpp</c> has no further in-file ≥150 clean window.
/// </summary>
public static class VmAmMapHead1121Slice
{
    public const int MarkPointCount = 10;
    public const double PlayerRadius = 16;
    public const double ZoomInPerSecond = 2;
    public const double ZoomOutPerSecond = 0.2;
    public const double OldZoomIn = 1.02;
    public const double OldZoomOut = 1 / 1.02;

    public static double ZoomIn(double scale, double seconds) =>
        scale * Math.Pow(ZoomInPerSecond, Math.Max(0, seconds));

    public static double ZoomOut(double scale, double seconds) =>
        scale * Math.Pow(ZoomOutPerSecond, Math.Max(0, seconds));

    public static double ApplyLegacyZoom(double scale, bool zoomIn, int steps)
    {
        var factor = zoomIn ? OldZoomIn : OldZoomOut;
        var next = scale;
        for (var i = 0; i < steps; i++)
            next *= factor;
        return next;
    }

    public static int ClampAutomapCheat(int requested, bool netGame, bool cheatsEnabled)
    {
        if (netGame && !cheatsEnabled && requested != 0)
            return 0;
        return requested;
    }

    public sealed class OverlayContext
    {
        public bool AutomapActive { get; set; }
        public bool ViewActive { get; set; }
        public int Overlay { get; init; }
    }

    public static void ApplyOverlayChange(OverlayContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.AutomapActive && ctx.ViewActive && ctx.Overlay == 0)
        {
            ctx.AutomapActive = false;
            ctx.ViewActive = true;
        }
    }

    public static bool ShouldRefreshShowAllLines(bool hasLevel, bool hasAutomap) =>
        hasLevel && hasAutomap;

    public sealed class HeadDefaults
    {
        public bool TexturedWalls { get; init; }
        public float LineAlpha { get; init; }
        public int LineThickness { get; init; }
        public int LineAntialiasing { get; init; }
        public bool ThingRenderStyles { get; init; }
        public int ShowSubsector { get; init; }
        public int ShowAllLines { get; init; }
        public int Rotate { get; init; }
        public int Overlay { get; init; }
    }

    public static HeadDefaults CreateDefaults() => new()
    {
        TexturedWalls = false,
        LineAlpha = 1f,
        LineThickness = 1,
        LineAntialiasing = 0,
        ThingRenderStyles = true,
        ShowSubsector = -1,
        ShowAllLines = -1,
        Rotate = 0,
        Overlay = 0
    };

    public static int RefreshShowAllLines(bool hasLevel, bool hasAutomap, int requested, Action<int>? update)
    {
        if (!ShouldRefreshShowAllLines(hasLevel, hasAutomap))
            return requested;
        update?.Invoke(requested);
        return requested;
    }
}
