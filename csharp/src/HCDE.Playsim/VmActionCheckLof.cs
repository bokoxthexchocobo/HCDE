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
    public const int CloffSettarget = 0x00800000;
    public const int CloffSetmaster = 0x01000000;
    public const int CloffSettracer = 0x02000000;

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

    public static void ApplyMulWidthToOffsets(int flags, ref double offsetForward, ref double offsetWidth, double radius)
    {
        if ((flags & CloffMulWidth) == 0)
            return;
        offsetForward *= radius;
        offsetWidth *= radius;
    }

    public static double TraceOriginZ(double posZ, double offsetHeight, double floorclip, bool fromBase, double height, double attackOffset) =>
        posZ + offsetHeight - floorclip + (fromBase ? 0 : height * 0.5 + attackOffset);

    public static bool RangeExceeded(double distance, double range, bool checkPartial) =>
        range > 0 && !checkPartial && distance > range;

    public static bool EarlyTargetRangeFail(int flags, bool hasTarget, double distance3D, double range) =>
        hasTarget && RangeExceeded(distance3D, range, (flags & CloffCheckpartial) != 0);

    public static double ResolveRange(double range, bool hasPlayer) =>
        range == 0 ? (hasPlayer ? PlayerMissileRange : MissileRange) : range;

    public static (double Forward, double Side) ScaleOffsets(
        bool mulWidth,
        double offsetForward,
        double offsetWidth,
        double radius) =>
        mulWidth ? (offsetForward * radius, offsetWidth * radius) : (offsetForward, offsetWidth);

    public static (double X, double Y, double AngleRadians, double PitchRadians) AimFromTarget(
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
        double xyDistToTarget,
        double targetCenterZ,
        double posZ,
        double offsetHeight)
    {
        var aimRad = noAimHorz ? selfYawRadians : angleToTargetRadians;
        var traceAngleRad = paramAngleRadians + aimRad;
        var sinAim = Math.Sin(aimRad);
        var cosAim = Math.Cos(aimRad);
        var (fwd, side) = ScaleOffsets(mulWidth, offsetForward, offsetWidth, radius);
        var xy = Vec2Offset(fwd, side, sinAim, cosAim);
        var pitch = AdjustPitchRadians(
            noAimVert, aimVertNoOffset, paramPitchRadians, selfPitchRadians,
            xyDistToTarget, targetCenterZ, posZ, offsetHeight);
        return (xy.X, xy.Y, traceAngleRad, pitch);
    }

    public static (double X, double Y, double AngleRadians, double PitchRadians) AimAllowNullTarget(
        double paramAngleRadians,
        double paramPitchRadians,
        double selfYawRadians,
        double selfPitchRadians,
        double offsetForward,
        double offsetWidth,
        double radius,
        bool mulWidth)
    {
        var traceAngleRad = paramAngleRadians + selfYawRadians;
        var pitch = paramPitchRadians + selfPitchRadians;
        var sinAngle = Math.Sin(traceAngleRad);
        var cosAngle = Math.Cos(traceAngleRad);
        var (fwd, side) = ScaleOffsets(mulWidth, offsetForward, offsetWidth, radius);
        var xy = Vec2Offset(fwd, side, sinAngle, cosAngle);
        return (xy.X, xy.Y, traceAngleRad, pitch);
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

    public static (double X, double Y, double Z) BuildTraceUnitVelocityNative(double pitchRadians, double angleRadians)
    {
        var cp = Math.Cos(pitchRadians);
        var yawCos = Math.Cos(angleRadians);
        var yawSin = Math.Sin(angleRadians);
        var pitchSin = Math.Sin(pitchRadians);
        return (cp * yawCos, cp * yawSin, -pitchSin);
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

    public static void ApplyLofHitActorLinksNative(
        int flags,
        int hitType,
        bool badActor,
        bool traceActorPresent,
        Action? setTarget,
        Action? setMaster,
        Action? setTracer)
    {
        if (!ShouldLinkHitActor(hitType, badActor) || !traceActorPresent)
            return;
        if ((flags & CloffSettarget) != 0)
            setTarget?.Invoke();
        if ((flags & CloffSetmaster) != 0)
            setMaster?.Invoke();
        if ((flags & CloffSettracer) != 0)
            setTracer?.Invoke();
    }

    public static bool EvaluateCheckLofAfterTraceNative(
        int hitType,
        int flags,
        bool badActor,
        double traceDistance,
        double minRange,
        bool traceActorPresent,
        Action? setTarget,
        Action? setMaster,
        Action? setTracer)
    {
        var hitActor = hitType == TraceHitActor;
        if (!TraceSuccess(hitActor, flags, badActor, hitType))
            return false;
        if (MinRangeFail(traceDistance, minRange))
            return false;
        ApplyLofHitActorLinksNative(flags, hitType, badActor, traceActorPresent, setTarget, setMaster, setTracer);
        return true;
    }
}
