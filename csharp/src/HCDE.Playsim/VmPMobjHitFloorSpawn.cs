namespace HCDE.Playsim;

/// <summary>
/// <c>P_HitFloor</c>, missile net registration, <c>GetDefaultSpeed</c>, and
/// <c>P_SpawnMissileXYZ</c> aim policy from <c>p_mobj.cpp</c> (7672–7979).
/// </summary>
public static class VmPMobjHitFloorSpawn
{
    public const int Mf6Touchy = 1;
    public const int Mf6Armed = 1 << 1;
    public const int FfExists = 1;
    public const int FfSolid = 1 << 1;
    public const int FfSwimmable = 1 << 2;
    public const int Mf3Floorhugger = 1;
    public const int Mf3Ceilinghugger = 1 << 1;
    public const int Mf4Spectral = 1;
    public const double OnFloorZ = -32768;
    public const double OnCeilingZ = -32767;

    public static bool TouchyCrushOnImpact(int flags6, bool sentient, double velZ) =>
        (flags6 & Mf6Touchy) != 0
        && ((flags6 & Mf6Armed) != 0 || sentient)
        && velZ < -5;

    public static bool DisarmTouchy(int flags6) => (flags6 & Mf6Armed) != 0;

    public static bool ClientSideFloorMatch(double actorZ, double floorZ) => actorZ == floorZ;

    public static bool Valid3dFloor(int flags) => (flags & FfExists) != 0;

    public static bool SolidOrSwimmable3d(int flags) => (flags & (FfSolid | FfSwimmable)) != 0;

    public static bool RoverTopMatches(double roverTopZ, double actorZ) => roverTopZ == actorZ;

    public static bool RegisterMissileNet(bool missileNull, bool sourceNull) =>
        !missileNull && !sourceNull;

    public static bool UseFastMonsterSpeed(bool fastMonsters, double fastSpeed) =>
        fastMonsters && fastSpeed >= 0;

    public static double PickDefaultSpeed(bool fastMonsters, double fastSpeed, double normalSpeed) =>
        UseFastMonsterSpeed(fastMonsters, fastSpeed) ? fastSpeed : normalSpeed;

    public static bool SpawnMissile_NullSourceOrType(bool sourceNull, bool typeNull) =>
        sourceNull || typeNull;

    public static bool AdjustSpawnZForFloorclip(double posZ) =>
        posZ != OnFloorZ && posZ != OnCeilingZ;

    public static bool HuggerClearsVertical(int flags3) =>
        (flags3 & (Mf3Floorhugger | Mf3Ceilinghugger)) != 0;

    public static bool RaiseTrajectoryOverTarget(double spawnZ, double sourceZ, double destHeight) =>
        spawnZ - sourceZ >= destHeight;

    public static double TrajectoryZBoost(double destHeight, double spawnZ, double sourceZ) =>
        destHeight - spawnZ + sourceZ;

    public static bool SpectralFriend(int flags4) => (flags4 & Mf4Spectral) != 0;

    public readonly record struct HitFloorSectorWalkContext(
        bool IsClientSide,
        double ActorZ,
        double FloorZ,
        int RoverFlags,
        double RoverTopZ);

    public static bool TryHitWaterVia3dFloor(HitFloorSectorWalkContext ctx) =>
        ctx.IsClientSide
        && Valid3dFloor(ctx.RoverFlags)
        && SolidOrSwimmable3d(ctx.RoverFlags)
        && RoverTopMatches(ctx.RoverTopZ, ctx.ActorZ);

    public static bool TouchingSectorFloorMatch(double actorZ, double floorZ) => actorZ == floorZ;

    public readonly record struct SpawnMissileValidationContext(
        bool SourceNull,
        bool TypeNull,
        bool DestNull);

    public static bool RejectInvalidSpawn(SpawnMissileValidationContext ctx) =>
        SpawnMissile_NullSourceOrType(ctx.SourceNull, ctx.TypeNull) || ctx.DestNull;

    public static bool ApplyFloorclipWhenCustomZ(double posZ) => AdjustSpawnZForFloorclip(posZ);

}
