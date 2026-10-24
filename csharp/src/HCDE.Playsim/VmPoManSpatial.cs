namespace HCDE.Playsim;

/// <summary><c>FPolyObj::ClosestPoint</c>, <c>PO_Busy</c>, subsector unlink helpers from <c>po_man.cpp</c> (1259–1384).</summary>
public static class VmPoManSpatial
{
    public sealed class ClosestPointContext
    {
        public double QueryX { get; set; }
        public double QueryY { get; set; }
        public List<PolySide> Sides { get; } = new();
    }

    public sealed class PolySide
    {
        public double V1X { get; set; }
        public double V1Y { get; set; }
        public double V2X { get; set; }
        public double V2Y { get; set; }
        public int SideId { get; set; }
    }

    public static (double X, double Y, int SideId) ClosestPoint(ClosestPointContext ctx)
    {
        var bestDist = double.PositiveInfinity;
        var bestX = 0.0;
        var bestY = 0.0;
        var bestSide = -1;

        foreach (var side in ctx.Sides)
        {
            var a = side.V2X - side.V1X;
            var b = side.V2Y - side.V1Y;
            var den = a * a + b * b;
            double ix, iy;
            if (den == 0)
            {
                ix = side.V1X;
                iy = side.V1Y;
            }
            else
            {
                var num = (ctx.QueryX - side.V1X) * a + (ctx.QueryY - side.V1Y) * b;
                var u = num / den;
                if (u <= 0)
                {
                    ix = side.V1X;
                    iy = side.V1Y;
                }
                else if (u >= 1)
                {
                    ix = side.V2X;
                    iy = side.V2Y;
                }
                else
                {
                    ix = side.V1X + u * a;
                    iy = side.V1Y + u * b;
                }
            }

            var dx = ix - ctx.QueryX;
            var dy = iy - ctx.QueryY;
            var dist = dx * dx + dy * dy;
            if (dist < bestDist)
            {
                bestDist = dist;
                bestX = ix;
                bestY = iy;
                bestSide = side.SideId;
            }
        }

        return (bestX, bestY, bestSide);
    }

    public static bool PoBusy(bool polyFound, bool hasSpecialData) =>
        polyFound && hasSpecialData;

    public sealed class SubsectorLinkNode
    {
        public SubsectorLinkNode? SNext { get; set; }
        public SubsectorLinkNode? PNext { get; set; }
        public SubsectorLinkNode? PPrev { get; set; }
        public SubsectorLink? Owner { get; set; }
        public int State { get; set; } = 1337;
        public bool BspDirty { get; set; }
    }

    public sealed class SubsectorLink
    {
        public SubsectorLinkNode? Head { get; set; }
        public bool BspDirty { get; set; }
    }

    public static void ClearSubsectorLinks(SubsectorLink link, Action<SubsectorLinkNode>? releaseNode = null)
    {
        while (link.Head is not null)
        {
            var node = link.Head;
            link.Head = node.SNext;

            if (node.PNext is not null)
                node.PNext.PPrev = node.PPrev;
            if (node.PPrev is not null)
                node.PPrev.PNext = node.PNext;
            else if (node.Owner is not null)
                node.Owner.Head = node.PNext;

            if (node.Owner?.BspDirty == false && node.BspDirty)
                node.Owner.BspDirty = true;

            node.State = -1;
            releaseNode?.Invoke(node);
        }

        link.Head = null;
    }

    public static void ClearAllSubsectorLinks(IEnumerable<SubsectorLink> polys, Action? releaseAllNodes = null)
    {
        foreach (var poly in polys)
            ClearSubsectorLinks(poly);
        releaseAllNodes?.Invoke();
    }
}
