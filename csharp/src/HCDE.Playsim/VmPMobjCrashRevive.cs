namespace HCDE.Playsim;

/// <summary>Crash, idle, spawn health, raise/revive, gib health, sentience from <c>p_mobj.cpp</c> (8621–8807).</summary>
public static class VmPMobjCrashRevive
{
    public const int MfCorpse = 1;
    public const int MfFriendly = 1 << 1;
    public const int Mf3Crashed = 1 << 2;
    public const int Mf3Ismonster = 1 << 3;
    public const int MfIcecorpse = 1 << 4;
    public const int Mf6Dontcorpse = 1 << 5;
    public const int Mf6Killed = 1 << 6;
    public const int MfNoblockmap = 1 << 7;
    public const int MfNosector = 1 << 8;
    public const int MtfFriendly = 1 << 9;
    public const int NameNone = 0;
    public const int NameCrash = 1;
    public const int NameExtreme = 2;
    public const int NameIdle = 3;
    public const int NameRaise = 4;
    public const int NamePlayerpawn = 5;
    public const int SkillFriendlyHealth = 0;
    public const int SkillMonsterHealth = 1;

    public static bool SkipCrashWhenDontCorpse(int flags6) => (flags6 & Mf6Dontcorpse) != 0;

    public static bool CrashEligible(int flags, int flags6, int flags3, int flags4) =>
        ((flags & MfCorpse) != 0 || (flags6 & Mf6Killed) != 0) &&
        (flags3 & Mf3Crashed) == 0 &&
        (flags & MfIcecorpse) == 0;

    public static bool ExtremeCrashByHealth(int health, int gibHealth) => health < gibHealth;

    public static bool DamageTypeSet(int damageType) => damageType != NameNone;

    public readonly record struct CrashStatePick(
        int DamageType,
        int Health,
        int GibHealth,
        bool ExtremeTypedState,
        bool NormalTypedState,
        bool ExtremeGenericState,
        bool NormalGenericState);

    public static bool PickExtremeTypedCrash(CrashStatePick ctx) =>
        DamageTypeSet(ctx.DamageType) && ExtremeCrashByHealth(ctx.Health, ctx.GibHealth) && ctx.ExtremeTypedState;

    public static bool PickNormalTypedCrash(CrashStatePick ctx) =>
        DamageTypeSet(ctx.DamageType) && ctx.NormalTypedState;

    public static bool PickExtremeGenericCrash(CrashStatePick ctx) =>
        ExtremeCrashByHealth(ctx.Health, ctx.GibHealth) && ctx.ExtremeGenericState;

    public static bool PickNormalGenericCrash(CrashStatePick ctx) => ctx.NormalGenericState;

    public static int MarkCrashedFlags3(int flags3) => flags3 | Mf3Crashed;

    public static int DefaultHealth(int startHealth, int classHealth) =>
        startHealth != 0 ? startHealth : classHealth;

    public static bool NonMonsterHealth(int flags3, int defHealth) =>
        (flags3 & Mf3Ismonster) == 0 || defHealth == 0;

    public static int ScaleHealth(int defHealth, double skillFactor) =>
        Math.Max(1, (int)(defHealth * skillFactor));

    public readonly record struct SpawnHealthContext(
        int StartHealth,
        int ClassHealth,
        int Flags,
        int Flags3,
        double FriendlySkill,
        double MonsterSkill);

    public static int SpawnHealth(SpawnHealthContext ctx)
    {
        var def = DefaultHealth(ctx.StartHealth, ctx.ClassHealth);
        if (NonMonsterHealth(ctx.Flags3, def))
            return def;
        if ((ctx.Flags & MfFriendly) != 0)
            return ScaleHealth(def, ctx.FriendlySkill);
        return ScaleHealth(def, ctx.MonsterSkill);
    }

    public static bool RaiseRequiresCorpse(int flags) => (flags & MfCorpse) != 0;

    public static bool RaiseBlockedByTics(bool ticsNotMinusOne, bool canRaise) =>
        ticsNotMinusOne && !canRaise;

    public static bool ReviveUnlinkNeeded(int flags, int defaultFlags) =>
        (flags & (MfNoblockmap | MfNosector)) != (defaultFlags & (MfNoblockmap | MfNosector));

    public static bool ReviveFriendlySpawnFlag(int spawnFlags) => (spawnFlags & MtfFriendly) != 0;

    public static int GibHealthDefault(int spawnHealth) => -spawnHealth;

    public static bool IsSentient(int health, bool hasSeeState) => health > 0 && hasSeeState;

    public enum CrashStateChoice
    {
        None,
        ExtremeTyped,
        NormalTyped,
        ExtremeGeneric,
        NormalGeneric,
    }

    public static CrashStateChoice SelectCrashState(CrashStatePick ctx)
    {
        if (PickExtremeTypedCrash(ctx)) return CrashStateChoice.ExtremeTyped;
        if (PickNormalTypedCrash(ctx)) return CrashStateChoice.NormalTyped;
        if (PickExtremeGenericCrash(ctx)) return CrashStateChoice.ExtremeGeneric;
        if (PickNormalGenericCrash(ctx)) return CrashStateChoice.NormalGeneric;
        return CrashStateChoice.None;
    }

    public static bool RunCrash(
        int flags6,
        int flags,
        int flags3,
        int flags4,
        CrashStatePick statePick,
        out CrashStateChoice choice,
        out int updatedFlags3)
    {
        choice = CrashStateChoice.None;
        updatedFlags3 = flags3;
        if (SkipCrashWhenDontCorpse(flags6))
            return false;
        if (!CrashEligible(flags, flags6, flags3, flags4))
            return false;
        choice = SelectCrashState(statePick);
        updatedFlags3 = MarkCrashedFlags3(flags3);
        return true;
    }

    public enum IdleStateSource
    {
        NamedIdle,
        SpawnStateFallback,
    }

    public static IdleStateSource ResolveSetIdleSource(bool idleFound) =>
        idleFound ? IdleStateSource.NamedIdle : IdleStateSource.SpawnStateFallback;

    public static bool TryGetRaiseState(
        int flags,
        bool ticsNotMinusOne,
        bool stateCanRaise,
        bool isPlayerPawn,
        bool raiseStateFound,
        out bool allowed)
    {
        allowed = false;
        if (!RaiseRequiresCorpse(flags))
            return false;
        if (RaiseBlockedByTics(ticsNotMinusOne, stateCanRaise))
            return false;
        if (RaiseBlockedByPlayerPawn(isPlayerPawn))
            return false;
        allowed = raiseStateFound;
        return allowed;
    }

    public sealed class ReviveActorScratch
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags5 { get; set; }
        public int Flags6 { get; set; }
        public int Flags7 { get; set; }
        public int Flags8 { get; set; }
        public int DamageType { get; set; }
        public int Health { get; set; }
        public int PoisonDamage { get; set; }
        public int PoisonType { get; set; }
        public int PoisonDuration { get; set; }
        public int PoisonPeriod { get; set; }
        public bool PlayerPoisonCount { get; set; }
    }

    public static void ApplyReviveFlagDefaults(ReviveActorScratch scratch, int defaultFlags, int defaultFlags2, int defaultFlags3, int defaultFlags4, int defaultFlags5, int defaultFlags6, int defaultFlags7, int defaultFlags8, int defaultDamageType)
    {
        scratch.Flags = defaultFlags;
        scratch.Flags2 = defaultFlags2;
        scratch.Flags3 = defaultFlags3;
        scratch.Flags4 = defaultFlags4;
        scratch.Flags5 = defaultFlags5;
        scratch.Flags6 = defaultFlags6;
        scratch.Flags7 = defaultFlags7;
        scratch.Flags8 = defaultFlags8;
        scratch.DamageType = defaultDamageType;
    }

    public static void ApplyReviveFriendlyFromSpawn(ReviveActorScratch scratch, int spawnFlags)
    {
        if (ReviveFriendlySpawnFlag(spawnFlags))
            scratch.Flags |= MfFriendly;
    }

    public static void ClearRevivePoison(ReviveActorScratch scratch)
    {
        scratch.PoisonDamage = 0;
        scratch.PoisonType = NameNone;
        scratch.PoisonDuration = 0;
        scratch.PoisonPeriod = 0;
        scratch.PlayerPoisonCount = false;
    }

    public static void RunRevive(
        ReviveActorScratch scratch,
        int liveFlags,
        int defaultFlags,
        int spawnFlags,
        int defaultFlags2,
        int defaultFlags3,
        int defaultFlags4,
        int defaultFlags5,
        int defaultFlags6,
        int defaultFlags7,
        int defaultFlags8,
        int defaultDamageType,
        int spawnHealth,
        out bool needsUnlink)
    {
        needsUnlink = ReviveUnlinkNeeded(liveFlags, defaultFlags);
        ApplyReviveFlagDefaults(scratch, defaultFlags, defaultFlags2, defaultFlags3, defaultFlags4, defaultFlags5, defaultFlags6, defaultFlags7, defaultFlags8, defaultDamageType);
        ApplyReviveFriendlyFromSpawn(scratch, spawnFlags);
        scratch.Health = spawnHealth;
        ClearRevivePoison(scratch);
    }

    public static int ResolveGibHealth(bool virtualOverride, int virtualValue, int spawnHealth) =>
        virtualOverride ? virtualValue : GibHealthDefault(spawnHealth);

    public static bool CountsAsKillForRevive(bool countsAsKill) => countsAsKill;
}
