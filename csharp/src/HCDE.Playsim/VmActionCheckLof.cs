namespace HCDE.Playsim;

/// <summary><c>CheckLOF</c> setup and result handling from <c>p_actionfunctions.cpp</c> (2385–2516).</summary>
public static class VmActionCheckLof
{
    public const int CloffMulHeight = 1 << 19;
    public const int CloffMulWidth = 1 << 20;
    public const int CloffFromBase = 1 << 18;
    public const int CloffCheckPartial = 1 << 13;
    public const int CloffJumpOnMiss = 1 << 21;
    public const int CloffSetTarget = 1 << 23;
    public const int CloffSetMaster = 1 << 24;
    public const int CloffSetTracer = 1 << 25;
    public const double PlayerMissileRange = 8192.0;
    public const double MissileRange = 2048.0;

    public static double ScaleOffsetHeight(int flags, bool isPlayer, double offsetHeight, double moHeight, double crouchFactor, double selfHeight) =>
        (flags & CloffMulHeight) != 0
            ? isPlayer ? offsetHeight * moHeight * crouchFactor : offsetHeight * selfHeight
            : offsetHeight;

    public static (double forward, double width) ScaleOffsetWidth(int flags, double offsetForward, double offsetWidth, double radius) =>
        (flags & CloffMulWidth) != 0
            ? (offsetForward * radius, offsetWidth * radius)
            : (offsetForward, offsetWidth);

    public static double PosZWithFloorclip(double posZ, double offsetHeight, double floorclip) =>
        posZ + offsetHeight - floorclip;

    public static double AddHitscanOriginZ(double posZ, int flags, double height, double attackOffset) =>
        (flags & CloffFromBase) != 0 ? posZ : posZ + height * 0.5 + attackOffset;

    public static bool TargetBeyondRange(bool hasTarget, double range, int flags, double distance3d) =>
        hasTarget && range > 0 && (flags & CloffCheckPartial) == 0 && distance3d > range;

    public static double DefaultLofRange(double range, bool isPlayer) =>
        range == 0 ? isPlayer ? PlayerMissileRange : MissileRange : range;

    public static (double vx, double vy, double vz) LofTraceUnitVector(double pitchCos, double angleCos, double angleSin, double pitchSin) =>
        (pitchCos * angleCos, pitchCos * angleSin, -pitchSin);

    public static bool LofHitAccepted(int hitTypeActor, int hitTypeNone, int flags, bool badActor, int hitType) =>
        hitType == hitTypeActor ||
        ((flags & CloffJumpOnMiss) != 0 && !badActor && hitType != hitTypeNone);

    public static bool RejectBelowMinRange(double minRange, double traceDistance) =>
        minRange > 0 && traceDistance < minRange;

    public static bool ShouldAssignLofActor(int hitTypeActor, int hitType, bool badActor, object? actor) =>
        hitType == hitTypeActor && actor is not null && !badActor;

    public static int SelectLofPointerOp(int flags) =>
        (flags & CloffSetTarget) != 0 ? 1 : (flags & CloffSetMaster) != 0 ? 2 : (flags & CloffSetTracer) != 0 ? 3 : 0;
}
