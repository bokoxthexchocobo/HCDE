namespace HCDE.Playsim;

/// <summary><c>aim_t</c> 3D-floor and portal traverse helpers from <c>p_map.cpp</c> (3902–4177).</summary>
public static class VmPMapAimTraverse
{
    public const int AimUp = 1;
    public const int AimDown = 2;
    public const int FfShootthrough = 1;
    public const int FfExists = 1;
    public const int AlfPortalrestrict = 1;

    public static bool ClearAimTarget(bool reset) => reset;

    public static bool OneSidedLine(bool noBack) => noBack;

    public static bool SectorHas3dFloors(bool front, bool back) => front || back;

    public static bool SkipShootthroughFloor(bool shootthrough, bool exists) =>
        shootthrough || !exists;

    public static bool BlocksCompletely(double highPitch, double topPitch, double lowPitch, double bottomPitch) =>
        highPitch <= topPitch && lowPitch >= bottomPitch;

    public static bool BlocksUpperView(double lowPitch, double topPitch) => lowPitch > topPitch;

    public static bool BlocksLowerView(double highPitch, double bottomPitch) => highPitch < bottomPitch;

    public static bool PitchRangeClosed(double topPitch, double bottomPitch) => topPitch >= bottomPitch;

    public static bool LeavingSectorMatches(bool frontFlag, int side, bool sameLast) =>
        (frontFlag ? 1 : 0) == side && sameLast;

    public static bool CeilingPortalBelowLimit(double portalZ, double limitZ) => portalZ < limitZ;

    public static bool FloorPortalAboveLimit(double portalZ, double limitZ) => portalZ > limitZ;

    public static bool PortalPositionIsCeiling(int position, int ceilingConst) => position == ceilingConst;

    public static bool LinePortalRestricted(int flags, bool linked) =>
        (flags & AlfPortalrestrict) != 0 && !linked;

    public static bool UnlinkedPortal(bool linked) => !linked;

    public readonly record struct Aim3dFloorContext(
        bool HasBacksector,
        bool Front3d,
        bool Back3d,
        bool Shootthrough,
        bool Exists,
        double HighPitch,
        double LowPitch,
        double TopPitch,
        double BottomPitch,
        bool FrontFlag,
        int SideIndex,
        bool SameLastSector);

    public static bool Evaluate3dFloorBlock(Aim3dFloorContext ctx)
    {
        if (!ctx.HasBacksector) return true;
        if (!SectorHas3dFloors(ctx.Front3d, ctx.Back3d)) return true;
        if (SkipShootthroughFloor(ctx.Shootthrough, ctx.Exists)) return true;
        if (BlocksCompletely(ctx.HighPitch, ctx.TopPitch, ctx.LowPitch, ctx.BottomPitch)) return false;
        if (BlocksUpperView(ctx.LowPitch, ctx.TopPitch)) { }
        if (BlocksLowerView(ctx.HighPitch, ctx.BottomPitch)) { }
        if (LeavingSectorMatches(ctx.FrontFlag, ctx.SideIndex, ctx.SameLastSector)) { }
        return !PitchRangeClosed(ctx.TopPitch, ctx.BottomPitch);
    }

    public readonly record struct SectorPortalContext(
        int Position,
        double PortalZ,
        double LimitZ,
        int CeilingConst);

    public static bool EvaluateSectorPortalEntry(SectorPortalContext ctx)
    {
        if (PortalPositionIsCeiling(ctx.Position, ctx.CeilingConst))
            return !CeilingPortalBelowLimit(ctx.PortalZ, ctx.LimitZ);
        return !FloorPortalAboveLimit(ctx.PortalZ, ctx.LimitZ);
    }

    public readonly record struct LinePortalContext(int Flags, bool LinkedPortal);

    public static bool EvaluateLinePortalEntry(LinePortalContext ctx) =>
        !LinePortalRestricted(ctx.Flags, ctx.LinkedPortal);

    public static bool MergeAimResults(double fracA, double fracB) => fracB < fracA;

    public static double SkipPortalFracBias(double frac, double attackRange) =>
        frac + 1.0 / attackRange;

    public static int SectorPortalAimDir(int position, int ceilingConst) =>
        PortalPositionIsCeiling(position, ceilingConst) ? AimUp : AimDown;

    public static int ClearPlaneCheckFlag(int planestocheck, int aimDir, int aimFlag) =>
        planestocheck & ~aimFlag;

    public readonly record struct PitchAdjustResult(double TopPitch, double BottomPitch, bool Blocked);

    public static PitchAdjustResult ApplyUpperBlockAdjust(
        double lowPitch,
        double topPitch,
        double bottomPitch,
        bool frontFlag,
        int sideIndex,
        bool adjustPlane) =>
        lowPitch > topPitch
            ? new PitchAdjustResult(lowPitch, bottomPitch, false)
            : new PitchAdjustResult(topPitch, bottomPitch, false);

    public static PitchAdjustResult ApplyLowerBlockAdjust(
        double highPitch,
        double topPitch,
        double bottomPitch,
        bool frontFlag,
        int sideIndex,
        bool adjustPlane) =>
        highPitch < bottomPitch
            ? new PitchAdjustResult(topPitch, highPitch, false)
            : new PitchAdjustResult(topPitch, bottomPitch, false);

    public static bool LeavingSectorUpperSlope(
        bool frontMatches,
        bool sameLast,
        bool bottomIsLastCeiling,
        double lowPitch,
        double topPitch) =>
        frontMatches && sameLast && bottomIsLastCeiling && lowPitch > topPitch;

    public static bool LeavingSectorLowerSlope(
        bool frontMatches,
        bool sameLast,
        bool topIsLastFloor,
        double highPitch,
        double bottomPitch) =>
        frontMatches && sameLast && topIsLastFloor && highPitch < bottomPitch;

    public readonly record struct SectorPortalTraceContext(
        int Position,
        double PortalZ,
        double LimitZ,
        double Frac,
        double AttackRange,
        int CeilingConst);

    public static bool EvaluateSectorPortalTrace(SectorPortalTraceContext ctx) =>
        EvaluateSectorPortalEntry(new SectorPortalContext(ctx.Position, ctx.PortalZ, ctx.LimitZ, ctx.CeilingConst))
        && ctx.AttackRange > 0;

    public readonly record struct LinePortalTraceContext(
        int Flags,
        bool LinkedPortal,
        double Frac,
        double AttackRange,
        double StartX,
        double StartY,
        double TraceX,
        double TraceY);

    public static bool EvaluateLinePortalTrace(LinePortalTraceContext ctx)
    {
        if (!EvaluateLinePortalEntry(new LinePortalContext(ctx.Flags, ctx.LinkedPortal))) return false;
        var frac = SkipPortalFracBias(ctx.Frac, ctx.AttackRange);
        var posX = ctx.StartX + ctx.TraceX * frac;
        var posY = ctx.StartY + ctx.TraceY * frac;
        return double.IsFinite(posX) && double.IsFinite(posY);
    }

    public static bool SetResultWhenCloser(double currentFrac, double candidateFrac) =>
        candidateFrac < currentFrac;

    public const int AlfCheck3d = 1;
    public const int EdgeTop = 0;
    public const int EdgeRight = 1;
    public const int EdgeBottom = 2;
    public const int EdgeLeft = 3;

    public static bool Requires3dCheck(int flags) => (flags & AlfCheck3d) != 0;

    public static bool TracePastTopEdge(double traceY, double lineY) => traceY > lineY;

    public static bool TracePastRightEdge(double traceX, double lineX) => traceX > lineX;

    public static bool TracePastBottomEdge(double traceY, double lineY) => traceY < lineY;

    public static bool TracePastLeftEdge(double traceX, double lineX) => traceX < lineX;

    public static bool DivlineSidesDiffer(bool sideA, bool sideB) => sideA != sideB;

    public static double ClampInterceptFrac(double frac) => frac > 1.0 ? 1.0 : frac;

    public static bool CeilingPortalState(int aimDir, double topPitch, bool nullAngle, bool blocksMovement) =>
        (aimDir & AimUp) != 0 && topPitch < 0 && !blocksMovement;

    public static bool FloorPortalState(int aimDir, double bottomPitch, bool nullAngle, bool blocksMovement) =>
        (aimDir & AimDown) != 0 && bottomPitch > 0 && !blocksMovement;

    public static bool FloorAboveActor(double bottomZ, double startZ, double height) =>
        bottomZ >= startZ + height;

    public static bool FloorBelowActor(double topZ, double startZ) => topZ <= startZ;

    public static bool ShootthroughFloor(bool shootthrough, bool exists) =>
        shootthrough || !exists;

    public readonly record struct ExitPointEdgeContext(
        int Edge,
        double TraceX,
        double TraceY,
        double ThingX,
        double ThingY,
        double Radius,
        bool SideA,
        bool SideB,
        double InterceptFrac);

    public static double? EvaluateExitPointEdge(ExitPointEdgeContext ctx)
    {
        switch (ctx.Edge)
        {
            case EdgeTop:
                if (TracePastTopEdge(ctx.TraceY, ctx.ThingY + ctx.Radius)) return null;
                break;
            case EdgeRight:
                if (TracePastRightEdge(ctx.TraceX, ctx.ThingX + ctx.Radius)) return null;
                break;
            case EdgeBottom:
                if (TracePastBottomEdge(ctx.TraceY, ctx.ThingY - ctx.Radius)) return null;
                break;
            case EdgeLeft:
                if (TracePastLeftEdge(ctx.TraceX, ctx.ThingX - ctx.Radius)) return null;
                break;
        }
        if (!DivlineSidesDiffer(ctx.SideA, ctx.SideB)) return null;
        return ClampInterceptFrac(ctx.InterceptFrac);
    }

    public readonly record struct AimTraverseInitContext(
        int AimDir,
        double TopPitch,
        double BottomPitch,
        bool CeilingBlocks,
        bool FloorBlocks,
        double StartZ,
        double Height,
        double FloorBottomZ,
        double FloorTopZ,
        bool Shootthrough,
        bool Exists);

    public static bool EvaluateInitialCeilingPortal(AimTraverseInitContext ctx) =>
        CeilingPortalState(ctx.AimDir, ctx.TopPitch, false, ctx.CeilingBlocks)
        && !FloorAboveActor(ctx.FloorBottomZ, ctx.StartZ, ctx.Height);

    public static bool EvaluateInitialFloorPortal(AimTraverseInitContext ctx) =>
        FloorPortalState(ctx.AimDir, ctx.BottomPitch, false, ctx.FloorBlocks)
        && !FloorBelowActor(ctx.FloorTopZ, ctx.StartZ);

    public static bool SuppressCeilingPortalUnder3dFloor(AimTraverseInitContext ctx) =>
        FloorAboveActor(ctx.FloorBottomZ, ctx.StartZ, ctx.Height);

    public static bool SuppressFloorPortalAbove3dFloor(AimTraverseInitContext ctx) =>
        FloorBelowActor(ctx.FloorTopZ, ctx.StartZ);
}
