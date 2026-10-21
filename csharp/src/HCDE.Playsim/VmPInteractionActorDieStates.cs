namespace HCDE.Playsim;

/// <summary><c>AActor::Die</c> player death and diestate selection (part 2) from <c>p_interaction.cpp</c> (622–817).</summary>
public static class VmPInteractionActorDieStates
{
    public const int MfUnmorphed = 1;
    public const int MfSolid = 1 << 1;
    public const int Mf4Extremedeath = 1 << 2;
    public const int Mf4Noextremedeath = 1 << 3;
    public const int Mf4Noicedeath = 1 << 4;
    public const int Mf3Ismonster = 1 << 5;
    public const int CfExtremelydead = 1;
    public const int NameExtreme = 1;
    public const int NameIce = 2;
    public const int NameMassacre = 3;
    public const int NameNone = 0;

public static bool ClearSolidOnPlayerDeath(int flags) => (flags & MfSolid) != 0;

public static bool UnmorphDestroyEarly(int flags) => (flags & MfUnmorphed) != 0;

    public static bool ExtremeByGib(bool healthBelowGib, bool inflictorExtreme, bool noExtreme) =>
        (healthBelowGib || inflictorExtreme) && !noExtreme;

    public static bool DamageTypeIsExtreme(int damageType) => damageType == NameExtreme;

    public static bool ClearExtremeDamageType(int damageType) => damageType == NameExtreme;

    public static bool IceGenericFreeze(bool noAutofreeze, int flags4, bool player, bool monster) =>
        !noAutofreeze && (flags4 & Mf4Noicedeath) == 0 && (player || monster);

    public static bool PreserveMassacreType(int damageType) => damageType == NameMassacre;

public static bool NonPlayerExtremeHealthClamp(bool player, int health, int gibHealth) =>
        !player && health >= gibHealth;

    public static int RandomizeDeathTics(int tics, int random3) =>
        tics > 1 ? Math.Max(1, tics - (random3 & 3)) : tics;

public readonly record struct DieStatePick(
        int DamageType,
        bool ExtremelyDead,
        bool TypedExtremeFound,
        bool TypedNormalFound,
        bool IceFreezeFound,
        bool GenericDeathFound);

    public static bool PickTypedExtreme(DieStatePick ctx) =>
        ctx.DamageType != NameNone && ctx.ExtremelyDead && ctx.TypedExtremeFound;

    public static bool PickTypedNormal(DieStatePick ctx) =>
        ctx.DamageType != NameNone && ctx.TypedNormalFound;

    public static bool PickIceFreeze(DieStatePick ctx) =>
        ctx.DamageType == NameIce && ctx.IceFreezeFound;

    public static bool PickGenericExtreme(DieStatePick ctx) => ctx.ExtremelyDead && ctx.GenericDeathFound;

    public static bool PickGenericNormal(DieStatePick ctx) => ctx.GenericDeathFound;

    public static bool BotRespawnScheduled(int botnum, bool demoplayback) => botnum != 0 && !demoplayback;

    public static bool ClearBotEnemy(bool botNull, bool enemyMatch) => !botNull && enemyMatch;

    public static bool AutomapStopOnCameraDeath(bool isCamera, bool automap) => isCamera && automap;

public static bool SkipDamageTypePassThrough(int damageType) => damageType != NameMassacre;

    public static int ClampHealthForExtreme(bool player, int health, int gibHealth, bool extremely) =>
        !player && extremely && health >= gibHealth ? gibHealth - 1 : health;

    public readonly record struct DeathStateResolveContext(
        int DamageType,
        int InflictorFlags4,
        int Health,
        int GibHealth,
        bool NoAutofreeze,
        int Flags4,
        bool Player,
        bool Monster,
        bool ExtremelyDead);

    public static bool ResolveExtremelyDead(DeathStateResolveContext ctx) =>
        ExtremeByGib(ctx.Health < ctx.GibHealth, (ctx.InflictorFlags4 & Mf4Extremedeath) != 0,
            (ctx.InflictorFlags4 & Mf4Noextremedeath) != 0);

    public static int NormalizeDamageTypeForBarrel(DeathStateResolveContext ctx, int current) =>
        SkipDamageTypePassThrough(current) ? NameNone : current;

    public static bool ShouldSetExtremelyDeadFlag(bool player, bool extremely) => player && extremely;

    public static bool ApplyCfExtremelyDead(bool player, bool extremely) => player && extremely;
}
