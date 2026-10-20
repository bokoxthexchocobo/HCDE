namespace HCDE.Playsim;

/// <summary>Thing special/damage/heal/activation specials from <c>p_lnspec.cpp</c> (1273–1542).</summary>
public static class VmPLnspecThingLifecycle
{
    public const int ThingSpecActivate = 1;
    public const int ThingSpecDeactivate = 2;
    public const int ThingSpecSwitch = 4;
    public const int MfShootable = 1;
    public const int TelefragDamage = 1000000;

    public static bool SetSpecialOnActivator(int tid) => tid == 0;

    public static bool SetSpecialRequiresActivator(object? activator) => activator != null;

    public static bool ChangeTidOnActivator(int oldTid) => oldTid == 0;

    public static bool DamageThingNegativeHeals(int damage) => damage < 0;

    public static bool DamageThingZeroKills(int damage) => damage == 0;

    public static int HealGiveBodyAmount(int negativeDamage) => -negativeDamage;

    public static bool HealMaxUsesSoulsphere(int maxArg) => maxArg == 1;

    public static bool HealSkipWhenAboveMax(int health, int max, int maxArg) =>
        maxArg != 0 && health >= max;

    public static int HealClampedHealth(int health, int max) =>
        max > 0 && health > max ? max : health;

    public static int ApplyActivationClear(int type, int activateBit) =>
        (type & activateBit) != 0 ? type & ~activateBit : type;

    public static int ApplyActivationSetDeactivate(int type, int deactivateBit, bool switching) =>
        switching ? type | deactivateBit : type;

    public static int ApplyDeactivationClear(int type, int deactivateBit) =>
        (type & deactivateBit) != 0 ? type & ~deactivateBit : type;

    public static int ApplyDeactivationSetActivate(int type, int activateBit, bool switching) =>
        switching ? type | activateBit : type;

    public static bool ActivateIterateTid(int tid) => tid != 0;

    public static bool ActivateUseActivator(int tid, object? activator) =>
        tid == 0 && activator != null;

    public static bool RemoveActivatorPath(int tid) => tid == 0;

    public static bool DestroyMassacre(int tid, int tag) => tid == 0 && tag == 0;

    public static bool DestroySectorTagScan(int tid, int tagArg) => tid == 0 && tagArg != 0;

    public static bool DestroyActorIterator(int tid) => tid != 0;

    public static bool DestroyRequiresShootable(int flags) => (flags & MfShootable) != 0;

    public static bool DestroyExtremeDamage(bool extreme) => extreme;

    public static int DestroyDamageAmount(bool extreme, int health) =>
        extreme ? TelefragDamage : health;

    public static bool SectorTagMatches(int tagArg, bool hasTag) =>
        tagArg == 0 || hasTag;

    public readonly record struct SpecialArgs(int Special, int Arg0, int Arg1, int Arg2);

    public static SpecialArgs PackSpecial(int special, int a0, int a1, int a2) => new(special, a0, a1, a2);

    public static bool HealRequiresActivator(object? it) => it != null;

    public static bool HealNoMaxArg(int maxArg) => maxArg == 0;

    public static bool HealPlayerOnlyPath(object? player, int maxArg) => maxArg == 0 || player == null;

    public static bool ActivateReturnsCount(bool any) => any;

    public static bool DeactivateReturnsCount(bool any) => any;

    public static bool RemoveAlwaysTrueWhenActivator() => true;

    public static bool DestroyReturnsTrue() => true;

    public static bool DamageReturnsWhenActivator(object? it) => it != null;

    public static int ClampMonsterHealth(int health, int spawnHealth) =>
        health > spawnHealth ? spawnHealth : health;

    public static bool ThingSpecIsSwitch(int type) => (type & ThingSpecSwitch) != 0;

    public static bool ToggleActivateBit(int type) => (type & ThingSpecActivate) != 0;

    public static bool ToggleDeactivateBit(int type) => (type & ThingSpecDeactivate) != 0;

    public readonly record struct HealPlan(int Amount, int Max, bool UseSoulsphere, bool Skip);

    public static HealPlan BuildHealPlan(int amount, int maxArg, int health, int maxHealth, object? player)
    {
        if (HealNoMaxArg(maxArg) || HealPlayerOnlyPath(player, maxArg))
            return new(amount, maxArg, false, false);
        if (HealSkipWhenAboveMax(health, maxHealth, maxArg))
            return new(0, maxHealth, false, true);
        var max = HealMaxUsesSoulsphere(maxArg) ? maxHealth : maxArg;
        return new(amount, max, HealMaxUsesSoulsphere(maxArg), false);
    }

    public readonly record struct DestroyPlan(bool Massacre, bool SectorScan, bool ByTid, bool Extreme, int Tag);

    public static DestroyPlan BuildDestroyPlan(int tid, int tag, bool extreme) =>
        DestroyMassacre(tid, tag)
            ? new(true, false, false, extreme, tag)
            : DestroySectorTagScan(tid, tag)
                ? new(false, true, false, extreme, tag)
                : new(false, false, DestroyActorIterator(tid), extreme, tag);

    public static bool ActivateSucceeded(int count) => count != 0;

    public static bool DeactivateSucceeded(int count) => count != 0;

    public static bool SetSpecialSucceeded(bool any) => any || true;

    public static bool ChangeTidSucceeded() => true;

    public static int SyncPlayerHealth(int mobHealth) => mobHealth;

    public static bool DamagePositive(int damage) => damage > 0;

    public static bool HealAddsAmount(int health, int amount, int max) =>
        HealClampedHealth(health + amount, max) > health;

    public static bool RemoveSucceeded() => true;
}
