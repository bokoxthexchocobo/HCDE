namespace HCDE.Playsim;

/// <summary><c>P_PushUp</c>, <c>P_PushDown</c>, and <c>PIT_FloorDrop</c> policy from <c>p_map.cpp</c> (6762–6939).</summary>
public static class VmPMapSectorPush
{
    public const int Mf2Thruactors = 1;
    public const int Mf6Thruspecies = 1;
    public const int MfMissile = 1;
    public const int Mf2Reflective = 1 << 1;
    public const int Mf7Thureflect = 1;
    public const int Mf2Passmobj = 1 << 2;
    public const int Mf3Ismonster = 1;
    public const int Mf4Actlikebridge = 1;
    public const int MfNogravity = 1;
    public const int MfNoliftdrop = 1 << 1;
    public const int Mf5Movewithsector = 1 << 2;
    public const int Mf6Relativetofloor = 1;
    public const int SecfFloordrop = 1;

    public static bool PushUp_BlocksOnCeiling(double top, double ceilingZ) => top > ceilingZ;

    public static bool PushUp_SkipThruActors(int flags2) => (flags2 & Mf2Thruactors) != 0;

    public static bool SkipSameSpecies(int flags6, bool sameSpecies) =>
        (flags6 & Mf6Thruspecies) != 0 && sameSpecies;

    public static bool SkipReflectiveMissile(int thingFlags, int intersectFlags2, int intersectFlags7) =>
        (thingFlags & MfMissile) != 0
        && (intersectFlags2 & Mf2Reflective) != 0
        && (intersectFlags7 & Mf7Thureflect) != 0;

    public static bool CannotPushIntersect(int intersectFlags2, int intersectFlags3, int intersectFlags4, int intersectMass, int myMass) =>
        (intersectFlags2 & Mf2Passmobj) == 0
        || ((intersectFlags3 & Mf3Ismonster) == 0 && intersectMass > myMass)
        || (intersectFlags4 & Mf4Actlikebridge) != 0;

public static bool PushDown_BlocksOnFloor(double z, double floorZ) => z <= floorZ;

    public static bool OnlyPushDownWhenAbove(double oldZ, double targetZ) => oldZ > targetZ;

    public static bool FloorDrop_NoChange(double oldFloorZ, double newFloorZ) => oldFloorZ == newFloorZ;

    public static bool FloorDrop_SkipBridge(int flags4) => (flags4 & Mf4Actlikebridge) != 0;

    public static bool FloorDrop_GravityMove(
        double velZ,
        int flags,
        double z,
        double oldFloorZ,
        int sectorFlags,
        double moveAmt) =>
        velZ == 0
        && (((flags & MfNogravity) == 0)
            || (z == oldFloorZ && (flags & MfNoliftdrop) == 0))
        && ((flags & MfNogravity) != 0
            || (flags & Mf5Movewithsector) != 0
            || (((sectorFlags & SecfFloordrop) != 0 || moveAmt < 9)
                && z - oldFloorZ <= moveAmt));

    public static bool RelativeFloorFollow(int flags, int flags6) =>
        (flags & MfNogravity) != 0 && (flags6 & Mf6Relativetofloor) != 0;

public static double StackBelow(double thingZ, double height) => thingZ - height;

    public readonly record struct PushIntersectContext(
        int ThingFlags6,
        bool SameSpecies,
        int ThingFlags,
        int IntersectFlags2,
        int IntersectFlags7);

    public static bool SkipPushIntersect(PushIntersectContext ctx) =>
        SkipSameSpecies(ctx.ThingFlags6, ctx.SameSpecies)
        || SkipReflectiveMissile(ctx.ThingFlags, ctx.IntersectFlags2, ctx.IntersectFlags7);

    public static bool LiftDropWhenNoGravityChange(double z, double oldFloorZ, int flags) =>
        z != oldFloorZ && (flags & MfNoliftdrop) == 0;

public const int PushOk = 0;
    public const int PushCeilingBlocked = 1;
    public const int PushIntersectBlocked = 2;

    public static bool FloorRaise_OnFloor(double z, double floorZ) => z <= floorZ;

    public static bool FloorRaise_MarkNoFitBridge(int flags4) => (flags4 & Mf4Actlikebridge) != 0;

    public static bool FloorRaise_UseRelativePath(double z, double floorZ, int flags, int flags6) =>
        z > floorZ && RelativeFloorFollow(flags, flags6);

    public static bool FloorRaise_SnapToFloor(double z, double floorZ) => z <= floorZ;

    public readonly record struct PushUpDispatchContext(int Result, bool HadInstant);

    public static bool PushUp_KeepPositionAndCrunch(PushUpDispatchContext ctx) =>
        ctx.Result == PushIntersectBlocked;

    public static bool PushUp_CrunchAndUpdate(PushUpDispatchContext ctx) =>
        ctx.Result == PushCeilingBlocked || ctx.Result == PushOk;

    public static bool PushUp_RestoreOldZ(PushUpDispatchContext ctx) =>
        ctx.Result == PushIntersectBlocked;

    public static bool CeilingLower_AboveCeiling(double top, double ceilingZ) => top > ceilingZ;

    public static bool CeilingLower_FitsBetween(double ceilingZ, double height, double floorZ) =>
        ceilingZ - height >= floorZ;

    public static bool CeilingLower_ClampToFloor(double ceilingZ, double height, double floorZ) =>
        !(ceilingZ - height >= floorZ);

    public static bool CeilingLower_OnFloorAtStart(double z, double floorZ) => z <= floorZ;

    public readonly record struct CeilingLowerPushContext(int PushDownResult, bool StartedOnFloor);

    public static bool CeilingLower_CrunchOnPushFail(CeilingLowerPushContext ctx) =>
        ctx.PushDownResult == PushCeilingBlocked || ctx.PushDownResult == PushIntersectBlocked;

    public static bool CeilingLower_ResetToFloor(CeilingLowerPushContext ctx) =>
        CeilingLower_CrunchOnPushFail(ctx) && ctx.StartedOnFloor;

    public static bool CeilingRaise_SkipBridge(int flags4) => (flags4 & Mf4Actlikebridge) != 0;

    public static bool CeilingRaise_DoomCompatMove(
        double z,
        double floorZ,
        double top,
        double ceilingZ,
        double moveAmt,
        int flags) =>
        z < floorZ
        && top >= ceilingZ - moveAmt
        && (flags & MfNoliftdrop) == 0;

    public static bool CeilingRaise_PassobjStack(
        bool passMobj,
        bool adjustGood,
        double top,
        double ceilingZ) => passMobj && !adjustGood && top < ceilingZ;

    public static double CeilingRaise_StackZ(double ceilingZ, double height) => ceilingZ - height;

    public static double ViewZDelta(double newZ, double oldZ) => newZ - oldZ;

}
