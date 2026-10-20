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

    public static string ResolveGetTag(
        bool hasTag,
        string tagText,
        Func<string, string> stringTable,
        string? def,
        string className)
    {
        if (hasTag)
        {
            if (tagText.Length > 0 && tagText[0] == '$')
                return stringTable(tagText[1..]);
            return tagText;
        }

        if (def is { Length: > 0 })
            return def;
        return className;
    }

    public readonly record struct CounterFlags(int Flags, int Flags5);

    public static CounterFlags ApplyClearCounters(CounterFlags cf, bool countsAsKill, int health)
    {
        var flags = cf.Flags;
        var flags5 = cf.Flags5;
        if (ClearKillCounter(countsAsKill, health))
            flags &= ~MfCountkill;
        if (ClearItemCounter(flags))
            flags &= ~MfCountitem;
        if (ClearSecretCounter(flags5))
            flags5 &= ~Mf5Countsecret;
        return new CounterFlags(flags, flags5);
    }

    public static int CountInventoryBeforeEuthanize(ReadOnlySpan<bool> euthanizeMe)
    {
        var count = 0;
        foreach (var euth in euthanizeMe)
        {
            if (euth)
                break;
            count++;
        }

        return count;
    }

    public static int ApplyDamageFactorChain(int damage, double actorFactor, bool positive, int afterMobjFactor) =>
        positive ? afterMobjFactor : (int)(damage * actorFactor);

    public readonly record struct RestoreZContext(
        int Flags,
        int Flags2,
        double SpawnZ,
        double FloorZ,
        double CeilingZ,
        double Height,
        int Random256);

    public static double ChooseRestoreZ(RestoreZContext ctx)
    {
        if (RestoreUsesSpawnCeiling(ctx.Flags))
            return CeilingSpawnZ(ctx.CeilingZ, ctx.Height, ctx.SpawnZ);
        if (RestoreUsesSpawnFloat(ctx.Flags2))
        {
            var space = ctx.CeilingZ - ctx.Height - ctx.FloorZ;
            return SpawnFloatSpace(ctx.CeilingZ, ctx.FloorZ, ctx.Height)
                ? SpawnFloatZ(space - 40, ctx.Random256, ctx.FloorZ)
                : ctx.FloorZ;
        }

        return FloorSpawnZ(ctx.SpawnZ, ctx.FloorZ);
    }

    public static double FinalRestoreZ(double z, double floorz, double ceilingz, double height, int mfSolid)
    {
        if (ClampBelowFloor(z, floorz))
            z = floorz;
        if ((mfSolid & MfSolid) != 0 && ClampAboveCeiling(z + height, ceilingz))
            z = ceilingz - height;
        return z;
    }
}
