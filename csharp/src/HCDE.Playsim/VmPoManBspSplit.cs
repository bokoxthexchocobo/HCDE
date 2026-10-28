namespace HCDE.Playsim;

/// <summary><c>GetIntersection</c>, BSP helpers, <c>SplitPoly</c>, subsector links, poly node pool from <c>po_man.cpp</c> (1394–1769).</summary>
public static class VmPoManBspSplit
{
    public const double PolyEpsilon = 0.3125;

    public sealed class PolySeg
    {
        public double V1X { get; set; }
        public double V1Y { get; set; }
        public double V2X { get; set; }
        public double V2Y { get; set; }
        public int WallId { get; set; }
    }

    public sealed class PolyNode
    {
        public int State { get; set; } = 1337;
        public object? Poly { get; set; }
        public List<PolySeg> Segs { get; } = new();
        public PolyNode? PNext { get; set; }
        public PolyNode? PPrev { get; set; }
        public PolyNode? SNext { get; set; }
        public SubsectorHost? Subsector { get; set; }
    }

    public sealed class SubsectorHost
    {
        public PolyNode? Polys { get; set; }
        public bool BspDirty { get; set; }
    }

    public sealed class BspNode
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }
        public double Len { get; set; }
        public object? Child0 { get; set; }
        public object? Child1 { get; set; }
        public float[] Bbox0 { get; } = new float[4];
        public float[] Bbox1 { get; } = new float[4];
    }

    public static bool GetIntersection(PolySeg seg, BspNode bsp, out double ix, out double iy)
    {
        var v2x = seg.V1X;
        var v2y = seg.V1Y;
        var v2dx = seg.V2X - v2x;
        var v2dy = seg.V2Y - v2y;
        var v1x = bsp.X;
        var v1y = bsp.Y;
        var v1dx = bsp.Dx;
        var v1dy = bsp.Dy;

        var den = v1dy * v2dx - v1dx * v2dy;
        if (den == 0)
        {
            ix = iy = 0;
            return false;
        }

        var num = (v1x - v2x) * v1dy + (v2y - v1y) * v1dx;
        var frac = num / den;
        if (frac < 0 || frac > 1)
        {
            ix = iy = 0;
            return false;
        }

        ix = v2x + frac * v2dx;
        iy = v2y + frac * v2dy;
        return true;
    }

    public static double PartitionDistance(double vx, double vy, BspNode node)
    {
        if (node.Len == 0)
            return 0;
        return Math.Abs(-node.Dy * (vx - node.X) + node.Dx * (vy - node.Y)) / node.Len;
    }

    public static void AddToBBox(float[] child, float[] parent)
    {
        if (child[3] > parent[3]) parent[3] = child[3];
        if (child[2] < parent[2]) parent[2] = child[2];
        if (child[0] < parent[0]) parent[0] = child[0];
        if (child[1] > parent[1]) parent[1] = child[1];
    }

    public static void AddToBBoxVertex(double x, double y, float[] bbox)
    {
        var xf = (float)x;
        var yf = (float)y;
        if (xf < bbox[0]) bbox[0] = xf;
        if (xf > bbox[1]) bbox[1] = xf;
        if (yf < bbox[2]) bbox[2] = yf;
        if (yf > bbox[3]) bbox[3] = yf;
    }

    public sealed class SplitPolyContext
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public Func<double, double, BspNode, int>? PointOnSide { get; init; }
        public Func<PolyNode>? NewPolyNode { get; init; }
    }

    public static void SplitPoly(PolyNode pnode, object? node, float[] bbox, SplitPolyContext ctx)
    {
        if (node is not BspNode bsp)
        {
            if (node is SubsectorHost sub)
                LinkPolyNodeToSubsector(pnode, sub, bbox);
            return;
        }

        var centerSide = ctx.PointOnSide?.Invoke(ctx.CenterX, ctx.CenterY, bsp) ?? 0;
        var lists = new[] { new List<PolySeg>(), new List<PolySeg>() };

        foreach (var seg in pnode.Segs)
        {
            var distV1 = PartitionDistance(seg.V1X, seg.V1Y, bsp);
            var distV2 = PartitionDistance(seg.V2X, seg.V2Y, bsp);

            if (distV1 <= PolyEpsilon)
            {
                if (distV2 <= PolyEpsilon)
                    lists[centerSide].Add(CloneSeg(seg));
                else
                {
                    var side = ctx.PointOnSide?.Invoke(seg.V2X, seg.V2Y, bsp) ?? centerSide;
                    lists[side].Add(CloneSeg(seg));
                }
            }
            else if (distV2 <= PolyEpsilon)
            {
                var side = ctx.PointOnSide?.Invoke(seg.V1X, seg.V1Y, bsp) ?? centerSide;
                lists[side].Add(CloneSeg(seg));
            }
            else
            {
                var side1 = ctx.PointOnSide?.Invoke(seg.V1X, seg.V1Y, bsp) ?? 0;
                var side2 = ctx.PointOnSide?.Invoke(seg.V2X, seg.V2Y, bsp) ?? 0;
                if (side1 != side2)
                {
                    if (GetIntersection(seg, bsp, out var ix, out var iy))
                    {
                        lists[0].Add(CloneSeg(seg));
                        lists[1].Add(CloneSeg(seg));
                        lists[side1][^1].V2X = ix;
                        lists[side1][^1].V2Y = iy;
                        lists[side2][^1].V1X = ix;
                        lists[side2][^1].V1Y = iy;
                    }
                    else
                        lists[side1].Add(CloneSeg(seg));
                }
                else
                    lists[side1].Add(CloneSeg(seg));
            }
        }

        if (lists[1].Count == 0)
        {
            SplitPoly(pnode, bsp.Child0, bsp.Bbox0, ctx);
            AddToBBox(bsp.Bbox0, bbox);
        }
        else if (lists[0].Count == 0)
        {
            SplitPoly(pnode, bsp.Child1, bsp.Bbox1, ctx);
            AddToBBox(bsp.Bbox1, bbox);
        }
        else
        {
            var newNode = ctx.NewPolyNode?.Invoke() ?? new PolyNode();
            newNode.Poly = pnode.Poly;
            newNode.Segs.Clear();
            newNode.Segs.AddRange(lists[1]);
            pnode.Segs.Clear();
            pnode.Segs.AddRange(lists[0]);

            SplitPoly(newNode, bsp.Child1, bsp.Bbox1, ctx);
            SplitPoly(pnode, bsp.Child0, bsp.Bbox0, ctx);
            AddToBBox(bsp.Bbox0, bbox);
            AddToBBox(bsp.Bbox1, bbox);
        }
    }

    static PolySeg CloneSeg(PolySeg seg) =>
        new() { V1X = seg.V1X, V1Y = seg.V1Y, V2X = seg.V2X, V2Y = seg.V2Y, WallId = seg.WallId };

    static void LinkPolyNodeToSubsector(PolyNode pnode, SubsectorHost sub, float[] bbox)
    {
        pnode.PNext = sub.Polys;
        if (pnode.PNext is not null)
            pnode.PNext.PPrev = pnode;
        pnode.PPrev = null;
        sub.Polys = pnode;

        if (pnode.Poly is PolySubsectorLinkHost host)
        {
            pnode.SNext = host.SubsectorLinks;
            host.SubsectorLinks = pnode;
        }

        pnode.Subsector = sub;

        if (pnode.Segs.Count == 0)
            return;

        var subbbox = new float[] { float.MinValue, float.MaxValue, float.MaxValue, float.MinValue };
        foreach (var seg in pnode.Segs)
        {
            AddToBBoxVertex(seg.V1X, seg.V1Y, subbbox);
            AddToBBoxVertex(seg.V2X, seg.V2Y, subbbox);
        }

        AddToBBox(subbbox, bbox);
    }

    public sealed class PolySubsectorLinkHost
    {
        public PolyNode? SubsectorLinks { get; set; }
    }

    public sealed class CreateSubsectorLinksContext
    {
        public bool CompatPolyobj { get; set; }
        public object? HeadNode { get; set; }
        public SubsectorHost? CenterSubsector { get; set; }
        public List<(int SideIndex, double V1X, double V1Y, double V2X, double V2Y)> Sides { get; } = new();
        public PolySubsectorLinkHost Poly { get; } = new();
        public SplitPolyContext Split { get; } = new();
        public Action? ClearSubsectorLinks { get; init; }
    }

    public static void CreateSubsectorLinks(CreateSubsectorLinksContext ctx)
    {
        var node = RentPolyNode();
        var dummybbox = new float[4];
        node.Poly = ctx.Poly;
        node.Segs.Clear();
        foreach (var side in ctx.Sides)
        {
            node.Segs.Add(new PolySeg
            {
                V1X = side.V1X,
                V1Y = side.V1Y,
                V2X = side.V2X,
                V2Y = side.V2Y,
                WallId = side.SideIndex,
            });
        }

        if (!ctx.CompatPolyobj)
        {
            SplitPoly(node, ctx.HeadNode, dummybbox, ctx.Split);
            return;
        }

        var sub = ctx.CenterSubsector;
        if (sub is null)
            return;

        node.PNext = sub.Polys;
        if (node.PNext is not null)
            node.PNext.PPrev = node;
        node.PPrev = null;
        sub.Polys = node;
        node.SNext = ctx.Poly.SubsectorLinks;
        ctx.Poly.SubsectorLinks = node;
        node.Subsector = sub;
    }

    public static void PoLinkToSubsectors(IEnumerable<CreateSubsectorLinksContext> polys)
    {
        foreach (var poly in polys)
        {
            if (poly.Poly.SubsectorLinks is null)
                CreateSubsectorLinks(poly);
        }
    }

    static PolyNode? FreePolyNodes;

    public static PolyNode RentPolyNode()
    {
        PolyNode node;
        if (FreePolyNodes is not null)
        {
            node = FreePolyNodes;
            FreePolyNodes = node.PNext;
        }
        else
            node = new PolyNode();

        node.State = 1337;
        node.Poly = null;
        node.PNext = null;
        node.PPrev = null;
        node.Subsector = null;
        node.SNext = null;
        node.Segs.Clear();
        return node;
    }

    public static void FreePolyNode(PolyNode node)
    {
        node.Segs.Clear();
        node.PNext = FreePolyNodes;
        FreePolyNodes = node;
    }

    public static void ReleaseAllPolyNodes()
    {
        while (FreePolyNodes is not null)
        {
            var next = FreePolyNodes.PNext;
            FreePolyNodes = next;
        }
    }
}
