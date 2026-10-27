namespace HCDE.Playsim;

/// <summary>
/// Sector color, fade, panning, and scale specials from <c>p_lnspec.cpp</c> (**2419–2568**); chunk **929**.
/// </summary>
public static class VmPLnspecScrollSector929Slice
{
    public static double PanningOffset(int integer, int fraction) => integer + fraction / 100.0;

    public readonly record struct SectorColorInvoke(int Tag, int R, int G, int B, int Desaturate);

    public static SectorColorInvoke SectorSetColor(int tag, int r, int g, int b, int desaturate) =>
        new(tag, r, g, b, desaturate);

    public readonly record struct SectorFadeInvoke(int Tag, int R, int G, int B);

    public static SectorFadeInvoke SectorSetFade(int tag, int r, int g, int b) => new(tag, r, g, b);

    public static (double x, double y) CeilingPanning(int xInt, int xFrac, int yInt, int yFrac) =>
        (PanningOffset(xInt, xFrac), PanningOffset(yInt, yFrac));

    public static (double x, double y) FloorPanning(int xInt, int xFrac, int yInt, int yFrac) =>
        CeilingPanning(xInt, xFrac, yInt, yFrac);

    public readonly record struct SectorPanningInvoke(int Tag, double XOffset, double YOffset, bool CeilingPlane);

    public static SectorPanningInvoke BuildCeilingPanning(int tag, int xInt, int xFrac, int yInt, int yFrac)
    {
        var (x, y) = CeilingPanning(xInt, xFrac, yInt, yFrac);
        return new(tag, x, y, true);
    }

    public static SectorPanningInvoke BuildFloorPanning(int tag, int xInt, int xFrac, int yInt, int yFrac)
    {
        var (x, y) = FloorPanning(xInt, xFrac, yInt, yFrac);
        return new(tag, x, y, false);
    }

    public readonly record struct SectorScaleInvoke(int Tag, double? XScale, double? YScale);

    public static (double? xScale, double? yScale) InvertedScalePair(int xInt, int xFrac, int yInt, int yFrac) =>
        (InvertScaleComponent(xInt, xFrac), InvertScaleComponent(yInt, yFrac));

    public static double? InvertScaleComponent(int integer, int fraction)
    {
        var scale = integer + fraction / 100.0;
        if (scale == 0)
            return null;
        return 1.0 / scale;
    }

    public static SectorScaleInvoke SectorSetFloorScale(int tag, int xInt, int xFrac, int yInt, int yFrac) =>
        new(tag, InvertScaleComponent(xInt, xFrac), InvertScaleComponent(yInt, yFrac));

    public static SectorScaleInvoke SectorSetCeilingScale(int tag, int xInt, int xFrac, int yInt, int yFrac) =>
        SectorSetFloorScale(tag, xInt, xFrac, yInt, yFrac);

    public static double ScaleFactorFrom65536(int raw) => raw / 65536.0;

    public static double? InvertScaleFactor65536(int raw)
    {
        var scale = ScaleFactorFrom65536(raw);
        if (scale == 0)
            return null;
        return 1.0 / scale;
    }

    public static SectorScaleInvoke SectorSetFloorScale2(int tag, int xFactor, int yFactor) =>
        new(tag, InvertScaleFactor65536(xFactor), InvertScaleFactor65536(yFactor));

    public static SectorScaleInvoke SectorSetCeilingScale2(int tag, int xFactor, int yFactor) =>
        SectorSetFloorScale2(tag, xFactor, yFactor);

    public static bool ShouldApplyScale2X(int rawFactor, double? inverted) => rawFactor != 0 && inverted.HasValue;

    public static bool ShouldApplyScale2Y(int rawFactor, double? inverted) => rawFactor != 0 && inverted.HasValue;
}
