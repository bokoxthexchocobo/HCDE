namespace HCDE.Client;

/// <summary>
/// Automap key-responder tail, zoom step, follow scroll, ticker, and background blit decision
/// from <c>am_map.cpp</c> (<b>1501–1650</b>). Chunk <b>1131</b>.
/// </summary>
public static class VmAmMapZoomFollow1131Slice
{
    public const double ZoomInPerSecond = 2;
    public const double ZoomOutPerSecond = 0.2;
    public const double OldZoomIn = 1.02;
    public const double OldZoomOut = 1 / 1.02;
    public const int GameRaven = 6;

    public static bool ResponderResult(bool automapKeyEvent, bool keyHandled, bool keyUp, bool last, string? mainBinding)
    {
        if (!automapKeyEvent)
            return false;
        if (keyHandled && keyUp && !last)
            return string.IsNullOrEmpty(mainBinding) || mainBinding[0] != '+';
        return keyHandled;
    }

    public static double ZoomMultiplier(double zoomDirection, bool zoomInHeld, bool zoomOutHeld, double delta)
    {
        if (zoomDirection > 0)
            return OldZoomIn * zoomDirection;
        if (zoomDirection < 0)
            return OldZoomOut / -zoomDirection;
        if (zoomInHeld)
            return 1 + (ZoomInPerSecond - 1) * delta;
        if (zoomOutHeld)
            return 1 + (ZoomOutPerSecond - 1) * delta;
        return 1;
    }

    public static (double ScaleMapToFrame, double ScaleFrameToMap, double ZoomDirection) ApplyZoom(
        double scaleMapToFrame, double minScale, double maxScale, double zoomDirection, bool zoomInHeld, bool zoomOutHeld, double delta)
    {
        var scale = scaleMapToFrame * ZoomMultiplier(zoomDirection, zoomInHeld, zoomOutHeld, delta);
        if (scale < minScale)
            scale = minScale;
        else if (scale > maxScale)
            scale = maxScale;
        return (scale, 1 / scale, 0);
    }

    public readonly record struct FollowStep(bool Moved, double X, double Y, double X2, double Y2, double ScrollX, double ScrollY, double OldX, double OldY);

    public static FollowStep FollowPlayer(
        bool hasCamera,
        double cameraX, double cameraY,
        double oldX, double oldY,
        double windowW, double windowH,
        int rotateMode, bool viewActive,
        Action<double, double, double[]>? rotateScroll)
    {
        if (!hasCamera)
            return new FollowStep(false, 0, 0, 0, 0, 0, 0, oldX, oldY);

        if (oldX == cameraX && oldY == cameraY)
            return new FollowStep(false, 0, 0, 0, 0, 0, 0, oldX, oldY);

        var x = cameraX - windowW / 2;
        var y = cameraY - windowH / 2;
        var scrollX = cameraX - oldX;
        var scrollY = oldY - cameraY;
        if (rotateMode == 1 || (rotateMode == 2 && viewActive))
        {
            var pair = new[] { scrollX, scrollY };
            rotateScroll?.Invoke(scrollX, scrollY, pair);
            scrollX = pair[0];
            scrollY = pair[1];
        }

        return new FollowStep(true, x, y, x + windowW, y + windowH, scrollX, scrollY, cameraX, cameraY);
    }

    public static int TickClock(bool automapActive, int clock)
    {
        if (!automapActive)
            return clock;
        return clock + 1;
    }

    public static bool ShouldDrawMapBackground(
        bool textureValid, int drawMapBack, bool customColors, bool modDefined, int colorSet, int gameType)
    {
        if (!textureValid || drawMapBack == 0)
            return false;
        if (drawMapBack == 2 && (!customColors || !modDefined))
        {
            var raven = colorSet == 3 || (colorSet == -1 && (gameType & GameRaven) != 0);
            if (!raven)
                return false;
        }

        return true;
    }

    public static List<(int X, int Y)> BackgroundTiles(double originX, double originY, int frameW, int frameH, int patchW, int patchH)
    {
        var tiles = new List<(int X, int Y)>();
        if (patchW <= 0 || patchH <= 0)
            return tiles;
        for (var y = (int)originY; y < frameH; y += patchH)
        {
            for (var x = (int)originX; x < frameW; x += patchW)
                tiles.Add((x, y));
        }

        return tiles;
    }
}
