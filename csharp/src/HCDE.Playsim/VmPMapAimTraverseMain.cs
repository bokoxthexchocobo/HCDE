namespace HCDE.Playsim;

/// <summary><c>aim_t::AimTraverse</c> intercept loop from <c>p_map.cpp</c> (4294–4565).</summary>
public static class VmPMapAimTraverseMain
{
    public const int MlTwosided = 1;
    public const int MlBlockeverything = 1 << 1;
    public const int AlfCheckconversation = 1;
    public const int AlfChecknonshootable = 1 << 1;
    public const int AlfIgnorenoautoaim = 1 << 2;
    public const int AlfCheck3d = 1 << 3;
    public const int AlfNofriends = 1 << 4;
    public const int AlfForcenosmart = 1 << 5;
    public const int MfShootable = 1;
    public const int Mf3Ghost = 1;
    public const int Mf2Thrughost = 1;
    public const int Mf6Notautoaimed = 1;
    public const int Mf3Ismonster = 1;
    public const int AimUp = 1;
    public const int AimDown = 2;
    public const double LineopenMin = -32768;
    public const double LineopenMax = 32767;

    public const int ResultLine = 0;
    public const int ResultFriend = 1;
    public const int ResultOther = 2;

    public static bool StopWhenBetterFound(double interceptFrac, double? lineTargetFrac) =>
        lineTargetFrac is double existing && interceptFrac > existing;

    public static bool LinePortalEntry(bool isPortal, int frontFlag) =>
        isPortal && frontFlag == 0;

    public static bool LineBlocksAim(int flags) =>
        (flags & MlTwosided) == 0 || (flags & MlBlockeverything) != 0;

    public static bool OpeningBlocks(double range, double bottom, double top) =>
        range <= 0 || bottom >= top;

    public static bool PitchRangeClosed(double topPitch, double bottomPitch) =>
        topPitch >= bottomPitch;

    public static bool AdjustBottomOpening(double bottom, double shootZ, double dist, ref double bottomPitch)
    {
        if (bottom == LineopenMin) return false;
        var pitch = -VecToAngle(dist, bottom - shootZ);
        if (pitch < bottomPitch)
        {
            bottomPitch = pitch;
            return true;
        }
        return false;
    }

    public static bool AdjustTopOpening(double top, double shootZ, double dist, ref double topPitch)
    {
        if (top == LineopenMax) return false;
        var pitch = -VecToAngle(dist, top - shootZ);
        if (pitch > topPitch)
        {
            topPitch = pitch;
            return true;
        }
        return false;
    }

    public static double VecToAngle(double dist, double dz) =>
        dist == 0 ? (dz >= 0 ? double.PositiveInfinity : double.NegativeInfinity) : Math.Atan2(dz, dist);

    public static bool SectorPortalOnEnter(
        int planestocheck,
        double topPitch,
        double bottomPitch,
        double openTop,
        double openBottom,
        bool blocksCeiling,
        bool blocksFloor) =>
        (planestocheck & AimUp) != 0 && topPitch < 0 && openTop != LineopenMax && !blocksCeiling
        || (planestocheck & AimDown) != 0 && bottomPitch > 0 && openBottom != LineopenMin && !blocksFloor;

    public static bool SkipSelf(AActorRef thing, AActorRef shootthing) => thing == shootthing;

    public static bool AimTargetMismatch(AActorRef? aimTarget, AActorRef thing) =>
        aimTarget is not null && thing != aimTarget;

    public static bool ShouldPassThroughPlayer(bool shooterIsPlayer, bool pass) =>
        shooterIsPlayer && pass;

    public readonly record struct ThingFilterContext(
        int Flags,
        bool HasConversation,
        bool Shootable,
        bool Ghost,
        bool ShooterIsPlayer,
        bool WeaponThruGhost,
        bool ClDoAutoaim,
        bool NotAutoaimed);

    public static bool SkipThingForAim(ThingFilterContext ctx)
    {
        if ((ctx.Flags & AlfCheckconversation) == 0 || !ctx.HasConversation)
        {
            if ((ctx.Flags & AlfChecknonshootable) != 0) return false;
            if (!ctx.Shootable) return true;
            if (ctx.Ghost && ctx.ShooterIsPlayer && ctx.WeaponThruGhost) return true;
        }
        if (!ctx.ClDoAutoaim && (ctx.Flags & AlfIgnorenoautoaim) == 0 && ctx.NotAutoaimed) return true;
        return false;
    }

    public static bool ThingSectorHas3dFloors(bool hasFloors) => hasFloors;

    public static bool RaiseTopPitchFromCeilingPlane(double pitch, ref double topPitch)
    {
        if (pitch > topPitch)
        {
            topPitch = pitch;
            return true;
        }
        return false;
    }

    public static bool LowerBottomPitchFromFloorPlane(double pitch, ref double bottomPitch)
    {
        if (pitch < bottomPitch)
        {
            bottomPitch = pitch;
            return true;
        }
        return false;
    }

    public static bool ShotOverTop(double shootZ, double thingTop, double thingTopPitch, double bottomPitch) =>
        shootZ > thingTop && thingTopPitch > bottomPitch;

    public static bool ShotUnderBottom(double shootZ, double thingZ, double thingBottomPitch, double topPitch) =>
        shootZ < thingZ && thingBottomPitch < topPitch;

    public static bool ExitPointClearsTopShot(double exitFrac, double exitDist, double thingTop, double shootZ, double bottomPitch)
    {
        if (exitFrac <= 0) return false;
        var pitch = -VecToAngle(exitDist, thingTop - shootZ);
        return pitch <= bottomPitch;
    }

    public static bool ExitPointClearsBottomShot(double exitFrac, double exitDist, double thingZ, double shootZ, double topPitch)
    {
        if (exitFrac <= 0) return false;
        var pitch = -VecToAngle(exitDist, thingZ - shootZ);
        return pitch >= topPitch;
    }

    public readonly record struct FfSightNarrowContext(
        bool CrossedFfloors,
        bool Unlinked,
        bool CanSee,
        double ThingTopPitch,
        double ThingBottomPitch,
        double TopPitch,
        double BottomPitch);

    public static FfSightNarrowResult EvaluateFfSightNarrow(FfSightNarrowContext ctx)
    {
        if (!ctx.CrossedFfloors || ctx.Unlinked || ctx.CanSee)
            return FfSightNarrowResult.Continue;

        var top = ctx.TopPitch;
        var bottom = ctx.BottomPitch;
        if (ctx.ThingTopPitch < top)
        {
            if (ctx.ThingBottomPitch > top) top = ctx.ThingBottomPitch;
        }
        else if (ctx.ThingBottomPitch > bottom)
        {
            if (ctx.ThingTopPitch < bottom) bottom = ctx.ThingTopPitch;
        }
        if (top < bottom) return FfSightNarrowResult.ContinueSkipThing;
        return FfSightNarrowResult.StopTraverse;
    }

    public readonly record struct FfSightNarrowResult(int Kind, double TopPitch, double BottomPitch)
    {
        public static FfSightNarrowResult Continue => new(0, 0, 0);
        public static FfSightNarrowResult ContinueSkipThing => new(1, 0, 0);
        public static FfSightNarrowResult StopTraverse => new(2, 0, 0);

        public bool ShouldContinueThing => Kind == 1;
        public bool ShouldStop => Kind == 2;
    }

    public static double ClampThingTopPitch(double thingTopPitch, double topPitch) =>
        thingTopPitch < topPitch ? topPitch : thingTopPitch;

    public static double ClampThingBottomPitch(double thingBottomPitch, double bottomPitch) =>
        thingBottomPitch > bottomPitch ? bottomPitch : thingBottomPitch;

    public static double MidThingPitch(double thingTopPitch, double thingBottomPitch) =>
        thingTopPitch / 2 + thingBottomPitch / 2;

    public static bool Check3dRangeExceeded(
        int flags,
        double thingPitchCos,
        double traceLen,
        double frac,
        double attackRange)
    {
        if ((flags & AlfCheck3d) == 0) return false;
        if (thingPitchCos == 0) return false;
        var d3 = traceLen * frac / thingPitchCos;
        return d3 > attackRange;
    }

    public readonly record struct SmartAimContext(
        int SvSmartaim,
        int Flags,
        AActorRef? AimTarget,
        bool IsFriend,
        bool IsMonster,
        bool HasPlayer);

    public static SmartAimDecision EvaluateSmartAim(SmartAimContext ctx)
    {
        if ((ctx.Flags & AlfNofriends) != 0 && ctx.IsFriend && ctx.AimTarget is null)
            return SmartAimDecision.Skip;
        if (ctx.SvSmartaim == 0 || (ctx.Flags & AlfForcenosmart) != 0 || ctx.AimTarget is not null)
            return SmartAimDecision.HitLineTarget;
        if (ctx.IsFriend)
        {
            if (ctx.SvSmartaim < 2) return SmartAimDecision.RecordFriend;
            return SmartAimDecision.Continue;
        }
        if (!ctx.IsMonster && !ctx.HasPlayer)
        {
            if (ctx.SvSmartaim < 3) return SmartAimDecision.RecordOther;
            return SmartAimDecision.Continue;
        }
        return SmartAimDecision.HitLineTarget;
    }

    public enum SmartAimDecision
    {
        Skip,
        Continue,
        RecordFriend,
        RecordOther,
        HitLineTarget,
    }

    public readonly record struct LineInterceptDecision(
        bool StopTraverse,
        bool EnterLinePortal,
        bool ContinueLoop,
        double TopPitch,
        double BottomPitch);

    public static LineInterceptDecision EvaluateLineIntercept(
        bool isLine,
        bool isPortal,
        int frontFlag,
        int lineFlags,
        double openRange,
        double openBottom,
        double openTop,
        double shootZ,
        double frac,
        double attackRange,
        double topPitch,
        double bottomPitch,
        bool threeDFloorOk,
        int planestocheck)
    {
        if (!isLine) return new LineInterceptDecision(false, false, true, topPitch, bottomPitch);
        if (LinePortalEntry(isPortal, frontFlag))
            return new LineInterceptDecision(true, true, false, topPitch, bottomPitch);
        if (LineBlocksAim(lineFlags))
            return new LineInterceptDecision(true, false, false, topPitch, bottomPitch);
        if (OpeningBlocks(openRange, openBottom, openTop))
            return new LineInterceptDecision(true, false, false, topPitch, bottomPitch);

        var dist = attackRange * frac;
        var bottom = bottomPitch;
        var top = topPitch;
        AdjustBottomOpening(openBottom, shootZ, dist, ref bottom);
        AdjustTopOpening(openTop, shootZ, dist, ref top);
        if (PitchRangeClosed(top, bottom))
            return new LineInterceptDecision(true, false, false, top, bottom);
        if (!threeDFloorOk)
            return new LineInterceptDecision(true, false, false, top, bottom);
        return new LineInterceptDecision(false, false, true, top, bottom);
    }

    public readonly record struct AActorRef(int Id);

    public static int MapSmartAimToResult(SmartAimDecision decision) =>
        decision switch
        {
            SmartAimDecision.RecordFriend => ResultFriend,
            SmartAimDecision.RecordOther => ResultOther,
            _ => ResultLine,
        };
}
