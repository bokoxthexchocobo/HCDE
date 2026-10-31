namespace HCDE.Playsim;

/// <summary>
/// Rough search, vanilla side tests, and BSP walks from <c>p_maputl.cpp</c>
/// (<b>1820–2106</b>). Chunk <b>1160</b>. The file ends at <b>2106</b>.
/// </summary>
public static class VmPMaputlSide1160Slice
{
    public const int CompatSide = 0x00000800;
    public const double Epsilon = 1d / 65536d;
    const int Sign = unchecked((int)0x80000000);

    public readonly record struct Node(int X, int Y, int Dx, int Dy, int Child0, int Child1);

    public readonly record struct Search(
        int FoundId,
        bool Front,
        double FrontX,
        double FrontY,
        double FrontDx,
        double FrontDy,
        bool OnlySeekable,
        double Fov,
        int Distance);

    public static Search RoughSearch(
        bool hasActor,
        double x,
        double y,
        double yaw,
        int distance,
        bool onlySeekable,
        bool frontOnly,
        double fov,
        int foundId)
    {
        if (!hasActor)
            return new Search(-1, false, 0, 0, 0, 0, false, 0, distance);

        double frontDx = 0;
        double frontDy = 0;
        if (frontOnly)
        {
            frontDx = -SinDeg(yaw);
            frontDy = -CosDeg(yaw);
        }

        return new Search(foundId, frontOnly, x, y, frontDx, frontDy, onlySeekable, fov, distance);
    }

    public static int SlowSide(double x, double y, int nodeX, int nodeY, int nodeDx, int nodeDy)
    {
        int fx = FloatToFixed(x);
        int fy = FloatToFixed(y);
        if (nodeDx == 0)
        {
            if (fx <= nodeX)
                return nodeDy > 0 ? 1 : 0;
            return nodeDy < 0 ? 1 : 0;
        }

        if (nodeDy == 0)
        {
            if (fy <= nodeY)
                return nodeDx < 0 ? 1 : 0;
            return nodeDx > 0 ? 1 : 0;
        }

        int pointDx = unchecked(fx - nodeX);
        int pointDy = unchecked(fy - nodeY);
        int mixed = nodeDy ^ nodeDx ^ pointDx ^ pointDy;
        if ((mixed & Sign) != 0)
        {
            if (((nodeDy ^ pointDx) & Sign) != 0)
                return 1;
            return 0;
        }

        double left = (double)nodeDy * pointDx;
        double right = (double)pointDy * nodeDx;
        if (right < left)
            return 0;
        return 1;
    }

    public static int PreciseSide(int x, int y, int nodeX, int nodeY, int nodeDx, int nodeDy)
    {
        int product = DMulScale(unchecked(y - nodeY), nodeDx, unchecked(nodeX - x), nodeDy, 32);
        if (product > 0)
            return 1;
        return 0;
    }

    public static int VanillaSide(double x, double y, double originX, double originY, double deltaX, double deltaY)
    {
        if (deltaX == 0)
        {
            if (x <= originX)
                return deltaY > 0 ? 1 : 0;
            return deltaY < 0 ? 1 : 0;
        }

        if (deltaY == 0)
        {
            if (y <= originY)
                return deltaX < 0 ? 1 : 0;
            return deltaX > 0 ? 1 : 0;
        }

        int dx = FloatToFixed(x - originX);
        int dy = FloatToFixed(y - originY);
        int left = MulScale((int)(deltaY * 256d), dx, 16);
        int right = MulScale(dy, (int)(deltaX * 256d), 16);
        if (right < left)
            return 0;
        return 1;
    }

    public static int PointSide(
        double x,
        double y,
        double originX,
        double originY,
        double deltaX,
        double deltaY,
        bool compat,
        bool precise)
    {
        if (precise || !compat)
            return PrecisePoint(x, y, originX, originY, deltaX, deltaY);
        return VanillaSide(x, y, originX, originY, deltaX, deltaY);
    }

    public static int Subsector(double x, double y, IReadOnlyList<Node> nodes)
    {
        if (nodes.Count == 0)
            return 0;
        return Walk(nodes, x, y, false);
    }

    public static int SectorBuggy(double x, double y, int emptySector, IReadOnlyList<Node> nodes, IReadOnlyList<int> sectors)
    {
        if (nodes.Count == 0)
            return emptySector;
        return sectors[Walk(nodes, x, y, true)];
    }

    public static int RenderSubsector(int x, int y, IReadOnlyList<Node> nodes)
    {
        if (nodes.Count == 0)
            return 0;
        return WalkFixed(nodes, x, y);
    }

    public static int BoxSide(
        double left,
        double bottom,
        double right,
        double top,
        double originX,
        double originY,
        double deltaX,
        double deltaY,
        bool compat)
    {
        int first;
        int second;
        if (deltaX == 0)
        {
            first = right < originX ? 1 : 0;
            second = left < originX ? 1 : 0;
            if (deltaY < 0)
            {
                first ^= 1;
                second ^= 1;
            }
        }
        else if (deltaY == 0)
        {
            first = top > originY ? 1 : 0;
            second = bottom > originY ? 1 : 0;
            if (deltaX < 0)
            {
                first ^= 1;
                second ^= 1;
            }
        }
        else if ((deltaX * deltaY) >= 0)
        {
            first = PointSide(left, top, originX, originY, deltaX, deltaY, compat, false);
            second = PointSide(right, bottom, originX, originY, deltaX, deltaY, compat, false);
        }
        else
        {
            first = PointSide(right, top, originX, originY, deltaX, deltaY, compat, false);
            second = PointSide(left, bottom, originX, originY, deltaX, deltaY, compat, false);
        }

        if (first == second)
            return first;
        return -1;
    }

    public static int ActorSide(
        double x,
        double y,
        double radius,
        double originX,
        double originY,
        double deltaX,
        double deltaY,
        bool compat)
    {
        return BoxSide(x - radius, y - radius, x + radius, y + radius, originX, originY, deltaX, deltaY, compat);
    }

    public static int FloatToFixed(double value)
    {
        return (int)Math.Round(value * 65536d, MidpointRounding.ToEven);
    }

    private static int Walk(IReadOnlyList<Node> nodes, double x, double y, bool slow)
    {
        int index = 0;
        while (true)
        {
            Node node = nodes[index];
            int side = slow
                ? SlowSide(x, y, node.X, node.Y, node.Dx, node.Dy)
                : PreciseSide(FloatToFixed(x), FloatToFixed(y), node.X, node.Y, node.Dx, node.Dy);
            int child = side == 0 ? node.Child0 : node.Child1;
            if ((child & 1) != 0)
                return child >> 1;
            index = child >> 1;
        }
    }

    private static int WalkFixed(IReadOnlyList<Node> nodes, int x, int y)
    {
        int index = 0;
        while (true)
        {
            Node node = nodes[index];
            int side = PreciseSide(x, y, node.X, node.Y, node.Dx, node.Dy);
            int child = side == 0 ? node.Child0 : node.Child1;
            if ((child & 1) != 0)
                return child >> 1;
            index = child >> 1;
        }
    }

    private static int PrecisePoint(double x, double y, double originX, double originY, double deltaX, double deltaY)
    {
        double cross = ((y - originY) * deltaX) + ((originX - x) * deltaY);
        if (cross > Epsilon)
            return 1;
        return 0;
    }

    private static int MulScale(int a, int b, int shift)
    {
        return (int)(((long)a * b) >> shift);
    }

    private static int DMulScale(int a, int b, int c, int d, int shift)
    {
        long value = ((long)a * b) + ((long)c * d);
        return (int)(value >> shift);
    }

    private static double CosDeg(double degrees)
    {
        return Math.Cos(degrees * (Math.PI / 180d));
    }

    private static double SinDeg(double degrees)
    {
        return Math.Sin(degrees * (Math.PI / 180d));
    }
}
