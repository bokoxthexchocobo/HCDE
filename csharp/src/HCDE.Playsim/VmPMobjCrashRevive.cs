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
}
