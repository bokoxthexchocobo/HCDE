namespace HCDE.Playsim;

/// <summary>Nightmare respawn and TID hash from <c>p_mobj.cpp</c> (3459–3700).</summary>
public static class VmPMobjNightmareHash
{
    public const int MfSpawnceiling = 1;
    public const int Mf2Spawnfloat = 1;
    public const int MfCountkill = 1;

    public static bool SpawnCeilingZ(int flags) => (flags & MfSpawnceiling) != 0;

    public static bool SpawnFloatZ(int flags2) => (flags2 & 1) != 0;

    public static bool FloorZClamp(double z, double floorz) => z < floorz;

    public static bool CeilingClamp(double top, double ceilingz) => top > ceilingz;

public static bool PositionBlocked(bool ok) => !ok;

public static bool ClearDormantSpawnflag(int spawnFlags) => (spawnFlags & 1) == 0;

public static bool PrevZPreserve(double z) => z >= 0;

public static bool TidZeroSkipsHash(int tid) => tid == 0;

public static int TidHashIndex(int tid) => tid & 127;

public static bool LinearSearch(int startTid) => startTid != 0;

public static bool TidNonZeroCandidate(int tid) => tid != 0;

    public static bool RandomSearch(int startZero) => startZero == 0;

    public static bool RandomBatch(int i, int limit) => i < limit;

    public static int GenPositiveTid(int raw) => raw & int.MaxValue;

    public static bool RecursiveFindFive(int tid) => tid != 0;

public static bool DefaultInfoCeiling(int flags) => (flags & MfSpawnceiling) != 0;

    public static bool DefaultInfoFloat(int flags2) => (flags2 & Mf2Spawnfloat) != 0;

public static bool DoubleFloorClamp(bool onfloor, bool below, bool above) => onfloor && (below || above);

public static bool CheckPositionDestroy(bool ok) => !ok;

public static bool TeleportFogBoth(bool old, bool newSpot) => old && newSpot;

public static bool LinearTidLoop(int tid, int limit) => tid <= limit;

    public static bool RandomTidBatch(int step) => step == 5;

    public static bool UniqueTidFound(int tid) => tid != 0;

    public static bool LimitZeroMeansMax(int limit) => limit == 0;

    public static bool DamageValNonNegative(int val) => val >= 0;

    public static bool MaskZeroMultiply(int mask, int add, int val) => mask == 0;

    public static bool MaskRollDamage(int mask, int add, int val) => mask != 0;

public static bool VmCallFailed(int ret) => ret < 1;

}
