namespace HCDE.Client;

/// <summary>
/// Automap scale/window helpers from <c>am_map.cpp</c> (1111–1328): activateNewScale through changeWindowLoc.
/// </summary>
public static class VmAutomapViewWindow
{
    public static (double x, double y, double w, double h) ActivateNewScale(
        double mX,
        double mY,
        double mW,
        double mH,
        int frameWidth,
        int frameHeight,
        double scaleFtOm)
    {
        var cx = mX + mW / 2;
        var cy = mY + mH / 2;
        var w = frameWidth * scaleFtOm;
        var h = frameHeight * scaleFtOm;
        var x = cx - w / 2;
        var y = cy - h / 2;
        return (x, y, w, h);
    }

    public static (double x, double y, double w, double h) RestoreScaleAndLoc(
        double oldX,
        double oldY,
        double oldW,
        double oldH,
        bool followPlayer,
        double cameraX,
        double cameraY)
    {
        var w = oldW;
        var h = oldH;
        double x;
        double y;
        if (!followPlayer)
        {
            x = oldX;
            y = oldY;
        }
        else
        {
            x = cameraX - w / 2;
            y = cameraY - h / 2;
        }

        return (x, y, w, h);
    }

    public static (double scaleMtof, double scaleFtOm) RecomputeScale(double frameWidth, double mapWidth) =>
        (frameWidth / mapWidth, mapWidth / frameWidth);

    public static bool CanAddMark(bool defaultFont, bool markTextureLoaded) =>
        !defaultFont || markTextureLoaded;

    public static int? TryAddMarkIndex(bool defaultFont, bool markTextureLoaded, int markIndex, int markCapacity)
    {
        if (!CanAddMark(defaultFont, markTextureLoaded))
            return null;
        return markIndex % markCapacity;
    }

    public static (double minX, double minY, double maxX, double maxY) FindVertexBounds(
        IEnumerable<(double x, double y)> vertices)
    {
        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;
        foreach (var (x, y) in vertices)
        {
            if (x < minX)
                minX = x;
            else if (x > maxX)
                maxX = x;
            if (y < minY)
                minY = y;
            else if (y > maxY)
                maxY = y;
        }

        return (minX, minY, maxX, maxY);
    }

    public static (double minScale, double maxScale) CalcMinMaxMapToFrame(
        double emptySpaceMarginPercent,
        double frameWidth,
        double frameHeight,
        double maxMapWidth,
        double maxMapHeight,
        double statusBarTop,
        double playerRadius)
    {
        var safe = 1.0 - emptySpaceMarginPercent / 100.0;
        var a = safe * (frameWidth / maxMapWidth);
        var b = safe * (statusBarTop / maxMapHeight);
        var minScale = a < b ? a : b;
        var maxScale = frameHeight / (2 * playerRadius);
        return (minScale, maxScale);
    }

    public static (double x, double y) ClipRotatedExtentsCenter(
        int amRotate,
        bool viewActive,
        double centerX,
        double centerY,
        double mapX,
        double mapY,
        double mapW,
        double mapH,
        double minX,
        double maxX,
        double minY,
        double maxY)
    {
        if (amRotate != 0 && !(amRotate == 2 && !viewActive))
            return (mapX, mapY);

        var x = mapX;
        var y = mapY;
        if (centerX > maxX)
            x = maxX - mapW / 2;
        else if (centerX < minX)
            x = minX - mapW / 2;

        if (centerY > maxY)
            y = maxY - mapH / 2;
        else if (centerY < minY)
            y = minY - mapH / 2;

        return (x, y);
    }

    public static double ClampParchmentScroll(double value) =>
        Math.Clamp(value, -40000, 40000);

    public static (double incX, double incY) ScalePanIncrements(
        double panX,
        double panY,
        double frameWidth,
        double frameHeight) =>
        (panX * frameWidth / 320, panY * frameHeight / 200);

    public static bool PanningDisablesFollow(double panX, double panY) => panX != 0 || panY != 0;
}
