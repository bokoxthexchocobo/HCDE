namespace HCDE.Client;

/// <summary>
/// <c>DAutomap::clearFB</c>, <c>clipMline</c>, and <c>drawMline</c> policy from <c>am_map.cpp</c> (1618–1864).
/// Rendering calls (<c>twod</c>, textures) are not hosted; geometry and draw parameters are.
/// </summary>
public static class VmAutomapDrawGeometry
{
    public const int ClipLeft = 1;
    public const int ClipRight = 2;
    public const int ClipBottom = 4;
    public const int ClipTop = 8;

    public readonly struct MapPoint
    {
        public MapPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }

    public readonly struct FramePoint
    {
        public FramePoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }

    public readonly struct ClipContext
    {
        public ClipContext(
            double mapLeft,
            double mapTop,
            double mapRight,
            double mapBottom,
            int frameWidth,
            int frameHeight,
            double scaleMapToFrame)
        {
            MapLeft = mapLeft;
            MapTop = mapTop;
            MapRight = mapRight;
            MapBottom = mapBottom;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            ScaleMapToFrame = scaleMapToFrame;
        }

        public double MapLeft { get; }
        public double MapTop { get; }
        public double MapRight { get; }
        public double MapBottom { get; }
        public int FrameWidth { get; }
        public int FrameHeight { get; }
        public double ScaleMapToFrame { get; }
    }

    public enum FramebufferClearMode
    {
        SolidColor,
        TiledMapBackground,
    }

    public static bool ShouldUseMapBackground(
        bool mapBackValid,
        int amDrawMapBack,
        bool amCustomColors,
        bool amModDefined,
        int amColorset,
        bool isRavenGame)
    {
        if (!mapBackValid || amDrawMapBack == 0)
            return false;

        if (amDrawMapBack != 2)
            return true;

        if (amCustomColors && amModDefined)
            return true;

        return amColorset == 3 || (amColorset == -1 && isRavenGame);
    }

    public static FramebufferClearMode ResolveFramebufferClearMode(
        bool mapBackValid,
        int amDrawMapBack,
        bool amCustomColors,
        bool amModDefined,
        int amColorset,
        bool isRavenGame)
    {
        return ShouldUseMapBackground(
                mapBackValid,
                amDrawMapBack,
                amCustomColors,
                amModDefined,
                amColorset,
                isRavenGame)
            ? FramebufferClearMode.TiledMapBackground
            : FramebufferClearMode.SolidColor;
    }

    public static int MapXToFrame(double mapX, in ClipContext ctx) =>
        (int)((mapX - ctx.MapLeft) * ctx.ScaleMapToFrame);

    public static int MapYToFrame(double mapY, in ClipContext ctx) =>
        (int)(ctx.FrameHeight - (mapY - ctx.MapTop) * ctx.ScaleMapToFrame);

    public static int ComputeOutcode(double frameX, double frameY, in ClipContext ctx)
    {
        var oc = 0;
        if (frameY < 0)
            oc |= ClipTop;
        else if (frameY >= ctx.FrameHeight)
            oc |= ClipBottom;
        if (frameX < 0)
            oc |= ClipLeft;
        else if (frameX >= ctx.FrameWidth)
            oc |= ClipRight;
        return oc;
    }

    public static bool TryClipMapLine(
        MapPoint a,
        MapPoint b,
        in ClipContext ctx,
        out FramePoint clippedA,
        out FramePoint clippedB)
    {
        var outcode1 = 0;
        var outcode2 = 0;

        if (a.Y > ctx.MapBottom)
            outcode1 = ClipTop;
        else if (a.Y < ctx.MapTop)
            outcode1 = ClipBottom;

        if (b.Y > ctx.MapBottom)
            outcode2 = ClipTop;
        else if (b.Y < ctx.MapTop)
            outcode2 = ClipBottom;

        if ((outcode1 & outcode2) != 0)
        {
            clippedA = default;
            clippedB = default;
            return false;
        }

        if (a.X < ctx.MapLeft)
            outcode1 |= ClipLeft;
        else if (a.X > ctx.MapRight)
            outcode1 |= ClipRight;

        if (b.X < ctx.MapLeft)
            outcode2 |= ClipLeft;
        else if (b.X > ctx.MapRight)
            outcode2 |= ClipRight;

        if ((outcode1 & outcode2) != 0)
        {
            clippedA = default;
            clippedB = default;
            return false;
        }

        var flA = new FramePoint(MapXToFrame(a.X, ctx), MapYToFrame(a.Y, ctx));
        var flB = new FramePoint(MapXToFrame(b.X, ctx), MapYToFrame(b.Y, ctx));

        outcode1 = ComputeOutcode(flA.X, flA.Y, ctx);
        outcode2 = ComputeOutcode(flB.X, flB.Y, ctx);

        if ((outcode1 & outcode2) != 0)
        {
            clippedA = default;
            clippedB = default;
            return false;
        }

        while (outcode1 != 0 || outcode2 != 0)
        {
            var outside = outcode1 != 0 ? outcode1 : outcode2;
            double tmpX;
            double tmpY;

            if ((outside & ClipTop) != 0)
            {
                var dy = flA.Y - flB.Y;
                var dx = flB.X - flA.X;
                tmpX = flA.X + Scale(dx, flA.Y, dy);
                tmpY = 0;
            }
            else if ((outside & ClipBottom) != 0)
            {
                var dy = flA.Y - flB.Y;
                var dx = flB.X - flA.X;
                tmpX = flA.X + Scale(dx, flA.Y - ctx.FrameHeight, dy);
                tmpY = ctx.FrameHeight - 1;
            }
            else if ((outside & ClipRight) != 0)
            {
                var dy = flB.Y - flA.Y;
                var dx = flB.X - flA.X;
                tmpY = flA.Y + Scale(dy, ctx.FrameWidth - 1 - flA.X, dx);
                tmpX = ctx.FrameWidth - 1;
            }
            else
            {
                var dy = flB.Y - flA.Y;
                var dx = flB.X - flA.X;
                tmpY = flA.Y + Scale(dy, -flA.X, dx);
                tmpX = 0;
            }

            if (outside == outcode1)
            {
                flA = new FramePoint(tmpX, tmpY);
                outcode1 = ComputeOutcode(flA.X, flA.Y, ctx);
            }
            else
            {
                flB = new FramePoint(tmpX, tmpY);
                outcode2 = ComputeOutcode(flB.X, flB.Y, ctx);
            }

            if ((outcode1 & outcode2) != 0)
            {
                clippedA = default;
                clippedB = default;
                return false;
            }
        }

        clippedA = flA;
        clippedB = flB;
        return true;
    }

    public static double Scale(double dx, double num, double dy)
    {
        if (dy == 0)
            return 0;
        return dx * num / dy;
    }

    public readonly struct ScreenLine
    {
        public ScreenLine(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public int X1 { get; }
        public int Y1 { get; }
        public int X2 { get; }
        public int Y2 { get; }
    }

    public enum DrawLineKind
    {
        Thin,
        Thick,
        AntialiasedThin,
        AntialiasedThick,
    }

    public readonly struct DrawLinePlan
    {
        public DrawLinePlan(
            DrawLineKind kind,
            int thickness,
            int alpha255,
            ScreenLine screenLine)
        {
            Kind = kind;
            Thickness = thickness;
            Alpha255 = alpha255;
            ScreenLine = screenLine;
        }

        public DrawLineKind Kind { get; }
        public int Thickness { get; }
        public int Alpha255 { get; }
        public ScreenLine ScreenLine { get; }
    }

    public static int ResolveLineThickness(int amLineThickness, int lineThicknessScaled) =>
        amLineThickness > 0 ? amLineThickness : lineThicknessScaled;

    public static DrawLinePlan PlanDrawLine(
        MapPoint mapA,
        MapPoint mapB,
        in ClipContext ctx,
        int frameOffsetX,
        int frameOffsetY,
        int amLineThickness,
        int lineThicknessScaled,
        bool antialiasing,
        int amLineAlpha)
    {
        if (!TryClipMapLine(mapA, mapB, ctx, out var a, out var b))
            return default;

        var thickness = ResolveLineThickness(amLineThickness, lineThicknessScaled);
        var screen = new ScreenLine(
            frameOffsetX + (int)a.X,
            frameOffsetY + (int)a.Y,
            frameOffsetX + (int)b.X,
            frameOffsetY + (int)b.Y);

        if (!antialiasing)
        {
            var kind = thickness >= 2 ? DrawLineKind.Thick : DrawLineKind.Thin;
            return new DrawLinePlan(kind, thickness, amLineAlpha * 255, screen);
        }

        var aaDivide = thickness >= 3 ? 3 : 2;
        var aaThickness = Math.Max(1, thickness - 2);
        var alpha = amLineAlpha * 255 / aaDivide;
        var aaKind = aaThickness >= 2 ? DrawLineKind.AntialiasedThick : DrawLineKind.AntialiasedThin;
        return new DrawLinePlan(aaKind, aaThickness, alpha, screen);
    }
}
