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

    public static bool Refind3dFloors(bool onfloor) => onfloor;

    public static bool PositionBlocked(bool ok) => !ok;

    public static bool ClearCountersOnFail(bool blocked) => blocked;

    public static bool CopySpawnAttribs(bool always) => always;

    public static bool ClearDormantSpawnflag(int spawnFlags) => (spawnFlags & 1) == 0;

    public static bool Reactiontime18(bool set) => set;

    public static bool PrevZPreserve(double z) => z >= 0;

    public static bool SpawnFogOldSpot(bool spawn) => spawn;

    public static bool SpawnFogNewSpot(bool spawn) => spawn;

    public static bool DestroyOldMobj(bool destroy) => destroy;

    public static bool TidZeroSkipsHash(int tid) => tid == 0;

    public static bool ClientSideHash(bool client) => client;

    public static int TidHashIndex(int tid) => tid & 127;

    public static bool LinkInext(bool hasNext) => hasNext;

    public static bool RemoveUnlinks(bool hasPrev) => hasPrev;

    public static bool ClearTidOnRemove(bool tidNonZero) => tidNonZero;

    public static bool EuthanizeSkipsSetTid(bool euthanize) => euthanize;

    public static bool IsTidUsedWalk(bool found) => found;

    public static bool LinearSearch(int startTid) => startTid != 0;

    public static bool OverflowClampLimit(bool overflow) => overflow;

    public static bool TidNonZeroCandidate(int tid) => tid != 0;

    public static bool RandomSearch(int startZero) => startZero == 0;

    public static bool RandomBatch(int i, int limit) => i < limit;

    public static int GenPositiveTid(int raw) => raw & int.MaxValue;

    public static bool RecursiveFindFive(int tid) => tid != 0;

    public static bool SkillRespawnIncrement(bool inc) => inc;

    public static bool DefaultInfoCeiling(int flags) => (flags & MfSpawnceiling) != 0;

    public static bool DefaultInfoFloat(int flags2) => (flags2 & Mf2Spawnfloat) != 0;

    public static bool SpawnHealthCopy(bool copy) => copy;

    public static bool AddSpawnPointZ(bool onfloor) => onfloor;

    public static bool DoubleFloorClamp(bool onfloor, bool below, bool above) => onfloor && (below || above);

    public static bool FfcfSameSector3d(bool find) => find;

    public static bool CheckPositionDestroy(bool ok) => !ok;

    public static bool PreservePrevZ(double z) => true;

    public static bool TeleportFogBoth(bool old, bool newSpot) => old && newSpot;

    public static bool FriendlinessCopy(bool copy) => copy;

    public static bool TranslationCopy(bool copy) => copy;

    public static bool SkillCountCopy(bool copy) => copy;

    public static bool HashSlotHead(bool client) => client;

    public static bool InextPrevLink(bool link) => link;

    public static bool RemoveClearsTid(bool remove) => remove;

    public static bool SetTidRemoveFirst(bool always) => always;

    public static bool SetTidAddAfter(bool add) => add;

    public static bool ProbeWalkChain(bool more) => more;

    public static bool LinearTidLoop(int tid, int limit) => tid <= limit;

    public static bool RandomTidBatch(int step) => step == 5;

    public static bool UniqueTidFound(int tid) => tid != 0;

    public static bool LimitZeroMeansMax(int limit) => limit == 0;
}
