namespace HCDE.Playsim;

/// <summary><c>FPolyObj::UpdateLinks</c>, <c>UpdateBBox</c>, <c>CalcCenter</c>, <c>UnLinkPolyobj</c>, <c>LinkPolyobj</c>, <c>RecalcActorFloorCeil</c> from <c>po_man.cpp</c> (803–856, 990–1017, 1154–1217, 1228–1248).</summary>
public static class VmPoManLinkUpdate
{
    public sealed class PortalDisplacement
    {
        public bool IsLinkedPortal { get; init; }
        public double OldDx { get; set; }
        public double OldDy { get; set; }
        public double NewDx { get; set; }
        public double NewDy { get; set; }
        public int DestPortalGroup { get; init; }
        public bool GroupProcessed { get; set; }
    }

    public sealed class UpdateLinksContext
    {
        public int PortalLinkMode { get; set; }
        public List<PortalDisplacement> Portals { get; } = new();
        public Action<int, double, double>? MoveDisplacementGroup { get; init; }
    }

    public static void UpdateLinks(UpdateLinksContext ctx)
    {
        if (ctx.PortalLinkMode != 2)
            return;

        var processed = new HashSet<int>();
        foreach (var port in ctx.Portals)
        {
            if (!port.IsLinkedPortal)
                continue;

            var deltaX = port.NewDx - port.OldDx;
            var deltaY = port.NewDy - port.OldDy;
            if (!processed.Add(port.DestPortalGroup))
                continue;

            ctx.MoveDisplacementGroup?.Invoke(port.DestPortalGroup, deltaX, deltaY);
        }
    }

    public static void CalcCenter(List<(double X, double Y)> vertices, out double centerX, out double centerY)
    {
        centerX = 0;
        centerY = 0;
        if (vertices.Count == 0)
            return;

        foreach (var v in vertices)
        {
            centerX += v.X;
            centerY += v.Y;
        }

        centerX /= vertices.Count;
        centerY /= vertices.Count;
    }

    public static void UpdateBBox(UpdateBBoxContext ctx)
    {
        foreach (var idx in ctx.LineIndices)
            ctx.AdjustLine?.Invoke(idx);
        CalcCenter(ctx.Vertices, out var cx, out var cy);
        ctx.CenterX = cx;
        ctx.CenterY = cy;
    }

    public sealed class UpdateBBoxContext
    {
        public List<int> LineIndices { get; } = new();
        public List<(double X, double Y)> Vertices { get; } = new();
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public Action<int>? AdjustLine { get; init; }
    }

    public sealed class UnlinkPolyobjContext
    {
        public int BoxLeft { get; set; }
        public int BoxRight { get; set; }
        public int BoxTop { get; set; }
        public int BoxBottom { get; set; }
        public int BlockWidth { get; set; }
        public int BlockHeight { get; set; }
        public Func<int, int, int?>? GetPolyAtCell { get; init; }
        public Action<int, int>? ClearPolyAtCell { get; init; }
    }

    public static void UnlinkPolyobj(UnlinkPolyobjContext ctx, int selfPolyId)
    {
        for (var j = ctx.BoxBottom; j <= ctx.BoxTop; j++)
        {
            var index = j * ctx.BlockWidth;
            for (var i = ctx.BoxLeft; i <= ctx.BoxRight; i++)
            {
                if (i < 0 || i >= ctx.BlockWidth || j < 0 || j >= ctx.BlockHeight)
                    continue;

                if (ctx.GetPolyAtCell?.Invoke(index + i, selfPolyId) == selfPolyId)
                    ctx.ClearPolyAtCell?.Invoke(index + i, selfPolyId);
            }
        }
    }

    public sealed class LinkPolyobjContext
    {
        public List<(double X, double Y)> SideVertices { get; } = new();
        public int BlockWidth { get; set; }
        public int BlockHeight { get; set; }
        public int BoxLeft { get; set; }
        public int BoxRight { get; set; }
        public int BoxTop { get; set; }
        public int BoxBottom { get; set; }
        public Func<double, int>? BlockX { get; init; }
        public Func<double, int>? BlockY { get; init; }
        public Action<int, int>? AssignPolyToCell { get; init; }
    }

    public static void LinkPolyobj(LinkPolyobjContext ctx, int selfPolyId)
    {
        if (ctx.SideVertices.Count == 0 || ctx.BlockX is null || ctx.BlockY is null)
            return;

        var minX = double.PositiveInfinity;
        var maxX = double.NegativeInfinity;
        var minY = double.PositiveInfinity;
        var maxY = double.NegativeInfinity;
        foreach (var (x, y) in ctx.SideVertices)
        {
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        ctx.BoxLeft = ctx.BlockX(minX);
        ctx.BoxRight = ctx.BlockX(maxX);
        ctx.BoxTop = ctx.BlockY(maxY);
        ctx.BoxBottom = ctx.BlockY(minY);

        for (var j = ctx.BoxBottom * ctx.BlockWidth; j <= ctx.BoxTop * ctx.BlockWidth; j += ctx.BlockWidth)
        {
            for (var i = ctx.BoxLeft; i <= ctx.BoxRight; i++)
            {
                if (i < 0 || i >= ctx.BlockWidth || j < 0 || j >= ctx.BlockHeight * ctx.BlockWidth)
                    continue;
                ctx.AssignPolyToCell?.Invoke(j + i, selfPolyId);
            }
        }
    }

    public sealed class RecalcActorFloorCeilContext
    {
        public double BoundsLeft { get; set; }
        public double BoundsRight { get; set; }
        public double BoundsTop { get; set; }
        public double BoundsBottom { get; set; }
        public Func<IEnumerable<RecalcActor>>? ActorsInBounds { get; init; }
        public Action<RecalcActor>? FindFloorCeiling { get; init; }
    }

    public readonly record struct RecalcActor(double X, double Y, double Radius);

    public static void RecalcActorFloorCeil(RecalcActorFloorCeilContext ctx)
    {
        if (ctx.ActorsInBounds is null)
            return;

        foreach (var actor in ctx.ActorsInBounds())
        {
            if (actor.X + actor.Radius <= ctx.BoundsLeft ||
                actor.X - actor.Radius >= ctx.BoundsRight ||
                actor.Y + actor.Radius <= ctx.BoundsBottom ||
                actor.Y - actor.Radius >= ctx.BoundsTop)
                continue;

            ctx.FindFloorCeiling?.Invoke(actor);
        }
    }
}
