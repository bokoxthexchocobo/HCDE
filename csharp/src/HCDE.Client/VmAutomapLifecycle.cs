namespace HCDE.Client;

/// <summary>
/// <c>DAutomap::Ticker</c>, <c>Serialize</c>, <c>UpdateShowAllLines</c>, <c>GoBig</c>, and <c>ResetFollowLocation</c> from <c>am_map.cpp</c>.
/// </summary>
public static class VmAutomapLifecycle
{
    public const int MarkPointArraySlots = 100;

    public static bool ShouldTickAutomap(bool automapActive) => automapActive;

    public static int AdvanceAutomapClock(int amclock) => amclock + 1;

    public readonly struct SerializeSnapshot
    {
        public SerializeSnapshot(
            int markPointNum,
            double scaleMtof,
            double scaleFtom,
            bool bigState,
            double minX,
            double minY,
            double maxX,
            double maxY,
            double minW,
            double minH,
            double maxW,
            double maxH,
            double minScaleMtof,
            double maxScaleMtof)
        {
            MarkPointNum = markPointNum;
            ScaleMtof = scaleMtof;
            ScaleFtom = scaleFtom;
            BigState = bigState;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            MinW = minW;
            MinH = minH;
            MaxW = maxW;
            MaxH = maxH;
            MinScaleMtof = minScaleMtof;
            MaxScaleMtof = maxScaleMtof;
        }

        public int MarkPointNum { get; }
        public double ScaleMtof { get; }
        public double ScaleFtom { get; }
        public bool BigState { get; }
        public double MinX { get; }
        public double MinY { get; }
        public double MaxX { get; }
        public double MaxY { get; }
        public double MinW { get; }
        public double MinH { get; }
        public double MaxW { get; }
        public double MaxH { get; }
        public double MinScaleMtof { get; }
        public double MaxScaleMtof { get; }
    }

    public static double[] PackMarkPoints(IReadOnlyList<(double x, double y)> marks, int slots)
    {
        var data = new double[slots * 2];
        for (var i = 0; i < slots && i < marks.Count; i++)
        {
            data[i * 2] = marks[i].x;
            data[i * 2 + 1] = marks[i].y;
        }

        return data;
    }

    public static bool ShouldSkipLineForShowAll(bool sameSector, bool hasFakeFloor, bool has3dFloorAttach) =>
        sameSector || hasFakeFloor || has3dFloorAttach;

    public static bool ComputeShowAllEnabled(int amShowAllLines, int totalLines, int flaggedDontDraw)
    {
        if (amShowAllLines > 0 && totalLines > 0)
            return flaggedDontDraw * 100 / totalLines >= amShowAllLines;
        if (amShowAllLines == 0)
            return true;
        return false;
    }

    public static bool ToggleBigState(bool current) => !current;

    public static bool ShouldSaveScaleOnGoBig(bool enteringBig) => enteringBig;

    public static bool ShouldRestoreScaleOnGoBig(bool leavingBig) => leavingBig;

    public static double ResetFollowOldLocationX() => double.MaxValue;

    public static double ComputeDrawerDeltaSeconds(ulong frameTimeMs, ulong lastFrameTimeMs) =>
        (frameTimeMs - lastFrameTimeMs) * 0.001;

    public static bool ShouldRunDrawer(bool automapActive) => automapActive;

    public static bool ShouldFollowPlayer(bool amFollowPlayer) => amFollowPlayer;

    public static (double x, double y) ClearPanWhenFollowing(bool followPlayer, double panX, double panY) =>
        followPlayer ? (0, 0) : (panX, panY);

    public static bool ShouldApplyPanWhenNotFollowing(bool followPlayer) => !followPlayer;

    public static double ScalePanDelta(double ftomPanInc, double deltaSeconds, int ticRate) =>
        ftomPanInc * deltaSeconds * ticRate;

    public static bool ShouldChangeZoom(bool zoomIn, bool zoomOut, int amZoomDir) =>
        zoomIn || zoomOut || amZoomDir != 0;

    public static bool ShouldDrawPortalOverlay(bool amPortalOverlay) => amPortalOverlay;

    public static int ResolveMapPortalGroup(bool portalOverlay, int sectorPortalGroup) =>
        portalOverlay ? sectorPortalGroup : 0;

    public static bool ShouldUseFullscreenAutomapFrame(bool viewActive) => !viewActive;

    public static bool ShouldDrawSubsectorFill(bool amTextured, bool viewActive, bool sectorFillValid, double fillAlpha) =>
        (amTextured && !viewActive) || (sectorFillValid && fillAlpha > 0.0);

    public static bool ShouldDeactivateAutomapOnStop() => true;

    public static bool ShouldReactivateViewOnStop() => true;
}
