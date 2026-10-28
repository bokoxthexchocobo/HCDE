namespace HCDE.Playsim;

/// <summary>Point-on-side and subsector lookup from <c>p_maputl.cpp</c> (1846–2067).</summary>
public static class VmPMapUtlPointSubsector
{
    public sealed class BspNode
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }
        public object? Child0 { get; set; }
        public object? Child1 { get; set; }
        public bool Child0IsSubsector { get; set; }
        public bool Child1IsSubsector { get; set; }
    }

    public static int PointOnSideSlow(double xx, double yy, BspNode node)
    {
        if (node.Dx == 0)
            return xx <= node.X ? (node.Dy > 0 ? 1 : 0) : (node.Dy < 0 ? 1 : 0);
        if (node.Dy == 0)
            return yy <= node.Y ? (node.Dx < 0 ? 1 : 0) : (node.Dx > 0 ? 1 : 0);

        var x = AcsFixedPoint.FromDouble(xx);
        var y = AcsFixedPoint.FromDouble(yy);
        var dx = x - AcsFixedPoint.FromDouble(node.X);
        var dy = y - AcsFixedPoint.FromDouble(node.Y);
        var ndx = AcsFixedPoint.FromDouble(node.Dx);
        var ndy = AcsFixedPoint.FromDouble(node.Dy);
        if (((ndy ^ ndx ^ dx ^ dy) & unchecked((int)0x80000000)) != 0)
            return (((ndy ^ dx) & unchecked((int)0x80000000)) != 0) ? 1 : 0;

        var left = (double)ndy * dx;
        var right = (double)dy * ndx;
        return right < left ? 0 : 1;
    }

    public static int VanillaPointOnLineSide(double x, double y, double v1X, double v1Y, double deltaX, double deltaY)
    {
        if (deltaX == 0)
            return x <= v1X ? (deltaY > 0 ? 1 : 0) : (deltaY < 0 ? 1 : 0);
        if (deltaY == 0)
            return y <= v1Y ? (deltaX < 0 ? 1 : 0) : (deltaX > 0 ? 1 : 0);

        var dx = (int)((x - v1X) * 65536);
        var dy = (int)((y - v1Y) * 65536);
        var left = (int)(deltaY * 256) * dx >> 16;
        var right = dy * (int)(deltaX * 256) >> 16;
        return right < left ? 0 : 1;
    }

    public static object? PointInSubsector(double x, double y, BspNode? root, Func<double, double, BspNode, int> pointOnSide)
    {
        var node = root;
        if (node is null)
            return 0;
        while (true)
        {
            var side = pointOnSide(x, y, node);
            var child = side == 0 ? node.Child0 : node.Child1;
            var isLeaf = side == 0 ? node.Child0IsSubsector : node.Child1IsSubsector;
            if (isLeaf)
                return child;
            node = (BspNode)child!;
        }
    }

    public static object? PointInSectorBuggy(double x, double y, BspNode? root, Func<double, double, BspNode, int> slowPointOnSide, Func<object?, object?> subsectorToSector)
    {
        var node = root;
        if (node is null)
            return null;
        while (true)
        {
            var side = slowPointOnSide(x, y, node);
            var child = side == 0 ? node.Child0 : node.Child1;
            var isLeaf = side == 0 ? node.Child0IsSubsector : node.Child1IsSubsector;
            if (isLeaf)
                return subsectorToSector(child);
            node = (BspNode)child!;
        }
    }

    public static object? PointInRenderSubsector(int x, int y, BspNode? root, Func<int, int, BspNode, int> pointOnSide)
    {
        if (root is null)
            return 0;
        var node = root;
        while (true)
        {
            var side = pointOnSide(x, y, node);
            var child = side == 0 ? node.Child0 : node.Child1;
            var isLeaf = side == 0 ? node.Child0IsSubsector : node.Child1IsSubsector;
            if (isLeaf)
                return child;
            node = (BspNode)child!;
        }
    }

    public static int BoxOnLineSide(double left, double top, double right, double bottom, double v1X, double v1Y, double deltaX, double deltaY, Func<double, double, double, double, double, double, int> pointOnLineSide)
    {
        int p1, p2;
        if (deltaX == 0)
        {
            p1 = right < v1X ? 1 : 0;
            p2 = left < v1X ? 1 : 0;
            if (deltaY < 0)
            {
                p1 ^= 1;
                p2 ^= 1;
            }
        }
        else if (deltaY == 0)
        {
            p1 = top > v1Y ? 1 : 0;
            p2 = bottom > v1Y ? 1 : 0;
            if (deltaX < 0)
            {
                p1 ^= 1;
                p2 ^= 1;
            }
        }
        else if (deltaX * deltaY >= 0)
        {
            p1 = pointOnLineSide(left, top, v1X, v1Y, deltaX, deltaY);
            p2 = pointOnLineSide(right, bottom, v1X, v1Y, deltaX, deltaY);
        }
        else
        {
            p1 = pointOnLineSide(right, top, v1X, v1Y, deltaX, deltaY);
            p2 = pointOnLineSide(left, bottom, v1X, v1Y, deltaX, deltaY);
        }

        return p1 == p2 ? p1 : -1;
    }
}
