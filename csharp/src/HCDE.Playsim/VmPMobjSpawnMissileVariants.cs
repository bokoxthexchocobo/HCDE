namespace HCDE.Playsim;

/// <summary>
/// Missile spawn helpers after <c>P_SpawnMissileXYZ</c> velocity setup in <c>p_mobj.cpp</c> (7980–8212).
/// </summary>
public static class VmPMobjSpawnMissileVariants
{
    public const int Mf4Spectral = 1;
    public const int MfMissile = 1;
    public const double OnFloorZ = -32768;
    public const double OnCeilingZ = -32767;
    public const double DefaultSpawnZOffset = 32;

    public static bool FailCheckMissileSpawn(bool checkSpawn, bool spawnOk) =>
        checkSpawn && !spawnOk;

    public static bool SpectralFriendFromOwner(int flags4) => (flags4 & Mf4Spectral) != 0;

    public static bool NullSource(bool sourceNull) => sourceNull;

    public static double SpawnMissileZOffset(double bobOffset) => DefaultSpawnZOffset + bobOffset;

    public static bool OldSpawn_NullArgs(bool sourceNull, bool typeNull) => sourceNull || typeNull;

    public static double OldSpawnVelZ(double destZ, double sourceZ, double dist) =>
        dist != 0 ? (destZ - sourceZ) / dist : 0;

    public static double OldSpawnDistanceScale(double dist, double speed) =>
        dist / (speed < 1 ? 1 : speed);

    public readonly record struct AimedMissileContext(
        double Distance2d,
        double DefaultSpeed,
        double DestZ,
        double SourceZ);

    public static double AimedVerticalSpeed(AimedMissileContext ctx)
    {
        var time = ctx.Distance2d / ctx.DefaultSpeed;
        return time != 0 ? (ctx.DestZ - ctx.SourceZ) / time : ctx.DefaultSpeed;
    }

    public static bool AngleSpawn_NullArgs(bool sourceNull, bool typeNull) => sourceNull || typeNull;

    public static bool AdjustAngleSpawnZ(double z) => z != OnFloorZ && z != OnCeilingZ;

    public static bool SubMissile_NullAfterSpawn(bool sourceNull, bool typeNull) => sourceNull || typeNull;

    public static bool SubMissileSpectralInherit(
        int otherFlags4,
        int sourceFlags,
        int sourceFlags4) =>
        (otherFlags4 & Mf4Spectral) != 0
        && (sourceFlags & MfMissile) != 0
        && (sourceFlags4 & Mf4Spectral) != 0;

    public static bool SubMissileSpectralFromTarget(int otherFlags4) =>
        (otherFlags4 & Mf4Spectral) != 0;

    public static bool SubMissileSpawnOk(bool checkSpawnOk) => checkSpawnOk;
}
