namespace HCDE.Playsim;

/// <summary><c>SpawnMapThing</c> head and invasion/sound bands from <c>p_mobj.cpp</c> (6569–6663, 6768–6836).</summary>
public static class VmPMobjSpawnMapThingBands
{
    public const int Df2AlwaysSpawnMulti = 1;
    public const int LevelFilterstarts = 1;
    public const int MtfDeathmatch = 1;
    public const int MtfCooperative = 1 << 1;
    public const int MtfSingle = 1 << 2;
    public const int GiShareware = 1;
    public const int MfNotdmatch = 1;
    public const int MaxUnknownThingWarnings = 16;
    public const int SmtDeathmatchStart = 9;
    public const int SmtSseqOverride = 10;
    public const int SmtPolyAnchor = 11;
    public const int SmtPolySpawnHurt = 14;
    public const int SmtPlayer1Start = 1;
    public const int SeqTypeClamp = 255;
    public const int SeqTypeMax = 63;
    public const int InvasionDelayMinSeconds = 10;

    public static bool InvalidEdNum(int edNum) => edNum == 0 || edNum == -1;

    public static bool ShouldWarnUnknownThing(int warnings) => warnings < MaxUnknownThingWarnings;

    public static bool SuppressFurtherUnknownWarnings(int warnings) =>
        warnings == MaxUnknownThingWarnings;

    public static bool ExplicitNoSpawn(bool typeNull, int special) => typeNull && special <= 0;

    public static bool CopyEntryArgs(int argsDefined) => argsDefined > 0;

    public static bool IsDeathmatchStart(int special) => special == SmtDeathmatchStart;

    public static bool IsPolySpawnSpecial(int special) =>
        special >= SmtPolyAnchor && special <= SmtPolySpawnHurt;

    public static int PlayerStartIndex(int special) => special - SmtPlayer1Start;

    public static bool SpawnMultiSkill(bool skillSpawnMulti, int dmflags2) =>
        skillSpawnMulti || (dmflags2 & Df2AlwaysSpawnMulti) != 0;

    public readonly record struct GameTypeMaskContext(
        bool Deathmatch,
        bool Multiplayer,
        bool SpawnMulti,
        bool SpawnMultiCoopOnly);

    public static int ResolveThingMask(GameTypeMaskContext ctx)
    {
        if (ctx.Deathmatch) return MtfDeathmatch;
        if (ctx.Multiplayer) return MtfCooperative;
        if (ctx.SpawnMulti || ctx.SpawnMultiCoopOnly)
            return ctx.SpawnMultiCoopOnly ? MtfCooperative : MtfCooperative | MtfSingle;
        return MtfSingle;
    }

    public static bool ThingPassesMask(int thingFlags, int mask) => (thingFlags & mask) != 0;

    public static bool SkillFilterPasses(int skillFilter, int skillMask, bool noSkillFlags) =>
        (skillFilter & skillMask) != 0 || noSkillFlags;

    public static bool SinglePlayerClassPasses(int classFilter, int spawnClass) =>
        spawnClass == 0 || (classFilter & spawnClass) != 0;

    public static bool CoopClassMaskAllows(int classFilter, int combinedMask) =>
        combinedMask == -1 || (classFilter & combinedMask) != 0;

    public static int NormalizeSeqOverrideArg(int arg0) => arg0 == SeqTypeClamp ? -1 : arg0;

    public static bool SeqOverrideInRange(int type) => type <= SeqTypeMax;

    public static bool SoundSequenceOverride(bool typeNull, int special) =>
        typeNull && special == SmtSseqOverride;

    public static int InvasionDelaySeconds(int arg1) =>
        arg1 < InvasionDelayMinSeconds ? InvasionDelayMinSeconds : arg1;

    public static bool SharewareMissingFrames(int gameFlags) => (gameFlags & GiShareware) != 0;

    public static bool BlockDeathmatchThing(bool deathmatch, int defaultFlags) =>
        deathmatch && (defaultFlags & MfNotdmatch) != 0;

    public const int Df2NoCoopThingSpawn = 1;
    public const int Df3NoCoopOnlyThings = 1;
    public const int Df3NoCoopOnlyItems = 1 << 1;
    public const int DfNoCoopWeaponSpawn = 1;
    public const int Level2Nomonsters = 1;
    public const int DfNoMonsters = 1;
    public const int Mf3Ismonster = 1;
    public const int Mf7Weaponspawn = 1;
    public const int Mf2Floatbob = 1;
    public const int Mf2Floorclip = 1 << 1;
    public const int Mf2Argsdefined = 1 << 2;
    public const int MfNogravity = 1 << 3;

    public static bool ApplyGeneralSpawnFilters(int playerStartIndex, int levelFlags) =>
        playerStartIndex == -1 || (levelFlags & LevelFilterstarts) != 0;

    public static int FoldCoopSpawnClass(int combinedMask, int spawnClass)
    {
        if (spawnClass == 0) return -1;
        return combinedMask | spawnClass;
    }

    public static bool CoopClassFilterRejects(int classFilter, int combinedMask) =>
        combinedMask != -1 && (classFilter & combinedMask) == 0;

    public static bool BlockDmOnlyInCoop(int dmflags2, int thingFlags) =>
        (dmflags2 & Df2NoCoopThingSpawn) != 0
        && (thingFlags & (MtfDeathmatch | MtfSingle)) == MtfDeathmatch;

    public static bool IsCoopOnlyThing(int special, int thingId, int thingFlags) =>
        special == 0 && thingId == 0
        && (thingFlags & (MtfCooperative | MtfSingle)) == MtfCooperative;

    public static bool BlockCoopOnlyThing(int dmflags3, bool coopOnlyThing) =>
        coopOnlyThing && (dmflags3 & Df3NoCoopOnlyThings) != 0;

    public static bool BlockCoopOnlyInventory(int dmflags3, bool coopOnlyThing, bool isInventory) =>
        coopOnlyThing && isInventory && (dmflags3 & Df3NoCoopOnlyItems) != 0;

    public static bool BlockCoopWeaponSpawn(int dmflags, bool weaponSpawnClass, int thingFlags) =>
        (dmflags & DfNoCoopWeaponSpawn) != 0 && weaponSpawnClass
        && (thingFlags & (MtfDeathmatch | MtfSingle)) == MtfDeathmatch;

    public static bool BlockMonsterSpawn(int levelFlags2, int dmflags, int defaultFlags3) =>
        ((levelFlags2 & Level2Nomonsters) != 0 || (dmflags & DfNoMonsters) != 0)
        && (defaultFlags3 & Mf3Ismonster) != 0;

    public static bool CopySpecialFromEntry(bool typeNotNull) => typeNotNull;

    public static bool ApplyMapthingGravityZero(int gravity) => gravity == 0;

    public static bool SkipMapthingArgs(bool argsDefinedOnActor) => argsDefinedOnActor;
}
