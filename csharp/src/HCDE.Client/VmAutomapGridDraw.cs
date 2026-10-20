namespace HCDE.Client;

/// <summary><c>DAutomap::drawGrid</c> from <c>am_map.cpp</c> (1877–1936).</summary>
public static class VmAutomapGridDraw
{
    public const int MapBlockUnits = 128;

    public readonly struct GridContext
    {
        public GridContext(
            double mapWindowWidth,
            double mapWindowHeight,
            double mapOriginX,
            double mapOriginY,
            double blockmapOriginX,
            double blockmapOriginY,
            int amRotate,
            bool viewActive)
        {
            MapWindowWidth = mapWindowWidth;
            MapWindowHeight = mapWindowHeight;
            MapOriginX = mapOriginX;
            MapOriginY = mapOriginY;
            BlockmapOriginX = blockmapOriginX;
            BlockmapOriginY = blockmapOriginY;
            AmRotate = amRotate;
            ViewActive = viewActive;
        }

        public double MapWindowWidth { get; }
        public double MapWindowHeight { get; }
        public double MapOriginX { get; }
        public double MapOriginY { get; }
        public double BlockmapOriginX { get; }
        public double BlockmapOriginY { get; }
        public int AmRotate { get; }
        public bool ViewActive { get; }
    }

    public readonly struct MapLine
    {
        public MapLine(double ax, double ay, double bx, double by)
        {
            Ax = ax;
            Ay = ay;
            Bx = bx;
            By = by;
        }

        public double Ax { get; }
        public double Ay { get; }
        public double Bx { get; }
        public double By { get; }
    }

    public static bool ShouldRotateGridLines(int amRotate, bool viewActive) =>
        amRotate == 1 || (amRotate == 2 && viewActive);

    public static double ComputeCoveringLength(double mapWidth, double mapHeight) =>
        Math.Sqrt(mapWidth * mapWidth + mapHeight * mapHeight);

    public static double AlignGridStart(double mapCoord, double extension, double blockmapOrigin)
    {
        var start = mapCoord - extension;
        return Math.Ceiling((start - blockmapOrigin) / MapBlockUnits) * MapBlockUnits + blockmapOrigin;
    }

    public static IEnumerable<MapLine> EnumerateVerticalGridLines(in GridContext ctx)
    {
        var minlen = ComputeCoveringLength(ctx.MapWindowWidth, ctx.MapWindowHeight);
        var extx = (minlen - ctx.MapWindowWidth) / 2;
        var exty = (minlen - ctx.MapWindowHeight) / 2;
        var minx = ctx.MapOriginX;
        var miny = ctx.MapOriginY;

        var start = AlignGridStart(minx, extx, ctx.BlockmapOriginX);
        var end = minx + minlen - extx;

        for (var x = start; x < end; x += MapBlockUnits)
        {
            yield return new MapLine(x, miny - exty, x, miny - exty + minlen);
        }
    }

    public static IEnumerable<MapLine> EnumerateHorizontalGridLines(in GridContext ctx)
    {
        var minlen = ComputeCoveringLength(ctx.MapWindowWidth, ctx.MapWindowHeight);
        var extx = (minlen - ctx.MapWindowWidth) / 2;
        var exty = (minlen - ctx.MapWindowHeight) / 2;
        var minx = ctx.MapOriginX;
        var miny = ctx.MapOriginY;

        var start = AlignGridStart(miny, exty, ctx.BlockmapOriginY);
        var end = miny + minlen - exty;

        for (var y = start; y < end; y += MapBlockUnits)
        {
            yield return new MapLine(minx - extx, y, minx - extx + minlen, y);
        }
    }
}
