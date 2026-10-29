namespace HCDE.Client;

/// <summary>
/// Vertex bounds tail, zoom limits, rotated clip, parchment scroll, pan, and the
/// <c>startDisplay</c> head from <c>am_map.cpp</c> (<b>1201–1350</b>). Chunk <b>1129</b>.
/// </summary>
public static class VmAmMapWindowBounds1129Slice
{
    public const double PlayerRadius = 16;
    public const double ParchmentLimit = 40000;
    public const double FloatMax = float.MaxValue;

    public static void ConsiderVertex(
        ref double minX, ref double minY, ref double maxX, ref double maxY, double x, double y)
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

    public static (double MaxW, double MaxH, double MinW, double MinH) ExtentsAfterVertices(double minX, double minY, double maxX, double maxY)
    {
        return (maxX - minX, maxY - minY, 2 * PlayerRadius, 2 * PlayerRadius);
    }

    public static (double MinScale, double MaxScale) ZoomScales(int emptySpaceMargin, double frameWidth, double statusTop, double frameHeight, double maxW, double maxH)
    {
        var safeFrame = 1.0 - emptySpaceMargin / 100.0;
        var fitWidth = safeFrame * (frameWidth / maxW);
        var fitHeight = safeFrame * (statusTop / maxH);
        var minScale = fitWidth < fitHeight ? fitWidth : fitHeight;
        var maxScale = frameHeight / (2 * PlayerRadius);
        return (minScale, maxScale);
    }

    public static bool ShouldLockToLevelBounds(int rotateMode, bool viewActive)
    {
        if (rotateMode == 0)
            return true;
        if (rotateMode == 2 && !viewActive)
            return true;
        return false;
    }

    public static void ClipCenterToBounds(
        ref double x, ref double y, ref double x2, ref double y2,
        double w, double h, double minX, double minY, double maxX, double maxY, bool lockToBounds)
    {
        if (lockToBounds)
        {
            if (x + w / 2 > maxX)
                x = maxX - w / 2;
            else if (x + w / 2 < minX)
                x = minX - w / 2;

            if (y + h / 2 > maxY)
                y = maxY - h / 2;
            else if (y + h / 2 < minY)
                y = minY - h / 2;
        }

        x2 = x + w;
        y2 = y + h;
    }

    public static double ClampParchment(double value)
    {
        if (value < -ParchmentLimit)
            return -ParchmentLimit;
        if (value > ParchmentLimit)
            return ParchmentLimit;
        return value;
    }

    public static (double X, double Y) ScrollParchment(
        double mapX, double mapY, double deltaMapX, double deltaMapY, double scaleMapToFrame,
        bool textureValid, int patchWidth, int patchHeight)
    {
        mapX -= deltaMapX * scaleMapToFrame;
        mapY -= deltaMapY * scaleMapToFrame;
        mapX = ClampParchment(mapX);
        mapY = ClampParchment(mapY);
        if (textureValid && patchWidth > 0 && patchHeight > 0)
        {
            while (mapX > 0)
                mapX -= patchWidth;
            while (mapX <= -patchWidth)
                mapX += patchWidth;
            while (mapY > 0)
                mapY -= patchHeight;
            while (mapY <= -patchHeight)
                mapY += patchHeight;
        }

        return (mapX, mapY);
    }

    public sealed class PanResult
    {
        public bool FollowPlayer { get; set; }
        public double OldLocationX { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double ScrollX { get; set; }
        public double ScrollY { get; set; }
    }

    public static PanResult ChangeWindowLoc(
        bool followPlayer,
        double panX, double panY,
        double x, double y, double w, double h,
        double minX, double minY, double maxX, double maxY,
        int frameWidth, int frameHeight,
        int rotateMode, bool viewActive,
        Action<double, double, double[]>? rotatePan)
    {
        var result = new PanResult { FollowPlayer = followPlayer, X = x, Y = y };
        if (panX != 0 || panY != 0)
        {
            result.FollowPlayer = false;
            result.OldLocationX = FloatMax;
        }

        var scaledX = panX * frameWidth / 320.0;
        var scaledY = panY * frameHeight / 200.0;
        var movedX = scaledX;
        var movedY = scaledY;
        if (rotateMode == 1 || (rotateMode == 2 && viewActive))
        {
            var pair = new[] { movedX, movedY };
            rotatePan?.Invoke(movedX, movedY, pair);
            movedX = pair[0];
            movedY = pair[1];
        }

        var oldX = x;
        var oldY = y;
        var nextX = x + movedX;
        var nextY = y + movedY;
        double x2 = 0, y2 = 0;
        ClipCenterToBounds(
            ref nextX, ref nextY, ref x2, ref y2, w, h, minX, minY, maxX, maxY,
            ShouldLockToLevelBounds(rotateMode, viewActive));
        result.X = nextX;
        result.Y = nextY;
        result.ScrollX = result.X != oldX ? scaledX : 0;
        result.ScrollY = result.Y != oldY ? -scaledY : 0;
        return result;
    }

    public static (double OldLocationX, int Clock, double PanX, double PanY, double ZoomMul, double W, double H) BeginDisplay(
        int frameWidth, int frameHeight, double scaleFrameToMap)
    {
        return (FloatMax, 0, 0, 0, 1, frameWidth * scaleFrameToMap, frameHeight * scaleFrameToMap);
    }
}
