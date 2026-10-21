namespace HCDE.Playsim;

/// <summary><c>A_SetHealth</c>, <c>A_ResetHealth</c>, <c>A_SetSpecies</c>, <c>A_SetChaseThreshold</c>, and <c>CheckBlock</c> bands from <c>p_actionfunctions.cpp</c> (4652–4888).</summary>
public static class VmActorHealthAndCheckBlock
{
    [Flags]
    public enum CbfFlags
    {
        NoLines = 1 << 0,
        SetTarget = 1 << 1,
        SetMaster = 1 << 2,
        SetTracer = 1 << 3,
        SetOnPtr = 1 << 4,
        Dropoff = 1 << 5,
        NoActors = 1 << 6,
        AbsolutePos = 1 << 7,
        AbsoluteAngle = 1 << 8,
    }

    public static int ResolveHealthAssignment(int health, bool isPlayer) =>
        health <= 0 ? 1 : health;

    public static (int MoHealth, int PlayerHealth) AssignPlayerHealth(int health, int currentMo, int currentPlayer)
    {
        var resolved = ResolveHealthAssignment(health, true);
        return (resolved, resolved);
    }

    public static int AssignMobjHealth(int health) => ResolveHealthAssignment(health, false);

    public static bool ShouldSkipHealthPointer(bool hasMobj) => !hasMobj;

    public static bool ShouldResetPlayerHealth(bool hasPlayer, int moHealth) =>
        hasPlayer && moHealth > 0;

    public static bool ShouldResetMobjHealth(bool hasMobj, int health) =>
        hasMobj && health > 0;

    public static int ClampChaseThreshold(int threshold) => threshold >= 0 ? threshold : 0;

    public static (int DefThreshold, int Threshold) AssignChaseThreshold(int value, bool useDefault) =>
        useDefault
            ? (ClampChaseThreshold(value), 0)
            : (0, ClampChaseThreshold(value));

    public static double MergeCheckBlockAngle(double baseYaw, double angle, CbfFlags flags) =>
        (flags & CbfFlags.AbsoluteAngle) != 0 ? angle : baseYaw + angle;

    public static (double X, double Y, double Z) PlanCheckBlockOffset(
        double xofs,
        double yofs,
        double zofs,
        double angleRadians,
        CbfFlags flags,
        Func<double, double, double, (double X, double Y, double Z)> vec3Offset)
    {
        if ((flags & CbfFlags.AbsolutePos) != 0)
            return (xofs, yofs, zofs);
        var sin = Math.Sin(angleRadians);
        var cos = Math.Cos(angleRadians);
        return vec3Offset(xofs * cos + yofs * sin, xofs * sin - yofs * cos, zofs);
    }

    public static int PlanDropoffCheckFlags(CbfFlags flags)
    {
        var fpass = 0;
        if ((flags & CbfFlags.Dropoff) != 0) fpass |= 1;
        if ((flags & CbfFlags.NoActors) != 0) fpass |= 2;
        if ((flags & CbfFlags.NoLines) != 0) fpass |= 4;
        return fpass;
    }

    public static bool EarlyCheckBlockFail(bool checker) => checker;

    public static bool CheckBlockReturnValue(
        CbfFlags flags,
        bool blockingActorPresent,
        bool blockingLinePresent,
        bool dropoffCheckFailed) =>
        ((flags & CbfFlags.NoActors) == 0 && blockingActorPresent)
        || ((flags & CbfFlags.NoLines) == 0 && blockingLinePresent)
        || ((flags & CbfFlags.Dropoff) != 0 && dropoffCheckFailed);

    public static bool ShouldApplyBlockingPointer(
        CbfFlags flags,
        bool setTarget,
        bool setMaster,
        bool setTracer) =>
        (setTarget && (flags & CbfFlags.SetTarget) != 0)
        || (setMaster && (flags & CbfFlags.SetMaster) != 0)
        || (setTracer && (flags & CbfFlags.SetTracer) != 0);

    public static bool BlockingSetterIsPointer(CbfFlags flags) => (flags & CbfFlags.SetOnPtr) != 0;
}
