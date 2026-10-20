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
        frontFlag == side && sameLast;

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
}
