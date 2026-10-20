namespace HCDE.Client;

/// <summary>
/// <c>DAutomap::drawMarks</c>, <c>drawAuthorMarkers</c>, <c>drawCrosshair</c>, and <c>Drawer</c> orchestration from <c>am_map.cpp</c> (3215–3434).
/// </summary>
public static class VmAutomapDrawerCompose
{
    public const int MarkEmptySentinel = -1;
    public const int ButtonAmPanLeft = 1;
    public const int ButtonAmPanRight = 2;
    public const int ButtonAmPanUp = 4;
    public const int ButtonAmPanDown = 8;
    public const int ButtonAmZoomIn = 16;
    public const int ButtonAmZoomOut = 32;

    public static bool ShouldEarlyOutDrawer(bool automapActive) => !automapActive;

    public static (double panX, double panY) ComputeKeyboardPanDelta(
        bool panLeft,
        bool panRight,
        bool panUp,
        bool panDown,
        double ftomPanInc,
        double deltaSeconds,
        double ticRate)
    {
        var panX = 0.0;
        var panY = 0.0;
        if (panLeft)
            panX -= ftomPanInc * deltaSeconds * ticRate;
        if (panRight)
            panX += ftomPanInc * deltaSeconds * ticRate;
        if (panUp)
            panY += ftomPanInc * deltaSeconds * ticRate;
        if (panDown)
            panY -= ftomPanInc * deltaSeconds * ticRate;
        return (panX, panY);
    }

    public static bool ShouldFollowPlayer(int amFollowPlayer) => amFollowPlayer != 0;

    public static bool ShouldChangeZoom(bool zoomIn, bool zoomOut, int amZoomDir) =>
        zoomIn || zoomOut || amZoomDir != 0;

    public static bool ResolveAllMap(bool levelAllMap) => levelAllMap;

    public static bool ResolveAllThings(bool allMap, bool hasScannerPower) => allMap && hasScannerPower;

    public static int ResolveMapPortalGroup(bool portalOverlay, int sectorPortalGroup) =>
        portalOverlay ? sectorPortalGroup : 0;

    public static (int x, int y, int w, int h) ResolveFramebufferRect(bool viewActive, int twodWidth, int bottom, int viewX, int viewY, int viewW, int viewH) =>
        viewActive ? (viewX, viewY, viewW, viewH) : (0, 0, twodWidth, bottom);

    public static bool ShouldClearBackground(bool viewActive) => !viewActive;

    public static bool ShouldDrawTexturedSubsectors(bool amTextured, bool viewActive, bool sectorFillValid, double fillAlpha) =>
        (amTextured && !viewActive) || (sectorFillValid && fillAlpha > 0.0);

    public static bool ShouldDrawGrid(int amShowGrid) => amShowGrid != 0;

    public static bool ShouldDrawThingsLayer(int amCheat, bool allThings) =>
        (amCheat >= 2 && amCheat != 4) || allThings;

    public static bool ShouldDrawKeys(bool easyKey, bool showKeysAlways) => easyKey || showKeysAlways;

    public static bool ShouldDrawCrosshair(bool viewActive) => !viewActive;

    public static (int cx, int cy) CrosshairPixel(int frameW, int frameH) => (frameW / 2, (frameH + 1) / 2);

    public static bool HasMarkAt(double markX) => markX != MarkEmptySentinel;

    public static bool ShouldRotateMark(int amRotate, bool viewActive) =>
        amRotate == 1 || (amRotate == 2 && viewActive);

    public static bool ShouldUseDefaultMarkTexture(bool customFontLoaded) => !customFontLoaded;

    public static bool ShouldSkipDormantMapMarker(bool dormant) => dormant;

    public static bool ShouldDrawMarkedActor(bool arg1DrawnOnly, bool subsectorDrawn) =>
        arg1DrawnOnly == false || subsectorDrawn;

    public static bool ShouldScaleMarkerWithZoom(int arg2) => arg2 == 1;

    public static IEnumerable<int> EnumerateMarkSlots(int markCount) => Enumerable.Range(0, markCount);
}
