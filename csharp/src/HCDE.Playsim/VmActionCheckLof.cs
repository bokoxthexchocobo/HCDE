namespace HCDE.Playsim;

/// <summary><c>AActor::CheckLOF</c> from <c>p_actionfunctions.cpp</c> (2358–2517).</summary>
public static class VmActionCheckLof
{
    public const int CloffMulHeight = 0x00000001;
    public const int CloffMulWidth = 0x00000002;
    public const int CloffFrombase = 0x00000004;
    public const int CloffCheckpartial = 0x00000008;
    public const int CloffNoaimHorz = 0x00000010;
    public const int CloffNoaimVert = 0x00000020;
    public const int CloffAimVertNooffset = 0x00000040;
    public const int CloffAllownull = 0x00000080;
    public const int CloffJumpOnMiss = 0x00000100;
    public const int CloffSettarget = 0x00000200;
    public const int CloffSetmaster = 0x00000400;
    public const int CloffSettracer = 0x00000800;

    public const int PlayerMissileRange = 32 * 64;
    public const int MissileRange = 32 * 64;

    public static bool TargetNullWithoutAllow(int flags, bool hasTarget) =>
        !hasTarget && (flags & CloffAllownull) == 0;

    public static double ScaleHeightOffset(bool mulHeight, bool hasPlayer, double offset, double height, double crouch) =>
        mulHeight ? (hasPlayer ? offset * height * crouch : offset * height) : offset;

    public static double ScaleWidthOffset(bool mulWidth, double offset, double radius) =>
        mulWidth ? offset * radius : offset;

    public static bool RangeExceeded(double distance, double range, bool checkPartial) =>
        range > 0 && !checkPartial && distance > range;

    public static bool UseActorYaw(bool noAimHorz) => noAimHorz;

    public static bool UseActorPitch(bool noAimVert) => noAimVert;

    public static bool AimVertNoOffset(bool flag) => flag;

    public static double DefaultRange(bool hasPlayer, int playerRange, int missileRange) =>
        hasPlayer ? playerRange : missileRange;

    public static bool TraceHitActor(int hitType, int traceHitActor) => hitType == traceHitActor;

    public static bool JumpOnMissSuccess(int flags, bool badActor, int hitType, int traceHitNone) =>
        (flags & CloffJumpOnMiss) != 0 && !badActor && hitType != traceHitNone;

    public static bool MinRangeFail(double distance, double minRange) =>
        minRange > 0 && distance < minRange;

    public static bool ShouldSetTarget(int flags) => (flags & CloffSettarget) != 0;

    public static bool ShouldSetMaster(int flags) => (flags & CloffSetmaster) != 0;

    public static bool ShouldSetTracer(int flags) => (flags & CloffSettracer) != 0;

    public static bool SuccessPath(bool hitActor, bool jumpOnMiss) => hitActor || jumpOnMiss;

    public static (double X, double Y) Vec2Offset(double forward, double side, double sin, double cos) =>
        (forward * cos + side * sin, forward * sin - side * cos);

    public static (double X, double Y, double Z) VelocityFromPitch(double pitchCos, double yawCos, double yawSin, double pitchSin) =>
        (pitchCos * yawCos, pitchCos * yawSin, -pitchSin);

    public static bool AddHalfHeightToOrigin(bool fromBase) => !fromBase;

    public static double CenterZOffset(double targetCenter, double posZ, double offsetHeight, bool aimVertNoOffset) =>
        aimVertNoOffset ? targetCenter - posZ + offsetHeight : targetCenter - posZ;
}
