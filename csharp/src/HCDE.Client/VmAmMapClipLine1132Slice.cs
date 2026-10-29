namespace HCDE.Client;

/// <summary>
/// Cohen-Sutherland <c>clipMline</c> and the first screen-space endpoint of <c>drawMline</c>
/// from <c>am_map.cpp</c> (<b>1651–1800</b>). Chunk <b>1132</b>.
/// </summary>
public static class VmAmMapClipLine1132Slice
{
    public const int Left = 1;
    public const int Right = 2;
    public const int Bottom = 4;
    public const int Top = 8;

    public static int Scale(int a, int b, int c)
    {
        return (int)(((long)a * b) / c);
    }

    public static int MapXToFrame(double mapX, double windowMapX, double scaleMapToFrame)
    {
        return (int)((mapX - windowMapX) * scaleMapToFrame);
    }

    public static int MapYToFrame(double mapY, double windowMapY, double scaleMapToFrame, int frameHeight)
    {
        return (int)(frameHeight - ((mapY - windowMapY) * scaleMapToFrame));
    }

    public static int MapOutcode(double x, double y, double minX, double minY, double maxX, double maxY)
    {
        var code = 0;
        if (y > maxY)
            code = Top;
        else if (y < minY)
            code = Bottom;
        if (x < minX)
            code |= Left;
        else if (x > maxX)
            code |= Right;
        return code;
    }

    public static int FrameOutcode(int x, int y, int frameWidth, int frameHeight)
    {
        var code = 0;
        if (y < 0)
            code |= Top;
        else if (y >= frameHeight)
            code |= Bottom;
        if (x < 0)
            code |= Left;
        else if (x >= frameWidth)
            code |= Right;
        return code;
    }

    public static bool TryClip(
        double ax, double ay, double bx, double by,
        double minX, double minY, double maxX, double maxY,
        int frameWidth, int frameHeight, double scaleMapToFrame,
        out int clippedAx, out int clippedAy, out int clippedBx, out int clippedBy)
    {
        var mapA = MapOutcode(ax, ay, minX, minY, maxX, maxY);
        var mapB = MapOutcode(bx, by, minX, minY, maxX, maxY);
        if ((mapA & mapB) != 0)
        {
            clippedAx = clippedAy = clippedBx = clippedBy = 0;
            return false;
        }

        clippedAx = MapXToFrame(ax, minX, scaleMapToFrame);
        clippedAy = MapYToFrame(ay, minY, scaleMapToFrame, frameHeight);
        clippedBx = MapXToFrame(bx, minX, scaleMapToFrame);
        clippedBy = MapYToFrame(by, minY, scaleMapToFrame, frameHeight);
        var codeA = FrameOutcode(clippedAx, clippedAy, frameWidth, frameHeight);
        var codeB = FrameOutcode(clippedBx, clippedBy, frameWidth, frameHeight);
        if ((codeA & codeB) != 0)
            return false;

        while ((codeA | codeB) != 0)
        {
            var outside = codeA != 0 ? codeA : codeB;
            int nextX;
            int nextY;
            if ((outside & Top) != 0)
            {
                var dy = clippedAy - clippedBy;
                var dx = clippedBx - clippedAx;
                nextX = clippedAx + Scale(dx, clippedAy, dy);
                nextY = 0;
            }
            else if ((outside & Bottom) != 0)
            {
                var dy = clippedAy - clippedBy;
                var dx = clippedBx - clippedAx;
                nextX = clippedAx + Scale(dx, clippedAy - frameHeight, dy);
                nextY = frameHeight - 1;
            }
            else if ((outside & Right) != 0)
            {
                var dy = clippedBy - clippedAy;
                var dx = clippedBx - clippedAx;
                nextY = clippedAy + Scale(dy, frameWidth - 1 - clippedAx, dx);
                nextX = frameWidth - 1;
            }
            else
            {
                var dy = clippedBy - clippedAy;
                var dx = clippedBx - clippedAx;
                nextY = clippedAy + Scale(dy, -clippedAx, dx);
                nextX = 0;
            }

            if (outside == codeA)
            {
                clippedAx = nextX;
                clippedAy = nextY;
                codeA = FrameOutcode(clippedAx, clippedAy, frameWidth, frameHeight);
            }
            else
            {
                clippedBx = nextX;
                clippedBy = nextY;
                codeB = FrameOutcode(clippedBx, clippedBy, frameWidth, frameHeight);
            }

            if ((codeA & codeB) != 0)
                return false;
        }

        return true;
    }

    public static (int X, int Y) ScreenStart(int frameOriginX, int frameOriginY, int clippedX, int clippedY)
    {
        return (frameOriginX + clippedX, frameOriginY + clippedY);
    }
}
