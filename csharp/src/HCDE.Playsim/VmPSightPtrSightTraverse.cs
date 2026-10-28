namespace HCDE.Playsim;

/// <summary><c>SightCheck::PTR_SightTraverse</c> from <c>p_sight.cpp</c> (210–384).</summary>
public static class VmPSightPtrSightTraverse
{
    public const double LineOpenMin = -32768;
    public const double LineOpenMax = 32768;

    public const int SoTopFront = 1;
    public const int SoTopBack = 2;
    public const int SoBottomFront = 4;
    public const int SoBottomBack = 8;

    public const int PortalDirFloor = 0;
    public const int PortalDirCeiling = 1;

    public const int SfIgnoreWaterBoundary = 1;
    public const int FfExists = 1;
    public const int FfSeethrough = 1 << 1;
    public const int FfSolid = 1 << 2;

    public const int PorttLinked = 1;

    public readonly record struct SightFFloorAtHit(
        bool Exists,
        int Flags,
        double BottomZ,
        double TopZ);

    public sealed class SightTraverseContext
    {
        public double TraceX { get; set; }
        public double TraceDx { get; set; }
        public double TraceY { get; set; }
        public double TraceDy { get; set; }
        public double SightStartZ { get; set; }
        public double TopSlope { get; set; } = LineOpenMax;
        public double BottomSlope { get; set; } = LineOpenMin;
        public double LastZTop { get; set; }
        public double LastZBottom { get; set; }
        public int PortalDir { get; set; }
        public int Flags { get; set; }
        public int MySeethroughMask { get; set; } = FfSeethrough;
        public bool CompatTrace { get; set; }
        public bool SameSectorLine { get; set; }
        public bool LineBlocksSight { get; set; }
        public double InterceptFrac { get; set; }
        public double OpenBottom { get; set; }
        public double OpenTop { get; set; }
        public double OpenRange { get; set; }
        public int OpenPortalFlags { get; set; }
        public bool HasLinkedPortal { get; set; }
        public int LinkedPortalType { get; set; }
        public int LinkedPortalGroup { get; set; }
        public bool HasBacksector { get; set; }
        public int FrontFlag { get; set; } = -1;
        public int? LastSectorSide { get; set; }
        public IReadOnlyList<SightFFloorAtHit> FrontFFloors { get; set; } = Array.Empty<SightFFloorAtHit>();
        public IReadOnlyList<SightFFloorAtHit> BackFFloors { get; set; } = Array.Empty<SightFFloorAtHit>();
        public List<(double Frac, double Top, double Bottom, int Plane, int Group)> PortalQueue { get; } = new();

        public Func<double, double, double, (double Bottom, double Top, double Range, int PortalFlags)>? SightOpening { get; init; }
        public Func<double, double, int>? PointOnLineSidePrecise { get; init; }
        public Func<int, int, int>? OppositePortalGroup { get; init; }
    }

    public static int SwapPortalFlagsSides(int portalFlags) =>
        ((portalFlags & (SoTopFront | SoBottomFront)) << 1) | ((portalFlags & (SoTopBack | SoBottomBack)) >> 1);

    public static bool PtrSightTraverse(SightTraverseContext ctx)
    {
        if (ctx.CompatTrace && ctx.SameSectorLine)
            return true;

        var (trX, trY) = TracePointAtFrac(ctx);

        if (ctx.SightOpening is not null)
        {
            var open = ctx.SightOpening(trX, trY, ctx.InterceptFrac);
            ctx.OpenBottom = open.Bottom;
            ctx.OpenTop = open.Top;
            ctx.OpenRange = open.Range;
            ctx.OpenPortalFlags = open.PortalFlags;
        }

        ApplyLineBlocksSight(ctx);

        if (IsTotallyClosedDoor(ctx))
            return false;

        if (ctx.InterceptFrac == 0)
            return true;

        UpdateSlopesFromOpening(ctx);

        if (!EnqueueSectorPortals(ctx, trX, trY))
            return false;

        if (TryLinkedPortalStop(ctx))
            return false;

        if (SlopesFullyBlocked(ctx))
            return false;

        if (ctx.FrontFFloors.Count > 0 || ctx.BackFFloors.Count > 0)
        {
            if (!EvaluateSight3dFloors(ctx, trX, trY))
                return false;
        }
        else
            ctx.LastSectorSide = null;

        FinalizeLastZ(ctx);
        return true;
    }

    private static (double X, double Y) TracePointAtFrac(SightTraverseContext ctx) =>
        (ctx.TraceX + ctx.TraceDx * ctx.InterceptFrac, ctx.TraceY + ctx.TraceDy * ctx.InterceptFrac);

    private static void ApplyLineBlocksSight(SightTraverseContext ctx)
    {
        if (!ctx.LineBlocksSight)
            return;
        ctx.OpenRange = 0;
        if (ctx.OpenBottom != LineOpenMin)
            ctx.OpenBottom = ctx.OpenTop;
    }

    private static bool IsTotallyClosedDoor(SightTraverseContext ctx) =>
        ctx.OpenRange == 0 && ctx.OpenPortalFlags == 0 &&
        (!ctx.HasLinkedPortal || ctx.LinkedPortalType != PorttLinked);

    private static void UpdateSlopesFromOpening(SightTraverseContext ctx)
    {
        if (ctx.OpenBottom > LineOpenMin)
        {
            var slope = (ctx.OpenBottom - ctx.SightStartZ) / ctx.InterceptFrac;
            if (slope > ctx.BottomSlope)
                ctx.BottomSlope = slope;
        }

        if (ctx.OpenTop < LineOpenMax)
        {
            var slope = (ctx.OpenTop - ctx.SightStartZ) / ctx.InterceptFrac;
            if (slope < ctx.TopSlope)
                ctx.TopSlope = slope;
        }
    }

    private static bool EnqueueSectorPortals(SightTraverseContext ctx, double trX, double trY)
    {
        if (ctx.OpenPortalFlags == 0)
            return true;

        ctx.FrontFlag = ctx.PointOnLineSidePrecise?.Invoke(trX, trY) ?? 0;
        var portalFlags = ctx.OpenPortalFlags;
        if (ctx.FrontFlag != 0 && !ctx.HasBacksector)
            return false;

        if (ctx.FrontFlag != 0)
            portalFlags = SwapPortalFlagsSides(portalFlags);

        var backGroupCeiling = ctx.OppositePortalGroup?.Invoke(PortalDirCeiling, ctx.FrontFlag) ?? 0;
        var backGroupFloor = ctx.OppositePortalGroup?.Invoke(PortalDirFloor, ctx.FrontFlag) ?? 0;

        if (ctx.PortalDir != PortalDirFloor && (portalFlags & SoTopBack) != 0 && (portalFlags & SoTopFront) == 0)
            ctx.PortalQueue.Add((ctx.InterceptFrac, ctx.TopSlope, ctx.BottomSlope, PortalDirCeiling, backGroupCeiling));
        if (ctx.PortalDir != PortalDirCeiling && (portalFlags & SoBottomBack) != 0 && (portalFlags & SoBottomFront) == 0)
            ctx.PortalQueue.Add((ctx.InterceptFrac, ctx.TopSlope, ctx.BottomSlope, PortalDirFloor, backGroupFloor));

        return true;
    }

    private static bool TryLinkedPortalStop(SightTraverseContext ctx)
    {
        if (!ctx.HasLinkedPortal)
            return false;
        ctx.PortalQueue.Add((ctx.InterceptFrac, ctx.TopSlope, ctx.BottomSlope, ctx.PortalDir, ctx.LinkedPortalGroup));
        return true;
    }

    private static bool SlopesFullyBlocked(SightTraverseContext ctx) =>
        ctx.TopSlope <= ctx.BottomSlope || ctx.OpenRange == 0;

    private static bool EvaluateSight3dFloors(SightTraverseContext ctx, double trX, double trY)
    {
        if (ctx.FrontFlag == -1)
            ctx.FrontFlag = ctx.PointOnLineSidePrecise?.Invoke(trX, trY) ?? 0;

        var topz = ctx.TopSlope * ctx.InterceptFrac + ctx.SightStartZ;
        var bottomz = ctx.BottomSlope * ctx.InterceptFrac + ctx.SightStartZ;

        for (var side = 1; side <= 2; side++)
        {
            var ffloors = side == 1 ? ctx.FrontFFloors : ctx.BackFFloors;
            var otherFfloors = side == 1 ? ctx.BackFFloors : ctx.FrontFFloors;

            foreach (var rover in ffloors)
            {
                if (!ShouldConsiderFFloor(ctx, rover))
                    continue;

                var highslope = (rover.TopZ - ctx.SightStartZ) / ctx.InterceptFrac;
                var lowslope = (rover.BottomZ - ctx.SightStartZ) / ctx.InterceptFrac;

                if (highslope >= ctx.TopSlope)
                {
                    if (lowslope <= ctx.BottomSlope)
                        return false;
                    if (lowslope < ctx.TopSlope)
                        ctx.TopSlope = lowslope;
                }
                else if (lowslope <= ctx.BottomSlope)
                {
                    if (highslope > ctx.BottomSlope)
                        ctx.BottomSlope = highslope;
                }
                else if (!OverlappingPartnerBlocks(ctx, rover, otherFfloors))
                {
                    // partner scan found no overlap block
                }
                else
                    return false;

                if (ctx.LastSectorSide == side - 1 && ctx.FrontFlag == side - 1)
                {
                    if (ctx.LastZTop <= rover.BottomZ && topz > rover.TopZ)
                        ctx.TopSlope = lowslope;
                    if (ctx.LastZBottom >= rover.TopZ && bottomz < rover.TopZ)
                        ctx.BottomSlope = highslope;
                }

                if (ctx.TopSlope <= ctx.BottomSlope)
                    return false;
            }
        }

        ctx.LastSectorSide = ctx.FrontFlag == 0 ? 1 : 0;
        return true;
    }

    private static bool ShouldConsiderFFloor(SightTraverseContext ctx, SightFFloorAtHit rover)
    {
        if ((rover.Flags & FfSeethrough) == ctx.MySeethroughMask || (rover.Flags & FfExists) == 0)
            return false;
        if ((ctx.Flags & SfIgnoreWaterBoundary) != 0 && (rover.Flags & FfSolid) == 0)
            return false;
        return true;
    }

    private static bool OverlappingPartnerBlocks(
        SightTraverseContext ctx,
        SightFFloorAtHit rover,
        IReadOnlyList<SightFFloorAtHit> partnerFfloors)
    {
        foreach (var rover2 in partnerFfloors)
        {
            if (!ShouldConsiderFFloor(ctx, rover2))
                continue;

            var ffbBottom = rover2.BottomZ;
            var ffbTop = rover2.TopZ;
            var ffBottom = rover.BottomZ;
            var ffTop = rover.TopZ;

            if ((ffbBottom >= ffBottom && ffbBottom <= ffTop) ||
                (ffbTop <= ffTop && ffbTop >= ffBottom) ||
                (ffbTop >= ffTop && ffbBottom <= ffBottom) ||
                (ffbTop <= ffTop && ffbBottom >= ffBottom))
                return true;
        }

        return false;
    }

    private static void FinalizeLastZ(SightTraverseContext ctx)
    {
        ctx.LastZTop = ctx.TopSlope * ctx.InterceptFrac + ctx.SightStartZ;
        ctx.LastZBottom = ctx.BottomSlope * ctx.InterceptFrac + ctx.SightStartZ;
    }
}
