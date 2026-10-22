namespace HCDE.Playsim;

/// <summary>Whole <c>AActor::Die</c> from <c>p_interaction.cpp</c> (330–796).</summary>
public static class VmPInteractionActorDie
{
    public const int MfUnmorphed = 1;
    public const int MfMissile = 1 << 1;
    public const int MfSpecial = 1 << 2;
    public const int MfSolid = 1 << 3;
    public const int Mf3Ismonster = 1 << 4;
    public const int Mf6Killed = 1 << 5;
    public const int Mf6Dontcorpse = 1 << 6;
    public const int MfCorpse = 1 << 7;
    public const int Mf7Nokillscripts = 1 << 8;
    public const int Mf7Usekillscripts = 1 << 9;
    public const int Mf4Dontfall = 1 << 10;
    public const int MfNogravity = 1 << 11;
    public const int MfDroppoff = 1 << 12;
    public const int Mf4Extremedeath = 1 << 13;
    public const int Mf4Noextremedeath = 1 << 14;
    public const int Mf4Noicedeath = 1 << 15;
    public const int MorphUndobydeathsaves = 1;
    public const int CfExtremelydead = 1;
    public const int NameExtreme = 1;
    public const int NameIce = 2;
    public const int NameMassacre = 3;
    public const int NameNone = 0;

    public sealed class DieActor
    {
        public int Health { get; set; }
        public int GibHealth { get; set; } = 1;
        public int Flags { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags6 { get; set; }
        public int Flags7 { get; set; }
        public int Effects { get; set; }
        public int Special { get; set; }
        public bool HasPlayer { get; set; }
        public bool HasAlternative { get; set; }
        public bool Destroyed { get; set; }
        public int Tics { get; set; } = 5;
        public int DamageType { get; set; }
        public string? DieState { get; set; }
        public bool RaiseStateExists { get; set; }
        public bool IsPlayerPawn { get; set; }
    }

    public sealed class DieContext
    {
        public required DieActor Actor { get; init; }
        public bool WasGibbed { get; init; }
        public bool ForceKillScripts { get; init; }
        public bool CountsAsKill { get; init; } = true;
        public bool NoDeathSpecial { get; init; }
        public int InflictorFlags4 { get; init; }
        public bool SourceNull { get; init; } = true;
        public bool Deathmatch { get; init; }
        public int Maptime { get; init; }
        public bool Multiplayer { get; init; }
        public bool PrimaryLevel { get; init; } = true;
        public int RandomKillMobj { get; init; }
        public Action? MorphedDeath { get; init; }
        public Action? WalkInventoryOwnerDied { get; init; }
        public Action? ExplodeMissile { get; init; }
        public Action? WorldThingDied { get; init; }
        public Action? StartKillScripts { get; init; }
        public Action? PlayerDeathBranch { get; init; }
        public Func<string?>? ResolveDieState { get; init; }
        public Action<string?>? SetState { get; init; }
        public Action? Destroy { get; init; }
    }

    public static bool Die(DieContext ctx)
    {
        var a = ctx.Actor;
        ctx.MorphedDeath?.Invoke();
        a.Effects &= ~1;
        ctx.WalkInventoryOwnerDied?.Invoke();

        if (MissileExplodesOnDeath(a.Flags))
        {
            ctx.ExplodeMissile?.Invoke();
            return true;
        }

        ctx.WorldThingDied?.Invoke();

        if (RunKillScripts(a.HasPlayer, a.Flags7, ctx.ForceKillScripts))
            ctx.StartKillScripts?.Invoke();

        a.Flags &= ~(1 | 2 | 4);
        if (DropGravityUnlessDontFall(a.Flags4))
            a.Flags &= ~MfNogravity;
        a.Flags |= MfDroppoff;

        if (ApplyCorpseFlag((a.Flags3 & Mf3Ismonster) != 0, a.RaiseStateExists, a.IsPlayerPawn, (a.Flags6 & Mf6Dontcorpse) != 0))
            a.Flags |= MfCorpse;

        if (a.HasPlayer)
            ctx.PlayerDeathBranch?.Invoke();

        a.DieState = ctx.ResolveDieState?.Invoke();
        if (a.DieState != null)
        {
            ctx.SetState?.Invoke(a.DieState);
            a.Tics = RandomizeDeathTics(a.Tics, ctx.RandomKillMobj);
        }
        else if (!a.HasPlayer)
        {
            ctx.Destroy?.Invoke();
            a.Destroyed = true;
        }

        return true;
    }

    public static bool RunMorphedDeath(bool hasAlt, bool unmorphedFlag) => hasAlt && !unmorphedFlag;

    public static bool KillDummyMorph(bool realMoKilled, bool stillAlt, bool playerNull, bool undoSaves) =>
        realMoKilled && ((stillAlt && playerNull) || (!stillAlt && !undoSaves));

    public static bool MissileExplodesOnDeath(int flags) => (flags & MfMissile) != 0;

    public static bool RunKillScripts(bool player, int flags7, bool forceKillScripts) =>
        !player && (flags7 & Mf7Nokillscripts) == 0 && (forceKillScripts || (flags7 & Mf7Usekillscripts) != 0);

    public static bool ApplyCorpseFlag(bool monster, bool raiseState, bool playerPawn, bool dontCorpse) =>
        (monster || raiseState || playerPawn) && !dontCorpse;

    public static bool DropGravityUnlessDontFall(int flags4) => (flags4 & Mf4Dontfall) == 0;

    public static bool FraglimitReached(int fraglimit, int frags) => fraglimit > 0 && frags >= fraglimit;

    public static bool ClearSolidOnPlayerDeath(int flags) => (flags & MfSolid) != 0;

    public static bool UnmorphDestroyEarly(int flags) => (flags & MfUnmorphed) != 0;

    public static bool ExtremeByGib(bool healthBelowGib, bool inflictorExtreme, bool noExtreme) =>
        (healthBelowGib || inflictorExtreme) && !noExtreme;

    public static int RandomizeDeathTics(int tics, int random3) =>
        tics > 1 ? Math.Max(1, tics - (random3 & 3)) : tics;

    public static bool IceGenericFreeze(bool noAutofreeze, int flags4, bool player, bool monster) =>
        !noAutofreeze && (flags4 & Mf4Noicedeath) == 0 && (player || monster);

    public readonly record struct FragSpreeContext(
        bool Deathmatch,
        bool SamePlayer,
        bool Teammate,
        int SpreeCount,
        int Multicount,
        int LastKillTime,
        int LevelTime,
        int Ticrate);

    public static int AdjustFragCount(FragSpreeContext ctx, int fragCount)
    {
        if (!ctx.Deathmatch || ctx.SamePlayer || ctx.Teammate)
            return fragCount - 1;
        return fragCount + 1;
    }

    public static bool ResolveExtremelyDead(int health, int gibHealth, int inflictorFlags4) =>
        ExtremeByGib(health < gibHealth, (inflictorFlags4 & Mf4Extremedeath) != 0, (inflictorFlags4 & Mf4Noextremedeath) != 0);

    public static int ClampRealMoHealthForGib(bool wasGibbed, int health, int gibHealth) =>
        wasGibbed && health >= gibHealth ? gibHealth - 1 : health;

    public static bool ClearShootableFloatSkullfly(int flags) => (flags & MfMissile) == 0;

    public static bool RunDeathSpecial(int special, bool specialFlag, bool monster, bool noDeathSpecial) =>
        special != 0 && (!specialFlag || monster) && !noDeathSpecial;

    public static bool FragRequiresMaptime(int maptime) => maptime != 0;

    public static bool SpreeMessageAt(int spree, int threshold) => spree >= threshold;

    public static bool MultikillWindowExpired(int lastKill, int now, int ticrate) => lastKill < now - 3 * ticrate;

    public static bool SinglePlayerKillCredit(bool mp, bool primary, bool countsAsKill) =>
        !mp && primary && countsAsKill;

    public readonly record struct MorphedDeathContext(
        bool HasAlternative,
        bool Unmorphed,
        bool WasGibbed,
        int RealHealth,
        int GibHealth,
        bool RealMoKilled,
        bool StillAlternative,
        bool PlayerNull,
        int MorphStyle);

    public static bool ShouldCallRealDie(MorphedDeathContext ctx) =>
        RunMorphedDeath(ctx.HasAlternative, ctx.Unmorphed) &&
        KillDummyMorph(ctx.RealMoKilled, ctx.StillAlternative, ctx.PlayerNull,
            (ctx.MorphStyle & MorphUndobydeathsaves) != 0);

    public static int AdjustSpreeCount(FragSpreeContext ctx, int spree) =>
        ctx.Teammate ? spree : spree + 1;

    public static bool EmitSpreeBanner(int spree, int threshold) => spree >= threshold;

    public static bool ResetMultikillOnGap(FragSpreeContext ctx) =>
        ctx.LastKillTime > 0 && MultikillWindowExpired(ctx.LastKillTime, ctx.LevelTime, ctx.Ticrate);

    public static int BumpMultikill(int count, bool reset) => reset ? 1 : count + 1;

    public readonly record struct KillCreditContext(
        bool SourcePlayer,
        bool CountsAsKill,
        bool Deathmatch,
        int Maptime,
        bool Multiplayer,
        bool PrimaryLevel);

    public static bool IncrementSourceKillcount(KillCreditContext ctx) =>
        ctx.SourcePlayer && ctx.CountsAsKill;

    public static bool AwardFrag(KillCreditContext ctx, bool hasPlayerVictim) =>
        ctx.SourcePlayer && hasPlayerVictim && FragRequiresMaptime(ctx.Maptime);

    public static int MarkKilledFlags6(int flags6) => flags6 | Mf6Killed;

    public readonly record struct DeathSpecialContext(
        int Special,
        bool MfSpecial,
        bool Monster,
        bool NoDeathSpecial,
        bool CountsAsKill);

    public static bool ActivateDeathSpecial(DeathSpecialContext ctx) =>
        RunDeathSpecial(ctx.Special, ctx.MfSpecial, ctx.Monster, ctx.NoDeathSpecial);

    public static bool IncrementKilledMonsters(DeathSpecialContext ctx) => ctx.CountsAsKill;

    public static bool DfYesLoseFrag(int dmflags2, bool deathmatch) => deathmatch && (dmflags2 & 1) != 0;

    public static bool DuelFragLimitActive(bool deathmatch, int duelLimit, int fraglimit) =>
        deathmatch && (duelLimit > 0 || fraglimit > 0);

    public static int PickActiveFragLimit(int duelLimit, int fraglimit, bool deathmatch) =>
        deathmatch && duelLimit > 0 ? duelLimit : fraglimit;

    public static bool GibHealthClampApplies(bool wasGibbed, int health, int gibHealth) =>
        wasGibbed && health >= gibHealth;

    public static bool DamageTypeIsExtreme(int damageType) => damageType == NameExtreme;

    public static bool PreserveMassacreType(int damageType) => damageType == NameMassacre;

    public static bool NonPlayerExtremeHealthClamp(bool player, int health, int gibHealth) =>
        !player && health >= gibHealth;

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

    public static bool ShouldSetExtremelyDeadFlag(bool player, bool extremely) => player && extremely;

    public static bool ApplyCfExtremelyDead(bool player, bool extremely) => player && extremely;

    public static string? PickDieState(DieStatePick pick)
    {
        if (PickTypedExtreme(pick)) return "Death.Extreme";
        if (PickTypedNormal(pick)) return "Death";
        if (PickIceFreeze(pick)) return "Death.Ice";
        if (PickGenericExtreme(pick)) return "Death.Extreme";
        if (PickGenericNormal(pick)) return "Death";
        return null;
    }

    public static void ApplyPlayerDeathFlags(DieActor a)
    {
        if (ClearSolidOnPlayerDeath(a.Flags))
            a.Flags &= ~MfSolid;
        if (UnmorphDestroyEarly(a.Flags))
            a.Destroyed = true;
    }
}
