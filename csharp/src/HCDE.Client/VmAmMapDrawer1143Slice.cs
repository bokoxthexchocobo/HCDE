namespace HCDE.Client;

/// <summary>
/// Map-marker placement, the crosshair, and the automap drawer from <c>am_map.cpp</c>
/// (<b>3301–3450</b>). Chunk <b>1143</b>. Serialize continues at <c>bigstate</c>.
/// </summary>
public static class VmAmMapDrawer1143Slice
{
    public const int SubsectorDrawn = 2;
    public const int LevelAllMap = 2;
    public const int TicRate = 35;
    public const int PanPixelsPerSecond = 140;
    public const int MarkCount = 10;

    public readonly record struct Pan(bool Applied, double X, double Y);

    public readonly record struct FrameRect(int X, int Y, int Width, int Height, bool Clear);

    public readonly record struct AutomapSave(int MarkIndex, int MarkValueCount, double ScaleMapToFrame, double ScaleFrameToMap);

    public static bool TargetsSelf(int arg0)
    {
        if (arg0 == 0)
            return true;
        return false;
    }

    public static (double X, double Y) MarkerScale(double scaleX, double scaleY, int arg2, double scaleMapToFrame)
    {
        if (arg2 == 1)
            return (scaleX * scaleMapToFrame, scaleY * scaleMapToFrame);
        return (scaleX, scaleY);
    }

    public static bool DrawMarkedActor(int arg1, int subsectorFlags)
    {
        if (arg1 == 0)
            return true;
        if (arg1 == 1 && (subsectorFlags & SubsectorDrawn) != 0)
            return true;
        return false;
    }

    public static bool AdvanceMarkedActors(int arg0)
    {
        if (arg0 != 0)
            return true;
        return false;
    }

    public static (int X, int Y) Crosshair(int width, int height)
    {
        return (width / 2, (height + 1) / 2);
    }

    public static double FrameDeltaSeconds(ulong nowMs, ulong lastMs)
    {
        return (nowMs - lastMs) * 0.001;
    }

    public static ulong NextClock(bool ran, ulong nowMs, ulong lastMs)
    {
        if (ran)
            return nowMs;
        return lastMs;
    }

    public static Pan ManualPan(
        bool follow, bool left, bool right, bool up, bool down, double scaleFrameToMap, double deltaSeconds)
    {
        if (follow)
            return new Pan(false, 0, 0);
        var step = (PanPixelsPerSecond / TicRate) * scaleFrameToMap * deltaSeconds * TicRate;
        var x = 0d;
        var y = 0d;
        if (left)
            x -= step;
        if (right)
            x += step;
        if (up)
            y += step;
        if (down)
            y -= step;
        return new Pan(true, x, y);
    }

    public static bool ScaleChanges(bool zoomIn, bool zoomOut, double zoomDir)
    {
        if (zoomIn || zoomOut || zoomDir != 0)
            return true;
        return false;
    }

    public static bool AllMap(int levelFlags2)
    {
        if ((levelFlags2 & LevelAllMap) != 0)
            return true;
        return false;
    }

    public static bool AllThings(bool allMap, bool hasScanner)
    {
        if (allMap && hasScanner)
            return true;
        return false;
    }

    public static int PortalGroup(bool overlay, int transitionGroup)
    {
        if (overlay)
            return transitionGroup;
        return 0;
    }

    public static FrameRect Frame(bool viewActive, int screenWidth, int bottom, int viewX, int viewY, int viewWidth, int viewHeight)
    {
        if (!viewActive)
            return new FrameRect(0, 0, screenWidth, bottom, true);
        return new FrameRect(viewX, viewY, viewWidth, viewHeight, false);
    }

    public static bool DrawSubsectors(bool textured, bool viewActive, bool fillColorValid, double fillAlpha)
    {
        if (textured && !viewActive)
            return true;
        if (fillColorValid && fillAlpha > 0)
            return true;
        return false;
    }

    public static bool DrawKeys(bool easyKey, bool showKeysAlways)
    {
        if (easyKey || showKeysAlways)
            return true;
        return false;
    }

    public static bool DrawThings(int cheat, bool allThings)
    {
        if (cheat >= 2 && cheat != 4)
            return true;
        if (allThings)
            return true;
        return false;
    }

    public static bool DrawCrosshair(bool viewActive)
    {
        if (!viewActive)
            return true;
        return false;
    }

    public static AutomapSave SaveHead(int markIndex, double scaleMapToFrame, double scaleFrameToMap)
    {
        return new AutomapSave(markIndex, MarkCount * 2, scaleMapToFrame, scaleFrameToMap);
    }
}
