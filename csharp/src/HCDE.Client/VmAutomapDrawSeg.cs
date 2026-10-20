namespace HCDE.Client;

/// <summary><c>DAutomap::drawSeg</c>, <c>drawPolySeg</c>, and <c>showSS</c> policy from <c>am_map.cpp</c> (2346–2410).</summary>
public static class VmAutomapDrawSeg
{
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

    public readonly struct Mline
    {
        public Mline(MapPoint a, MapPoint b)
        {
            A = a;
            B = b;
        }

        public MapPoint A { get; }
        public MapPoint B { get; }
    }

    public static bool ShouldApplyMapRotation(int amRotate, bool viewActive) =>
        amRotate == 1 || (amRotate == 2 && viewActive);

    public static Mline BuildSegLine(double v1x, double v1y, double v2x, double v2y) =>
        new(new MapPoint(v1x, v1y), new MapPoint(v2x, v2y));

    public static Mline BuildPolySegLine(double v1x, double v1y, double v2x, double v2y) =>
        BuildSegLine(v1x, v1y, v2x, v2y);

    public static Mline RotateEndpointsIfNeeded(Mline line, bool rotate, Func<double, double, (double x, double y)> rotatePoint)
    {
        if (!rotate)
            return line;
        var (ax, ay) = rotatePoint(line.A.X, line.A.Y);
        var (bx, by) = rotatePoint(line.B.X, line.B.Y);
        return new Mline(new MapPoint(ax, ay), new MapPoint(bx, by));
    }

    public static bool ShouldDrawSubsectorDebug(int amShowSubsector, int subsectorIndex, int subsectorCount) =>
        amShowSubsector >= 0 && subsectorIndex >= 0 && subsectorIndex < subsectorCount;

    public static IEnumerable<int> EnumerateSubsectorSegIndices(int numLines) =>
        Enumerable.Range(0, numLines);

    public static bool PolyNodeMatchesSubsector(int nodeSubsectorIndex, int targetSubsector) =>
        nodeSubsectorIndex == targetSubsector;
}
