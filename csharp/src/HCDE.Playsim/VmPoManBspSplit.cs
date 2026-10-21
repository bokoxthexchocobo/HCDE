namespace HCDE.Playsim;

/// <summary>BSP helpers for polyobject subsectors from <c>po_man.cpp</c> (1394–1495, 1652–1718, 1720–1769).</summary>
public static class VmPoManBspSplit
{
    public static double PartitionDistance(double vx, double vy, double nodeX, double nodeY, double nodeDx, double nodeDy)
    {
        var den = nodeDx * nodeDx + nodeDy * nodeDy;
        if (den == 0)
            return 0;
        return ((vx - nodeX) * nodeDx + (vy - nodeY) * nodeDy) / den;
    }

    public static void AddVertexToBBox(double x, double y, ref double left, ref double right, ref double bottom, ref double top)
    {
        if (x < left) left = x;
        if (x > right) right = x;
        if (y < bottom) bottom = y;
        if (y > top) top = y;
    }

    public static void MergeBboxes(
        double childLeft, double childRight, double childBottom, double childTop,
        ref double left, ref double right, ref double bottom, ref double top)
    {
        if (childLeft < left) left = childLeft;
        if (childRight > right) right = childRight;
        if (childBottom < bottom) bottom = childBottom;
        if (childTop > top) top = childTop;
    }

    public sealed class CreateSubsectorLinksContext
    {
        public List<(double X, double Y)> Vertices { get; } = new();
        public Action? ClearSubsectorLinks { get; init; }
        public Action<double, double, double, double>? SplitPoly { get; init; }
    }

    public static void CreateSubsectorLinks(CreateSubsectorLinksContext ctx)
    {
        ctx.ClearSubsectorLinks?.Invoke();
        if (ctx.Vertices.Count == 0)
            return;

        var left = double.PositiveInfinity;
        var right = double.NegativeInfinity;
        var bottom = double.PositiveInfinity;
        var top = double.NegativeInfinity;
        foreach (var (x, y) in ctx.Vertices)
            AddVertexToBBox(x, y, ref left, ref right, ref bottom, ref top);

        ctx.SplitPoly?.Invoke(left, right, bottom, top);
    }

    public static void PoLinkToSubsectors(IEnumerable<CreateSubsectorLinksContext> polys)
    {
        foreach (var poly in polys)
            CreateSubsectorLinks(poly);
    }

    public sealed class PolyNodePool
    {
        public int Allocated { get; private set; }
        public int Freed { get; private set; }
        private readonly Stack<int> _free = new();

        public int Rent()
        {
            if (_free.Count > 0)
            {
                _free.Pop();
                return ++Allocated;
            }

            Allocated++;
            return Allocated;
        }

        public void Return(int id)
        {
            _free.Push(id);
            Freed++;
        }

        public void ReleaseAll()
        {
            _free.Clear();
            Freed += Allocated;
            Allocated = 0;
        }
    }
}
