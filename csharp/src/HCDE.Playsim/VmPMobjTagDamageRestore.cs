namespace HCDE.Playsim;

/// <summary>Tag, counters, damage modifiers, translation, restore position from <c>p_mobj.cpp</c> (8811–8974).</summary>
public static class VmPMobjTagDamageRestore
{
    public const int MfCountkill = 1;
    public const int MfCountitem = 1 << 1;
    public const int Mf5Countsecret = 1 << 2;
    public const int MfSpawnceiling = 1 << 3;
    public const int Mf2Spawnfloat = 1 << 4;
    public const int MfSolid = 1 << 5;
    public const int OfEuthanizeme = 1;
    public const int InvalidTranslation = -1;

    public static bool TagUsesStringTable(bool hasTag, bool dollarPrefix) => hasTag && dollarPrefix;

    public static bool TagUsesLiteral(bool hasTag, bool dollarPrefix) => hasTag && !dollarPrefix;

    public static bool TagFallsBackToDefault(bool hasTag, bool hasDef) => !hasTag && hasDef;

    public static bool TagFallsBackToClassName(bool hasTag, bool hasDef) => !hasTag && !hasDef;

    public static bool ClearTagOnEmpty(bool defNull, bool defEmpty) => defNull || defEmpty;

    public static bool CharacterNameFromConversation(bool hasConversation, bool nameLen) =>
        hasConversation && nameLen;

    public static bool CharacterNameDollar(bool dollarPrefix) => dollarPrefix;

    public static bool ClearKillCounter(bool countsAsKill, int health) => countsAsKill && health > 0;

    public static bool ClearItemCounter(int flags) => (flags & MfCountitem) != 0;

    public static bool ClearSecretCounter(int flags5) => (flags5 & Mf5Countsecret) != 0;

    public static int InventoryWalkNext(bool euthanize) => euthanize ? 0 : 1;

    public static bool ModifyDamageVirtual(bool hasVirtual) => hasVirtual;

    public static int ApplyPositiveDamageFactor(int damage, double factor) =>
        damage > 0 ? (int)(damage * factor) : damage;

    public static bool ApplyMobjFactorWhenPositive(int damage) => damage > 0;

    public static bool EmptyTranslationResets(bool firstCharZero) => firstCharZero;

    public static bool CustomTranslationFound(int tnum) => tnum != InvalidTranslation;

    public static bool RestoreUsesSpawnCeiling(int flags) => (flags & MfSpawnceiling) != 0;

    public static bool RestoreUsesSpawnFloat(int flags2) => (flags2 & Mf2Spawnfloat) != 0;

    public static bool SpawnFloatSpace(double ceiling, double floor, double height) =>
        ceiling - height - floor > 48;

    public static double SpawnFloatZ(double space, int random256, double floor) =>
        space * random256 / 256.0 + floor + 40;

    public static bool ClampBelowFloor(double z, double floorz) => z < floorz;

    public static bool ClampAboveCeiling(double top, double ceilingz) => top > ceilingz;

    public static double CeilingSpawnZ(double ceilingz, double height, double spawnZ) =>
        ceilingz - height - spawnZ;

    public static double FloorSpawnZ(double spawnZ, double floorz) => spawnZ + floorz;
}
