namespace HCDE.Playsim;

/// <summary><c>PIT_FloorRaise</c>, <c>PIT_CeilingLower</c>, and <c>PIT_CeilingRaise</c> from <c>p_map.cpp</c> (6947–7104).</summary>
public static class VmPMapPitFloorCeiling
{
    public const int Mf4Actlikebridge = 1;
    public const int MfNogravity = 1;
    public const int MfNoliftdrop = 1 << 1;
    public const int Mf6Relativetofloor = 1;
    public const int Mf2Passmobj = 1;
    public const int PushOk = 0;
    public const int PushCeiling = 1;
    public const int PushBlocked = 2;

    public static bool FloorRaise_UnchangedFloor(double oldFloorZ, double newFloorZ) =>
        oldFloorZ == newFloorZ;

    public static bool FloorRaise_OnRidingFloor(double z, double floorZ) => z <= floorZ;

    public static bool FloorRaise_BridgeBlocks(int flags4) => (flags4 & Mf4Actlikebridge) != 0;

    public static bool FloorRaise_RelativeNoGravity(int flags, int flags6) =>
        (flags & MfNogravity) != 0 && (flags6 & Mf6Relativetofloor) != 0;

    public static bool FloorRaise_AboveFloorNoMove(double z, double floorZ, int flags, int flags6) =>
        z > floorZ && !FloorRaise_RelativeNoGravity(flags, flags6);

    public readonly record struct FloorRaisePushOutcome(int Code);

    public static bool FloorRaise_CrunchDefault(FloorRaisePushOutcome o) =>
        o.Code == PushOk || o.Code == PushCeiling;

    public static bool FloorRaise_RestoreZ(FloorRaisePushOutcome o) => o.Code == PushBlocked;

    public static bool CeilingLower_TopAboveCeiling(double top, double ceilingZ) => top > ceilingZ;

    public static bool CeilingLower_BridgeBlocks(int flags4) => (flags4 & Mf4Actlikebridge) != 0;

    public static bool CeilingLower_FitHeight(double ceilingZ, double height, double floorZ) =>
        ceilingZ - height >= floorZ;

    public static double CeilingLower_TargetZ(double ceilingZ, double height, double floorZ) =>
        CeilingLower_FitHeight(ceilingZ, height, floorZ) ? ceilingZ - height : floorZ;

    public static bool CeilingLower_StartedOnFloor(double z, double floorZ) => z <= floorZ;

    public readonly record struct CeilingLowerOutcome(int PushDownCode, bool OnFloor);

    public static bool CeilingLower_CrunchPaths(CeilingLowerOutcome o) =>
        o.PushDownCode == PushCeiling || o.PushDownCode == PushBlocked;

    public static bool CeilingLower_RevertToFloor(CeilingLowerOutcome o) =>
        CeilingLower_CrunchPaths(o) && o.OnFloor;

    public static bool CeilingRaise_SkipBridge(int flags4) => (flags4 & Mf4Actlikebridge) != 0;

    public static bool CeilingRaise_DoomMove(
        double z,
        double floorZ,
        double top,
        double ceilingZ,
        double moveAmt,
        int flags) =>
        z < floorZ
        && top >= ceilingZ - moveAmt
        && (flags & MfNoliftdrop) == 0;

    public static bool CeilingRaise_PassobjRelocate(
        bool passMobj,
        bool adjustOk,
        double top,
        double ceilingZ,
        double onTopZ) =>
        passMobj && !adjustOk && top < ceilingZ && onTopZ <= top;

    public static double MinStackZ(double a, double b) => a < b ? a : b;

    public static bool PlayerViewMo(bool hasPlayer, bool moIsSelf) => hasPlayer && moIsSelf;

    public static double ViewOffset(double newZ, double oldZ) => newZ - oldZ;

public readonly record struct FloorRaiseBranchContext(
        double Z,
        double FloorZ,
        int Flags,
        int Flags6);

    public static bool FloorRaise_TakeRidingBranch(FloorRaiseBranchContext ctx) =>
        FloorRaise_OnRidingFloor(ctx.Z, ctx.FloorZ);

    public static bool FloorRaise_TakeRelativeBranch(FloorRaiseBranchContext ctx) =>
        FloorRaise_RelativeNoGravity(ctx.Flags, ctx.Flags6) && ctx.Z > ctx.FloorZ;

    public static bool CeilingLower_NeedsPush(double top, double ceilingZ) =>
        CeilingLower_TopAboveCeiling(top, ceilingZ);

    public static bool CeilingRaise_AdjustFailed(bool isGood) => !isGood;
}
