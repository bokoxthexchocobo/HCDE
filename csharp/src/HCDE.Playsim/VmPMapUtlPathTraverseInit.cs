namespace HCDE.Playsim;

/// <summary><c>FPathTraverse::init</c> from <c>p_maputl.cpp</c> (1397–1604).</summary>
public static class VmPMapUtlPathTraverseInit
{
    public const int PtDelta = 1;
    public const int PtCompatible = 1 << 1;
    public const int PtAddLines = 1 << 2;
    public const int PtAddThings = 1 << 3;
    public const int CompatfHitscan = 1;

    public sealed class PathTraverseInitContext
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public int Flags { get; set; }
        public double StartFrac { get; set; }
        public double BmapOrgX { get; set; }
        public double BmapOrgY { get; set; }
        public int LevelCompatFlags { get; set; }
        public int ValidCount { get; set; }
        public int InterceptIndex { get; set; }
        public double TraceX { get; set; }
        public double TraceY { get; set; }
        public double TraceDx { get; set; }
        public double TraceDy { get; set; }

        public Action<int, int>? AddLineIntercepts { get; init; }
        public Action<int, int, bool>? AddThingIntercepts { get; init; }
    }

    public const double MapBlockUnits = 128.0;

    public static void PathTraverseInit(PathTraverseInitContext ctx)
    {
        var x1 = ctx.X1;
        var y1 = ctx.Y1;
        var x2 = ctx.X2;
        var y2 = ctx.Y2;

        ctx.TraceX = x1;
        ctx.TraceY = y1;
        if ((ctx.Flags & PtDelta) != 0)
        {
            ctx.TraceDx = x2;
            ctx.TraceDy = y2;
        }
        else
        {
            ctx.TraceDx = x2 - x1;
            ctx.TraceDy = y2 - y1;
        }

        if (ctx.StartFrac > 0)
        {
            var startdx = ctx.TraceDx * ctx.StartFrac;
            var startdy = ctx.TraceDy * ctx.StartFrac;
            x1 += startdx;
            y1 += startdy;
            x2 = ctx.TraceDx - startdx;
            y2 = ctx.TraceDy - startdy;
            ctx.Flags |= PtDelta;
        }

        ctx.ValidCount++;
        ctx.InterceptIndex = 0;

        if ((ctx.Flags & PtDelta) != 0)
        {
            x2 += x1;
            y2 += y1;
        }

        x1 -= ctx.BmapOrgX;
        y1 -= ctx.BmapOrgY;
        var xt1 = x1 / MapBlockUnits;
        var yt1 = y1 / MapBlockUnits;

        x2 -= ctx.BmapOrgX;
        y2 -= ctx.BmapOrgY;
        var xt2 = x2 / MapBlockUnits;
        var yt2 = y2 / MapBlockUnits;

        var mapx = RoundDown(xt1);
        var mapy = RoundDown(yt1);
        var mapex = RoundDown(xt2);
        var mapey = RoundDown(yt2);

        int mapxstep, mapystep;
        double xstep, ystep, partialx, partialy, xintercept, yintercept;

        if (mapex > mapx)
        {
            mapxstep = 1;
            partialx = 1.0 - xt1 + RoundDown(xt1);
            ystep = (y2 - y1) / Math.Abs(x2 - x1);
        }
        else if (mapex < mapx)
        {
            mapxstep = -1;
            partialx = xt1 - RoundDown(xt1);
            ystep = (y2 - y1) / Math.Abs(x2 - x1);
        }
        else
        {
            mapxstep = 0;
            partialx = 1.0;
            ystep = 256;
        }

        yintercept = yt1 + partialx * ystep;

        if (mapey > mapy)
        {
            mapystep = 1;
            partialy = 1.0 - yt1 + RoundDown(yt1);
            xstep = (x2 - x1) / Math.Abs(y2 - y1);
        }
        else if (mapey < mapy)
        {
            mapystep = -1;
            partialy = yt1 - RoundDown(yt1);
            xstep = (x2 - x1) / Math.Abs(y2 - y1);
        }
        else
        {
            mapystep = 0;
            partialy = 1.0;
            xstep = 256;
        }

        xintercept = xt1 + partialy * xstep;

        if (Math.Abs(xstep) == 1.0 && Math.Abs(ystep) == 1.0)
        {
            if (ystep < 0) partialx = 1.0 - partialx;
            if (xstep < 0) partialy = 1.0 - partialy;
            if (partialx == partialy)
            {
                xintercept = xt1;
                yintercept = yt1;
            }
        }

        var compatible = (ctx.Flags & PtCompatible) != 0 && (ctx.LevelCompatFlags & CompatfHitscan) != 0;

        for (var count = 0; count < 1000; count++)
        {
            if ((ctx.Flags & PtAddLines) != 0)
                ctx.AddLineIntercepts?.Invoke(mapx, mapy);

            if ((ctx.Flags & PtAddThings) != 0)
                ctx.AddThingIntercepts?.Invoke(mapx, mapy, compatible);

            if ((mapxstep | mapystep) == 0)
                break;

            switch ((RoundDown(yintercept) == mapy ? 2 : 0) | (RoundDown(xintercept) == mapx ? 1 : 0))
            {
                case 0:
                    return;
                case 1:
                    xintercept += xstep;
                    mapy += mapystep;
                    if (mapy == mapey) mapystep = 0;
                    break;
                case 2:
                    yintercept += ystep;
                    mapx += mapxstep;
                    if (mapx == mapex) mapxstep = 0;
                    break;
                case 3:
                    if (!compatible)
                    {
                        if ((ctx.Flags & PtAddLines) != 0)
                        {
                            ctx.AddLineIntercepts?.Invoke(mapx + mapxstep, mapy);
                            ctx.AddLineIntercepts?.Invoke(mapx, mapy + mapystep);
                        }

                        if ((ctx.Flags & PtAddThings) != 0)
                        {
                            ctx.AddThingIntercepts?.Invoke(mapx + mapxstep, mapy, false);
                            ctx.AddThingIntercepts?.Invoke(mapx, mapy + mapystep, false);
                        }

                        xintercept += xstep;
                        yintercept += ystep;
                        mapx += mapxstep;
                        mapy += mapystep;
                        if (mapx == mapex) mapxstep = 0;
                        if (mapy == mapey) mapystep = 0;
                    }
                    else
                    {
                        return;
                    }

                    break;
            }
        }
    }

    public static int RoundDown(double v) => (int)Math.Floor(v);
}
