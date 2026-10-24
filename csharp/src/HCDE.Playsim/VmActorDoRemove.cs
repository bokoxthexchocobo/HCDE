namespace HCDE.Playsim;

/// <summary><c>DoRemove</c>, <c>A_Remove*</c>, and telefog bands from <c>p_actionfunctions.cpp</c> (4430–4635).</summary>
public static class VmActorDoRemove
{
    [Flags]
    public enum RmvfFlags
    {
        Missiles = 1 << 0,
        NoMonsters = 1 << 1,
        Misc = 1 << 2,
        Everything = 1 << 3,
        ExFilter = 1 << 4,
        ExSpecies = 1 << 5,
        Either = 1 << 6,
    }

    public enum RemovePlan
    {
        Skip,
        RemoveOnce,
        RemoveMany,
    }

    public static bool PassesRemoveFilter(bool filterPass, bool speciesPass, RmvfFlags flags) =>
        (flags & RmvfFlags.Either) != 0 ? filterPass || speciesPass : filterPass && speciesPass;

    public static bool RemoveEverything(RmvfFlags flags) => (flags & RmvfFlags.Everything) != 0;

    public static bool RemoveMisc(bool isMonster, bool isMissile, RmvfFlags flags) =>
        (flags & RmvfFlags.Misc) != 0 && !(isMonster && isMissile);

    public static bool RemoveMonster(bool isMonster, RmvfFlags flags) =>
        isMonster && (flags & RmvfFlags.NoMonsters) == 0;

    public static bool RemoveMissile(bool isMissile, RmvfFlags flags) =>
        isMissile && (flags & RmvfFlags.Missiles) != 0;

    public static int CountRemovePasses(
        bool passesFilter,
        bool isMonster,
        bool isMissile,
        RmvfFlags flags)
    {
        if (!passesFilter)
            return 0;

        var count = 0;
        if (RemoveEverything(flags)) count++;
        if (RemoveMisc(isMonster, isMissile, flags)) count++;
        if (RemoveMonster(isMonster, flags)) count++;
        if (RemoveMissile(isMissile, flags)) count++;
        return count;
    }

    public static RemovePlan PlanDoRemove(
        bool passesFilter,
        bool isMonster,
        bool isMissile,
        RmvfFlags flags)
    {
        var passes = CountRemovePasses(passesFilter, isMonster, isMissile, flags);
        return passes switch
        {
            0 => RemovePlan.Skip,
            1 => RemovePlan.RemoveOnce,
            _ => RemovePlan.RemoveMany,
        };
    }

    public static bool RemoveChildEligible(int health, bool removeAll) => health <= 0 || removeAll;

    public static bool RemoveSiblingEligible(bool sameMaster, bool notSelf, int health, bool removeAll) =>
        sameMaster && notSelf && (health <= 0 || removeAll);

    public static bool ShouldIterateRemoveChildren(bool masterMatches, int health, bool removeAll) =>
        masterMatches && RemoveChildEligible(health, removeAll);

    public static (object? SourceType, object? DestType) AssignTeleFogTypes(object? sourceType, object? destType) =>
        (sourceType, destType);

    public static (object? SourceType, object? DestType) SwapTeleFogWhenDistinct(
        object? sourceType,
        object? destType)
    {
        if (Equals(sourceType, destType))
            return (sourceType, destType);
        return (destType, sourceType);
    }
}
