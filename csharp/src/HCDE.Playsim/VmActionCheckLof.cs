namespace HCDE.Playsim;

/// <summary><c>AActor::CheckLOF</c> and <c>CheckLOFTraceFunc</c> from <c>p_actionfunctions.cpp</c> (2286–2517).</summary>
public static class VmActionCheckLof
{
    public const int CloffNoaimVert = 0x00000001;
    public const int CloffNoaimHorz = 0x00000002;
    public const int CloffJumpenemy = 0x00000004;
    public const int CloffJumpfriend = 0x00000008;
    public const int CloffJumpobject = 0x00000010;
    public const int CloffJumpnonhostile = 0x00000020;
    public const int CloffSkipenemy = 0x00000040;
    public const int CloffSkipfriend = 0x00000080;
    public const int CloffSkipobject = 0x00000100;
    public const int CloffSkipnonhostile = 0x00000200;
    public const int CloffMustbeshootable = 0x00000400;
    public const int CloffSkiptarget = 0x00000800;
    public const int CloffAllownull = 0x00001000;
    public const int CloffCheckpartial = 0x00002000;
    public const int CloffMustbeghost = 0x00004000;
    public const int CloffIgnoreghost = 0x00008000;
    public const int CloffMustbesolid = 0x00010000;
    public const int CloffBeyondtarget = 0x00020000;
    public const int CloffFrombase = 0x00040000;
    public const int CloffMulHeight = 0x00080000;
    public const int CloffMulWidth = 0x00100000;
    public const int CloffJumpOnMiss = 0x00200000;
    public const int CloffAimVertNooffset = 0x00400000;
    public const int CloffSettarget = 0x00800000;
    public const int CloffSetmaster = 0x01000000;
    public const int CloffSettracer = 0x02000000;

    public const int MfShootable = 0x00000004;
    public const int MfSolid = 0x00000002;
    public const int Mf2Nonshootable = 0x04000000;
    public const int Mf3Ghost = 0x00000008;
    public const int Mf3Ismonster = 0x00002000;

    public const int TraceHitActor = 2;
    public const int TraceHitNone = 0;

    public const int PlayerMissileRange = 32 * 64;
    public const int MissileRange = 32 * 64;

    public enum LofTraceStatus { Stop, Skip, Abort }

    public static bool TargetNullWithoutAllow(int flags, bool hasTarget) =>
        !hasTarget && (flags & CloffAllownull) == 0;

    public static double ScaleHeightOffset(bool mulHeight, bool hasPlayer, double offset, double height, double crouch) =>
        mulHeight ? (hasPlayer ? offset * height * crouch : offset * height) : offset;

    public static double ScaleWidthOffset(bool mulWidth, double offset, double radius) =>
        mulWidth ? offset * radius : offset;

    public static bool RangeExceeded(double distance, double range, bool checkPartial) =>
        range > 0 && !checkPartial && distance > range;

    public static double DefaultRange(bool hasPlayer, int playerRange, int missileRange) =>
        hasPlayer ? playerRange : missileRange;

    public static bool JumpOnMissSuccess(int flags, bool badActor, int hitType) =>
        (flags & CloffJumpOnMiss) != 0 && !badActor && hitType != TraceHitNone;

    public static bool MinRangeFail(double distance, double minRange) =>
        minRange > 0 && distance < minRange;

    public static bool ShouldSetTarget(int flags) => (flags & CloffSettarget) != 0;

    public static bool ShouldSetMaster(int flags) => (flags & CloffSetmaster) != 0;

    public static bool ShouldSetTracer(int flags) => (flags & CloffSettracer) != 0;

    public static bool SuccessPath(bool hitActor, bool jumpOnMiss) => hitActor || jumpOnMiss;

    public static (double X, double Y) Vec2Offset(double forward, double side, double sin, double cos) =>
        (forward * cos + side * sin, forward * sin - side * cos);

    public static double VecToAngle(double dist, double dz) =>
        dist == 0 ? (dz >= 0 ? double.PositiveInfinity : double.NegativeInfinity) : Math.Atan2(dz, dist);

    public static double ResolveHorizontalAngleRadians(bool noAimHorz, double selfYaw, double angleToTargetRadians) =>
        noAimHorz ? selfYaw : angleToTargetRadians;

    public static double AdjustPitchRadians(
        bool noAimVert,
        bool aimVertNoOffset,
        double pitchRadians,
        double selfPitchRadians,
        double xyDist,
        double targetCenterZ,
        double posZ,
        double offsetHeight)
    {
        if (noAimVert)
            return pitchRadians + selfPitchRadians;
        var dz = aimVertNoOffset
            ? targetCenterZ - posZ + offsetHeight
            : targetCenterZ - posZ;
        return pitchRadians - VecToAngle(xyDist, dz);
    }

    public static double OriginZ(
        double posZ,
        double offsetHeight,
        double floorclip,
        bool fromBase,
        double height,
        double attackOffset) =>
        posZ + (fromBase ? 0 : height * 0.5 + attackOffset);

    public static double BasePosZ(double posZ, double offsetHeight, double floorclip) =>
        posZ + offsetHeight - floorclip;

    public static (double X, double Y, double Z) VelocityFromPitch(double pitchCos, double yawCos, double yawSin, double pitchSin) =>
        (pitchCos * yawCos, pitchCos * yawSin, -pitchSin);

    public static bool ZeroRangeUsesDefaults(double range) => range == 0;

    public static bool HitActorLinkAllowed(int hitType, bool badActor) =>
        hitType == TraceHitActor && !badActor;

    public static bool ShootabilityBlocks(int actorFlags, int actorFlags2) =>
        (actorFlags & MfShootable) == 0 || (actorFlags2 & Mf2Nonshootable) != 0;

    public static bool JumpRelationMatch(
        int flags,
        bool hostile,
        bool isFriend,
        bool isMonster) =>
        (flags & CloffJumpenemy) != 0 && hostile
        || (flags & CloffJumpfriend) != 0 && isFriend
        || (flags & CloffJumpobject) != 0 && !isMonster
        || (flags & CloffJumpnonhostile) != 0 && isMonster && !hostile;

    public static bool SkipRelationMatch(
        int flags,
        bool hostile,
        bool isFriend,
        bool isMonster) =>
        (flags & CloffSkipenemy) != 0 && hostile
        || (flags & CloffSkipfriend) != 0 && isFriend
        || (flags & CloffSkipobject) != 0 && !isMonster
        || (flags & CloffSkipnonhostile) != 0 && isMonster && !hostile;

    /// <summary>Managed mirror of <c>CheckLOFTraceFunc</c> (2286–2355).</summary>
    public static (LofTraceStatus Status, bool BadActor) EvaluateCheckLofTrace(
        int hitType,
        int hitActorId,
        int targetActorId,
        int flags,
        bool badActorAlready,
        int hitActorFlags,
        int hitActorFlags2,
        int hitActorFlags3,
        bool hostileToHit,
        bool friendToHit)
    {
        if (hitType != TraceHitActor)
            return (LofTraceStatus.Stop, badActorAlready);

        if (hitActorId == targetActorId)
        {
            if ((flags & CloffSkiptarget) != 0)
            {
                if ((flags & CloffBeyondtarget) != 0)
                    return (LofTraceStatus.Skip, badActorAlready);
                return (LofTraceStatus.Abort, badActorAlready);
            }
            return (LofTraceStatus.Stop, badActorAlready);
        }

        if ((flags & CloffMustbeshootable) != 0 && ShootabilityBlocks(hitActorFlags, hitActorFlags2))
            return (LofTraceStatus.Skip, badActorAlready);

        if ((flags & CloffMustbesolid) != 0 && (hitActorFlags & MfSolid) == 0)
            return (LofTraceStatus.Skip, badActorAlready);

        if ((flags & CloffMustbeghost) != 0)
        {
            if ((hitActorFlags3 & Mf3Ghost) == 0)
                return (LofTraceStatus.Skip, badActorAlready);
        }
        else if ((flags & CloffIgnoreghost) != 0 && (hitActorFlags3 & Mf3Ghost) != 0)
            return (LofTraceStatus.Skip, badActorAlready);

        var isMonster = (hitActorFlags3 & Mf3Ismonster) != 0;
        if (JumpRelationMatch(flags, hostileToHit, friendToHit, isMonster))
            return (LofTraceStatus.Stop, badActorAlready);

        if (SkipRelationMatch(flags, hostileToHit, friendToHit, isMonster))
            return (LofTraceStatus.Skip, badActorAlready);

        return (LofTraceStatus.Abort, true);
    }
}
