namespace HCDE.Playsim;

/// <summary><c>DoCheckClass</c>, <c>DoCheckSpecies</c>, <c>DoDamage</c>, <c>A_SetTics</c>, and <c>A_Damage*</c> bands from <c>p_actionfunctions.cpp</c> (3751–4175).</summary>
public static class VmActorDoDamage
{
    [Flags]
    public enum DmssFlags
    {
        FoilInvul = 1,
        AffectArmor = 2,
        Kill = 4,
        NoFactor = 8,
        FoilBuddha = 16,
        NoProtect = 32,
        ExFilter = 64,
        ExSpecies = 128,
        Either = 256,
        InflictorDmgType = 512,
    }

    public static bool CheckSpecies(string? actorSpecies, string? filterSpecies, bool exclude)
    {
        if (string.IsNullOrEmpty(filterSpecies))
            return true;
        return exclude ? actorSpecies != filterSpecies : actorSpecies == filterSpecies;
    }

    public static bool CheckClass(string? actorClass, string? filterClass, bool exclude)
    {
        if (string.IsNullOrEmpty(filterClass))
            return true;
        return exclude ? actorClass != filterClass : actorClass == filterClass;
    }

    public static bool PassesFilterSpecies(bool filterPass, bool speciesPass, DmssFlags flags) =>
        (flags & DmssFlags.Either) != 0 ? filterPass || speciesPass : filterPass && speciesPass;

    public static int AssembleDamageFlags(DmssFlags flags)
    {
        var dmg = 0;
        if ((flags & DmssFlags.FoilInvul) != 0) dmg |= 1;
        if ((flags & DmssFlags.FoilBuddha) != 0) dmg |= 2;
        if ((flags & (DmssFlags.Kill | DmssFlags.NoFactor)) != 0) dmg |= 4;
        if ((flags & DmssFlags.AffectArmor) == 0 || (flags & DmssFlags.Kill) != 0) dmg |= 8;
        if ((flags & DmssFlags.Kill) != 0) dmg |= 4;
        if ((flags & DmssFlags.NoProtect) != 0) dmg |= 16;
        return dmg;
    }

    public static int AdjustKillAmount(int amount, int health, DmssFlags flags) =>
        (flags & DmssFlags.Kill) != 0 ? amount + health : amount;

    public enum DamagePlan
    {
        Skip,
        Damage,
        Heal,
    }

    public static DamagePlan PlanDoDamage(int amount, bool passesFilter) =>
        !passesFilter ? DamagePlan.Skip : amount > 0 ? DamagePlan.Damage : amount < 0 ? DamagePlan.Heal : DamagePlan.Skip;

    public static int HealMagnitude(int amount) => -amount;

    public static bool UseInflictorDamageType(bool hasInflictor, DmssFlags flags) =>
        hasInflictor && (flags & DmssFlags.InflictorDmgType) != 0;

    public static bool SetTicsOnPsprite(bool fromPsprite, bool pspriteFound) =>
        fromPsprite && pspriteFound;

    public static bool ShouldSetActorTics(bool fromActor) => fromActor;

    public static bool ShouldDamageWhenPresent(object? victim) => victim is not null;

    public static bool PassesDoDamageFilter(
        bool filterPass,
        bool speciesPass,
        DmssFlags flags) =>
        PassesFilterSpecies(filterPass, speciesPass, flags);

    public static bool ShouldApplyDamageBranch(int amount, DamagePlan plan) =>
        plan == DamagePlan.Damage && amount > 0;

    public static bool ShouldApplyHealBranch(int amount, DamagePlan plan) =>
        plan == DamagePlan.Heal && amount < 0;
}
