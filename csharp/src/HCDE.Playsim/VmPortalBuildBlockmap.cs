namespace HCDE.Playsim;

/// <summary><c>FLevelLocals::BuildPortalBlockmap</c>, <c>FLinePortalTraverse::AddLineIntercepts</c>, <c>FindPortalDestination</c>, and <c>SetPortalRotation</c> from <c>portal.cpp</c> (60–232).</summary>
public static class VmPortalBuildBlockmap
{
    public const int PorttVisual = 0;
    public const int PorttLinked = 2;
    public const int PortfPolyobj = 1;

    public sealed class PortalBlockState
    {
        public bool ContainsLines { get; set; }
        public bool HasLinkedSectorPortals { get; set; }
    }

    public sealed class BuildPortalBlockmapContext
    {
        public int BmapWidth { get; set; }
        public int BmapHeight { get; set; }
        public PortalBlockState Global { get; } = new();
        public Func<int, int, IReadOnlyList<int>>? GetLineIndicesInBlock { get; init; }
        public Func<int, PortalLineInfo>? GetLineInfo { get; init; }
        public Action<int, int, PortalBlockCell>? InitCell { get; init; }
        public Action<int, int, Action<PortalBlockCell>>? MutateCell { get; init; }
    }

    public sealed class PortalBlockCell
    {
        public List<int> PortalLineIndices { get; } = new();
        public bool NeighborContainsLines { get; set; }
        public bool ContainsLinkedPortals { get; set; }
    }

    public readonly record struct PortalLineInfo(
        int Index,
        bool HasPortal,
        int PortalType,
        bool FrontLinkedFloorOrCeiling,
        bool BackLinkedFloorOrCeiling);

    public static void BuildPortalBlockmap(BuildPortalBlockmapContext ctx)
    {
        ctx.Global.ContainsLines = false;
        ctx.Global.HasLinkedSectorPortals = false;
        for (var y = 0; y < ctx.BmapHeight; y++)
        {
            for (var x = 0; x < ctx.BmapWidth; x++)
            {
                var cell = new PortalBlockCell();
                ctx.InitCell?.Invoke(x, y, cell);
                var list = ctx.GetLineIndicesInBlock?.Invoke(x, y) ?? Array.Empty<int>();
                foreach (var lineIdx in list)
                {
                    if (lineIdx < 0)
                        break;
                    var ld = ctx.GetLineInfo?.Invoke(lineIdx);
                    if (ld is null)
                        continue;
                    if (ld.Value.HasPortal && ld.Value.PortalType != PorttVisual)
                    {
                        ctx.Global.ContainsLines = true;
                        cell.PortalLineIndices.Add(lineIdx);
                        cell.NeighborContainsLines = true;
                        if (ld.Value.PortalType == PorttLinked)
                            cell.ContainsLinkedPortals = true;
                        if (x > 0)
                            MarkNeighbor(ctx, x - 1, y);
                        if (y > 0)
                            MarkNeighbor(ctx, x, y - 1);
                        if (x < ctx.BmapWidth - 1)
                            MarkNeighbor(ctx, x + 1, y);
                        if (y < ctx.BmapHeight - 1)
                            MarkNeighbor(ctx, x, y + 1);
                    }
                    else
                    {
                        var yes = ld.Value.FrontLinkedFloorOrCeiling;
                        if (ld.Value.BackLinkedFloorOrCeiling)
                            yes = true;
                        cell.ContainsLinkedPortals |= yes;
                        ctx.Global.HasLinkedSectorPortals |= yes;
                    }
                }
            }
        }
    }

    private static void MarkNeighbor(BuildPortalBlockmapContext ctx, int x, int y)
    {
        if (x < 0 || y < 0 || x >= ctx.BmapWidth || y >= ctx.BmapHeight)
            return;
        ctx.MutateCell?.Invoke(x, y, c => c.NeighborContainsLines = true);
    }

    public sealed class AddLineInterceptsContext
    {
        public int Bx { get; set; }
        public int By { get; set; }
        public int BlockmapDx { get; set; }
        public int BlockmapDy { get; set; }
        public int ValidCount { get; set; }
        public double TraceX { get; set; }
        public double TraceY { get; set; }
        public double TraceDx { get; set; }
        public double TraceDy { get; set; }
        public IReadOnlyList<int> PortalLinesInBlock { get; set; } = Array.Empty<int>();
        public Func<int, int>? LineValidCount { get; init; }
        public Action<int, int>? SetLineValidCount { get; init; }
        public Func<int, (double V1X, double V1Y, double V2X, double V2Y)>? LineVerts { get; init; }
        public Func<double, double, double, double, double, double, int>? PointOnDivlineSide6 { get; init; }
        public Func<double, double, double, double, int>? PointOnDivlineSide4 { get; init; }
        public Func<int, (double Dx, double Dy)>? LineDelta { get; init; }
        public Func<double, double, double, double, double, double, double>? InterceptVector { get; init; }
        public List<PortalIntercept> Intercepts { get; } = new();
    }

    public sealed class PortalIntercept
    {
        public double Frac { get; set; }
        public int LineIndex { get; set; }
    }

    public static void AddLineIntercepts(AddLineInterceptsContext ctx)
    {
        if (ctx.By < 0 || ctx.By >= ctx.BlockmapDy || ctx.Bx < 0 || ctx.Bx >= ctx.BlockmapDx)
            return;

        foreach (var lineIdx in ctx.PortalLinesInBlock)
        {
            if (ctx.LineValidCount?.Invoke(lineIdx) == ctx.ValidCount)
                continue;

            var (v1x, v1y, v2x, v2y) = ctx.LineVerts?.Invoke(lineIdx) ?? (0, 0, 0, 0);
            var side6 = ctx.PointOnDivlineSide6;
            var side4 = ctx.PointOnDivlineSide4;
            if (side6 is null)
                continue;
            if (side6(v1x, v1y, ctx.TraceX, ctx.TraceY, ctx.TraceDx, ctx.TraceDy) ==
                side6(v2x, v2y, ctx.TraceX, ctx.TraceY, ctx.TraceDx, ctx.TraceDy))
                continue;

            var dl = ctx.LineDelta?.Invoke(lineIdx) ?? (0, 0);
            if (side4 is null ||
                side4(ctx.TraceX, ctx.TraceY, v1x, v1y) != 0 ||
                side4(ctx.TraceX + ctx.TraceDx, ctx.TraceY + ctx.TraceDy, v1x, v1y) != 1)
                continue;

            var frac = ctx.InterceptVector?.Invoke(ctx.TraceX, ctx.TraceY, ctx.TraceDx, ctx.TraceDy, dl.Dx, dl.Dy) ?? -1;
            if (frac < 0 || frac > 1)
                continue;

            ctx.Intercepts.Add(new PortalIntercept { Frac = frac, LineIndex = lineIdx });
        }
    }

    public const int TagHashSize = 256;

    public sealed class FindPortalDestinationContext
    {
        public int SourceLineIndex { get; set; }
        public int Tag { get; set; }
        public int MatchType { get; set; } = -1;
        public required VmPTagManager.LineIdIterator Iterator { get; init; }
        public Func<int, int>? LineSpecial { get; init; }
    }

    public static int FindPortalDestinationLineIndex(FindPortalDestinationContext ctx)
    {
        if (ctx.Tag == 0)
            return -1;
        int lineno;
        while ((lineno = ctx.Iterator.Next()) >= 0)
        {
            if (lineno != ctx.SourceLineIndex &&
                (ctx.MatchType == -1 || ctx.MatchType == ctx.LineSpecial?.Invoke(lineno)))
                return lineno;
        }
        return -1;
    }

    public sealed class PortalRotationState
    {
        public double SinRot { get; set; }
        public double CosRot { get; set; } = 1;
        public double AngleDiffDegrees { get; set; }
        public int Flags { get; set; }
    }

    public static void SetPortalRotation(
        int portalType,
        bool hasDestination,
        double originDeltaAngleDeg,
        double destDeltaAngleDeg,
        bool originPolyobj,
        bool destPolyobj,
        PortalRotationState state)
    {
        if (!hasDestination)
            return;
        if (portalType != PorttLinked)
        {
            var angleDeg = destDeltaAngleDeg - originDeltaAngleDeg + 180;
            var rad = angleDeg * Math.PI / 180;
            state.SinRot = Math.Sin(rad);
            state.CosRot = Math.Cos(rad);
            state.AngleDiffDegrees = angleDeg;
            if (originPolyobj || destPolyobj)
                state.Flags |= PortfPolyobj;
            else
                state.Flags &= ~PortfPolyobj;
        }
        else
        {
            state.SinRot = 0;
            state.CosRot = 1;
            state.AngleDiffDegrees = 0;
        }
    }
}
