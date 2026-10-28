namespace HCDE.Client;

/// <summary><c>DAutomap::DrawMarker</c> policy from <c>am_map.cpp</c> (3178–3207).</summary>
public static class VmAutomapDrawMarker
{
    public static bool ShouldSkipInvalidTexture(bool textureValid) => !textureValid;

    public static (bool flip, double xScale) NormalizeHorizontalFlip(bool flip, double xscale)
    {
        if (xscale >= 0)
            return (flip, xscale);
        return (!flip, -xscale);
    }

    public static bool ShouldRotateMarkerPosition(int amRotate, bool viewActive) =>
        amRotate == 1 || (amRotate == 2 && viewActive);

    public static (int destX, int destY) MapToFramebuffer(
        double mapX,
        double mapY,
        int yAdjust,
        int frameX,
        int frameY,
        Func<double, int> cxmtof,
        Func<double, int> cymtof) =>
        (cxmtof(mapX) + frameX, cymtof(mapY) + yAdjust + frameY);

    public static double DestWidth(double texDisplayWidth, double cleanXFac, double xscale) =>
        texDisplayWidth * cleanXFac * xscale;

    public static double DestHeight(double texDisplayHeight, double cleanYFac, double yscale) =>
        texDisplayHeight * cleanYFac * yscale;

    public static int ClipRectLeft(int frameX) => frameX;

    public static int ClipRectRight(int frameX, int frameW) => frameX + frameW;

    public static int ClipRectTop(int frameY) => frameY;

    public static int ClipRectBottom(int frameY, int frameH) => frameY + frameH;

    public static bool UseTranslationIndex(int translationIndex) => translationIndex >= 0;

    public static double ClampMarkerAlpha(double alpha) => alpha < 0 ? 0 : alpha > 1 ? 1 : alpha;

    public static uint ResolveFillColor(uint fillColor, bool useFill) => useFill ? fillColor : 0u;

    public static bool ShouldApplyRenderStyle(uint renderStyleDword) => renderStyleDword != 0;

    public static (double sx, double sy) ApplyMapRotationIfNeeded(
        double x,
        double y,
        bool rotate,
        Func<double, double, (double rx, double ry)> rotatePoint)
    {
        if (!rotate)
            return (x, y);
        var (rx, ry) = rotatePoint(x, y);
        return (rx, ry);
    }
}
