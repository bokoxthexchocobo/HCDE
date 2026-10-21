namespace HCDE.Playsim;

/// <summary><c>AActor::CheckLOF</c> from <c>p_actionfunctions.cpp</c> (2358–2517).</summary>
public static class VmActionCheckLof
{
    public const int CloffNoaimVert = 0x00000001;
    public const int CloffNoaimHorz = 0x00000002;
    public const int CloffAllownull = 0x00001000;
    public const int CloffCheckpartial = 0x00002000;
    public const int CloffFrombase = 0x00040000;
    public const int CloffMulHeight = 0x00080000;
    public const int CloffMulWidth = 0x00100000;
    public const int CloffJumpOnMiss = 0x00200000;
    public const int CloffAimVertNooffset = 0x00400000;

    public const int TraceHitActor = 2;
    public const int TraceHitNone = 0;

    public const int PlayerMissileRange = 32 * 64;
    public const int MissileRange = 32 * 64;

    public static bool TargetNullWithoutAllow(int flags, bool hasTarget) =>
        !hasTarget && (flags & CloffAllownull) == 0;

    public static double ScaleHeightOffset(bool mulHeight, bool hasPlayer, double offset, double height, double crouch) =>
        mulHeight ? (hasPlayer ? offset * height * crouch : offset * height) : offset;

    public static double ScaleWidthOffset(bool mulWidth, double offset, double radius) =>
        mulWidth ? offset * radius : offset;

    public static double TraceOriginZ(double posZ, double offsetHeight, double floorclip, bool fromBase, double height, double attackOffset) =>
        posZ + offsetHeight - floorclip + (fromBase ? 0 : height * 0.5 + attackOffset);

    public static bool RangeExceeded(double distance, double range, bool checkPartial) =>
        range > 0 && !checkPartial && distance > range;

    public static double ResolveRange(double range, bool hasPlayer) =>
        range == 0 ? (hasPlayer ? PlayerMissileRange : MissileRange) : range;

    public static (double Forward, double Side) ScaleOffsets(
        bool mulWidth,
        double offsetForward,
        double offsetWidth,
        double radius) =>
        mulWidth ? (offsetForward * radius, offsetWidth * radius) : (offsetForward, offsetWidth);

    public static (double X, double Y, double PitchRadians) AimFromTarget(
        bool noAimHorz,
        bool noAimVert,
        bool aimVertNoOffset,
        double selfYawRadians,
        double angleToTargetRadians,
        double paramAngleRadians,
        double paramPitchRadians,
        double selfPitchRadians,
        double offsetForward,
        double offsetWidth,
        double radius,
        bool mulWidth,
        double sinYaw,
        double cosYaw,
        double xyDistToTarget,
        double targetCenterZ,
        double posZ,
        double offsetHeight)
    {
        var ang = noAimHorz ? selfYawRadians : angleToTargetRadians;
        var angle = paramAngleRadians + ang;
        var (fwd, side) = ScaleOffsets(mulWidth, offsetForward, offsetWidth, radius);
        var xy = Vec2Offset(fwd, side, sinYaw, cosYaw);
        var pitch = AdjustPitchRadians(
            noAimVert, aimVertNoOffset, paramPitchRadians, selfPitchRadians,
            xyDistToTarget, targetCenterZ, posZ, offsetHeight);
        return (xy.X, xy.Y, pitch);
    }

    public static (double X, double Y, double AngleRadians, double PitchRadians) AimAllowNullTarget(
        double paramAngleRadians,
        double paramPitchRadians,
        double selfYawRadians,
        double selfPitchRadians,
        double offsetForward,
        double offsetWidth,
        double radius,
        bool mulWidth,
        double sinAngle,
        double cosAngle)
    {
        var angle = paramAngleRadians + selfYawRadians;
        var pitch = paramPitchRadians + selfPitchRadians;
        var (fwd, side) = ScaleOffsets(mulWidth, offsetForward, offsetWidth, radius);
        var xy = Vec2Offset(fwd, side, sinAngle, cosAngle);
        return (xy.X, xy.Y, angle, pitch);
    }

    public static (double X, double Y) Vec2Offset(double forward, double side, double sin, double cos) =>
        (forward * cos + side * sin, forward * sin - side * cos);

    public static double VecToAngle(double dist, double dz) =>
        dist == 0 ? (dz >= 0 ? double.PositiveInfinity : double.NegativeInfinity) : Math.Atan2(dz, dist);

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

    public static (double X, double Y, double Z) VelocityFromPitch(double pitchCos, double yawCos, double yawSin, double pitchSin) =>
        (pitchCos * yawCos, pitchCos * yawSin, -pitchSin);

    public static bool JumpOnMissSuccess(int flags, bool badActor, int hitType) =>
        (flags & CloffJumpOnMiss) != 0 && !badActor && hitType != TraceHitNone;

    public static bool MinRangeFail(double distance, double minRange) =>
        minRange > 0 && distance < minRange;

    public static bool TraceSuccess(bool hitActor, int flags, bool badActor, int hitType) =>
        hitActor || JumpOnMissSuccess(flags, badActor, hitType);

    public static bool ShouldLinkHitActor(int hitType, bool badActor) =>
        hitType == TraceHitActor && !badActor;
}
