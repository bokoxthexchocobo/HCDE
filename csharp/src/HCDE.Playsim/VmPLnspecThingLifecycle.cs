namespace HCDE.Playsim;

/// <summary>Thing special/damage/heal/activation specials from <c>p_lnspec.cpp</c> (1273–1542).</summary>
public static class VmPLnspecThingLifecycle
{
    public const int ThingSpecActivate = 1;
    public const int ThingSpecDeactivate = 2;
    public const int ThingSpecSwitch = 4;
    public const int MfShootable = 1;
    public const int TelefragDamage = 1000000;

    public readonly record struct ThingSpecialWrite(int Special, int A0, int A1, int A2);

    public static ThingSpecialWrite PackSpecial(int special, int a0, int a1, int a2) => new(special, a0, a1, a2);

    public static bool ApplySpecialToActivator(int tid, object? activator) => tid == 0 && activator != null;

    public static bool ApplySpecialToIterator(int tid) => tid != 0;

    public static bool ChangeTidOnActivator(int oldTid) => oldTid == 0;

    public static int ChangeTidIteratorCount(int count) => count;

    public readonly record struct DamageThingResult(bool HadActivator, bool HealPlayer, bool HealMonster, bool Damage, bool Telefrag);

    public static DamageThingResult EvaluateDamage(int damage, bool hasPlayer)
    {
        if (damage < 0)
            return new(true, hasPlayer, !hasPlayer, false, false);
        if (damage == 0)
            return new(true, false, false, false, true);
        return new(true, false, false, true, false);
    }

    public static int NegativeDamageHealAmount(int damage) => -damage;

    public static int ClampHealToSpawn(int health, int spawnHealth) =>
        health > spawnHealth ? spawnHealth : health;

    public readonly record struct HealOutcome(int NewHealth, bool SyncPlayer, bool Skipped);

    public static HealOutcome ApplyHeal(int health, int amount, int maxArg, int maxSoulsphere, int dehMaxSoulsphere, object? player)
    {
        if (maxArg == 0 || player == null)
            return new(health, false, false);
        var max = maxArg == 1 ? dehMaxSoulsphere : maxArg;
        if (maxArg != 0 && health >= max)
            return new(health, false, true);
        var nh = health + amount;
        if (nh > max && max > 0)
            nh = max;
        return new(nh, player != null, false);
    }

    public static int ToggleActivationType(int type, bool isActivate)
    {
        if (isActivate)
        {
            if ((type & ThingSpecActivate) == 0)
                return type;
            type &= ~ThingSpecActivate;
            if ((type & ThingSpecSwitch) != 0)
                type |= ThingSpecDeactivate;
            return type;
        }
        if ((type & ThingSpecDeactivate) == 0)
            return type;
        type &= ~ThingSpecDeactivate;
        if ((type & ThingSpecSwitch) != 0)
            type |= ThingSpecActivate;
        return type;
    }

    public readonly record struct ActivateLoopResult(int Count, bool UsedActivator);

    public static ActivateLoopResult RunActivate(int tid, int iteratorHits, bool hasActivator) =>
        tid != 0
            ? new(iteratorHits, false)
            : hasActivator ? new(1, true) : new(0, false);

    public static bool ActivateSucceeded(ActivateLoopResult r) => r.Count != 0;

    public static bool DeactivateSucceeded(ActivateLoopResult r) => r.Count != 0;

    public static bool RemoveByActivator(int tid) => tid == 0;

    public readonly record struct DestroyMode(bool Massacre, bool SectorTagScan, bool ByTid);

    public static DestroyMode ResolveDestroyMode(int tid, int tag) =>
        tid == 0 && tag == 0
            ? new(true, false, false)
            : tid == 0
                ? new(false, true, false)
                : new(false, false, true);

    public static bool DestroyShouldDamage(bool shootable, bool sectorTagOk, int tagArg, bool hasTag) =>
        shootable && (tagArg == 0 || hasTag);

    public static int DestroyDamage(bool extreme, int health) => extreme ? TelefragDamage : health;

    public static bool DestroyReturnsTrue() => true;

    public static bool DamageReturns(bool hadActivator) => hadActivator;

    public static bool HealReturns(bool hadActivator) => hadActivator;

    public static int IteratorAdvanceCount(IEnumerable<object?> actors) =>
        actors.Count();

    public static bool ThingActivatePrefetchNext(bool hasNext) => hasNext;

    public static ThingSpecialWrite MergeSpecialOntoActor(ThingSpecialWrite write, int existingSpecial) =>
        write.Special != 0 ? write : new(existingSpecial, write.A0, write.A1, write.A2);

    public static bool SetSpecialReturnsTrue() => true;

    public static bool ChangeTidReturnsTrue() => true;

    public static int MaxSoulsphereFromDeh(int maxArg, int dehMax) => maxArg == 1 ? dehMax : maxArg;

    public static bool HealSkipAboveMax(int health, int max, int maxArg) =>
        maxArg != 0 && max > 0 && health >= max;

    public static int HealAfterAdd(int health, int amount) => health + amount;

    public static bool RemoveReturnsTrue() => true;

    public static bool MassacreWhenZeroTidZeroTag(int tid, int tag) => tid == 0 && tag == 0;

    public static bool SectorScanWhenZeroTidNonzeroTag(int tid, int tag) => tid == 0 && tag != 0;

    public static bool ByTidWhenNonzero(int tid) => tid != 0;

    public static bool ExtremeDamageArg(bool arg1) => arg1;

    public static bool SyncPlayerHealthAfterHeal(bool hasPlayer) => hasPlayer;

    public static IEnumerable<object?> ActivateIteratorWalk(IEnumerable<object?> actors)
    {
        foreach (var a in actors)
            yield return a;
    }

    public static int CountActivateHits(IEnumerable<object?> actors, int prefetch)
    {
        var count = 0;
        foreach (var _ in ActivateIteratorWalk(actors))
            count++;
        return count + prefetch;
    }

    public static bool DestroySectorIteratorShouldDamage(bool shootable, bool tagMatch) =>
        DestroyShouldDamage(shootable, tagMatch, 1, tagMatch);

    public static int DestroyByTidHits(int hits) => hits;

    public static bool GiveBodyWhenPlayer(bool hasPlayer, int negativeDamage) =>
        hasPlayer && negativeDamage < 0;

    public static bool MonsterHealSubtract(int health, int negativeDamage) => health - negativeDamage > health;

    public static bool TelefragWhenZeroDamage(DamageThingResult r) => r.Telefrag;

    public static bool PositiveDamage(DamageThingResult r) => r.Damage;

    public static ActivateLoopResult DeactivateLoop(int tid, int hits, bool activator) =>
        RunActivate(tid, hits, activator);

    public static bool ThingSpecHasSwitch(int type) => (type & ThingSpecSwitch) != 0;

    public static int ClearActivateBit(int type) => type & ~ThingSpecActivate;

    public static int ClearDeactivateBit(int type) => type & ~ThingSpecDeactivate;

    public static int SetDeactivateBit(int type) => type | ThingSpecDeactivate;

    public static int SetActivateBit(int type) => type | ThingSpecActivate;

    public static bool ActivateWhileHasNext(bool hasNext) => hasNext;

    public static object? ActivatePrefetch(object? current, object? next) => next;

    public static bool DeactivateWhileHasNext(bool hasNext) => hasNext;

    public static bool RemoveThingDirect(object? it) => it != null;

    public static bool DestroyMassacreBranch(DestroyMode mode) => mode.Massacre;

    public static bool DestroySectorBranch(DestroyMode mode) => mode.SectorTagScan;

    public static bool DestroyTidBranch(DestroyMode mode) => mode.ByTid;

    public static bool DamageUsesMod(int mod) => mod >= 0;

    public static bool HealUsesMaxArg(int maxArg) => maxArg >= 0;

    public static int HealAmountArg(int amount) => amount;

    public static bool SetSpecialArg2(int a2) => true;

    public static bool SetSpecialArg3(int a3) => true;

    public static bool SetSpecialArg4(int a4) => true;

    public static int ChangeTidNewTid(int tid) => tid;

    public static bool IteratorHasMore(bool more) => more;

    public static bool ActivatorPathValid(object? a) => a != null;

    public static bool DeactivateActivatorPath(int tid, object? a) => tid == 0 && a != null;

    public static int ActivateIncrement(int c) => c + 1;

    public static bool DestroyTagZeroMeansAll(int tag) => tag == 0;

    public static bool DestroyExtremeArg(int arg1) => arg1 != 0;

    public static bool HealPlayerHealthSync(bool player) => player;

    public static bool DamageActivatorNull(object? it) => it == null;

    public static bool HealActivatorNull(object? it) => it == null;

    public static bool ActivateCountNonZero(int count) => count != 0;

    public static bool DeactivateCountNonZero(int count) => count != 0;

    public static int SpecialArg0(int v) => v;

    public static int SpecialArg1(int v) => v;

    public static int SpecialArg2(int v) => v;

    public static bool MassacreNoTagBranch(int tid, int tag) => tid == 0 && tag == 0;

    public static bool DamageMobjPositive(int dmg) => dmg > 0;

    public static bool GiveBodyPath(bool player, int dmg) => player && dmg < 0;

    public static int SubtractMonsterHealth(int h, int dmg) => h - dmg;

    public static bool SpawnHealthCap(int h, int spawn) => h <= spawn;

    public static bool ThingDestroyOnlyMassacreInChunk(DestroyMode m) => m.Massacre;

    public static bool ActivateTidBranch(int tid) => tid != 0;

    public static bool DeactivateTidBranch(int tid) => tid != 0;

    public static bool RemoveTidBranch(int tid) => tid != 0;

    public static bool ChangeTidSetOnActivator(int oldTid) => oldTid == 0;

    public static bool ChangeTidSetOnIterator(int oldTid) => oldTid != 0;
}
