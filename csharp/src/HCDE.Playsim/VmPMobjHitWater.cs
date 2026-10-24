namespace HCDE.Playsim;

/// <summary><c>P_GetThingFloorType</c> and <c>P_HitWater</c> from <c>p_mobj.cpp</c> (7475–7656).</summary>
public static class VmPMobjHitWater
{
    public const int CfPredicting = 1;
    public const int MfMissile = 1;
    public const int FfExists = 1;
    public const int FfSolid = 1 << 1;
    public const int FfSwimmable = 1 << 2;
    public const int SecmfClipfakeplanes = 1;
    public const int Mf3Ismonster = 1;
    public const int Mf3Dontsplash = 1;
    public const int ThwSmall = 1;
    public const int ThwNovel = 1 << 1;

    public static bool UseActorFloorTerrain(int floorTerrain) => floorTerrain >= 0;

    public static bool SkipWhenPredicting(bool hasPlayer, int cheats) =>
        hasPlayer && (cheats & CfPredicting) != 0;

    public static bool CheckAboveCenter(bool checkAbove, double posZ, double centerZ, double velZ, bool isMissile) =>
        checkAbove && (!isMissile || true) && posZ > centerZ - (isMissile ? velZ : 0);

    public static bool Valid3dFloor(int flags) => (flags & FfExists) != 0;

    public static bool Splashable3dTop(int flags, double alpha) =>
        (flags & (FfSolid | FfSwimmable)) != 0 || alpha > 0;

    public static bool ZNearPlane(double posZ, double planeZ) =>
        posZ > planeZ - 0.5 && posZ < planeZ + 0.5;

    public static bool BlockBelowBottomPlane(double planez, double posZ, double floorZ) =>
        planez < posZ && planez >= floorZ;

    public static bool UseHeightSecTerrain(bool force, bool hasHeightSec, int moreFlags) =>
        !force && hasHeightSec && (moreFlags & SecmfClipfakeplanes) == 0;

    public static bool NoSplashDefinition(int splashNum) => splashNum == -1;

    public static bool DealDamageOnLand(bool isPlayer, bool damageOnLand, int damageAmount, int timeMask, int levelTime) =>
        isPlayer && damageOnLand && damageAmount > 0 && (levelTime & timeMask) != 0;

    public static bool SkipUnderwaterFloor(bool underwater, double posZ, double floorZ) =>
        underwater && posZ <= floorZ;

    public static bool SkipNovelLowVelocity(int flags, bool monsterOrPlayer, double velZ, bool force) =>
        (flags & ThwNovel) == 0 && monsterOrPlayer && velZ >= -6 && !force;

    public static bool SmallSplashMass(int flags, int mass) =>
        (flags & ThwSmall) != 0 || mass < 10;

    public static bool AllowSplash(int flags3) => (flags3 & Mf3Dontsplash) == 0;

    public static bool UseSmallSplashDef(bool smallSplash, bool hasSmallClass) =>
        smallSplash && hasSmallClass;

    public static bool ClientSideSmallSplash(bool isClientSide, bool defaultClientSide) =>
        !isClientSide || defaultClientSide;

    public static bool LiquidWithoutSplash(int splashNum, bool isLiquid) =>
        splashNum == -1 && isLiquid;

    public readonly record struct PortalFloorWalkContext(
        double DirZ,
        double PosZ,
        double PortalPlaneZ,
        bool BlocksMovement);

    public static bool WalkDownThroughFloorPortal(PortalFloorWalkContext ctx) =>
        ctx.DirZ < 0
        && !ctx.BlocksMovement
        && ctx.PosZ < ctx.PortalPlaneZ;

    public static bool WalkUpThroughCeilingPortal(PortalFloorWalkContext ctx) =>
        ctx.DirZ > 0
        && !ctx.BlocksMovement
        && ctx.PosZ > ctx.PortalPlaneZ;

    public readonly record struct HitWater3dProbeContext(
        int Flags,
        double Alpha,
        double PosZ,
        double TopPlaneZ,
        double BottomPlaneZ,
        double ActorFloorZ);

    public static bool TrySplashAt3dTop(HitWater3dProbeContext ctx) =>
        Valid3dFloor(ctx.Flags)
        && Splashable3dTop(ctx.Flags, ctx.Alpha)
        && ZNearPlane(ctx.PosZ, ctx.TopPlaneZ);

    public static bool RejectBelowBottom(HitWater3dProbeContext ctx) =>
        BlockBelowBottomPlane(ctx.BottomPlaneZ, ctx.PosZ, ctx.ActorFloorZ);

    public readonly record struct SplashSpawnContext(
        bool AllowSplash,
        bool SmallSplash,
        bool HasSmallDef,
        bool IsClientSide,
        bool DefaultClientSide);

    public static bool SpawnSmallSplash(SplashSpawnContext ctx) =>
        ctx.AllowSplash && UseSmallSplashDef(ctx.SmallSplash, ctx.HasSmallDef)
        && ClientSideSmallSplash(ctx.IsClientSide, ctx.DefaultClientSide);

    public static bool SpawnRegularSplash(SplashSpawnContext ctx) =>
        ctx.AllowSplash && !ctx.SmallSplash;

    public static bool ReturnLiquidOnly(int splashNum, bool isLiquid) =>
        LiquidWithoutSplash(splashNum, isLiquid);
}
