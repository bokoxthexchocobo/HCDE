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

        if (!ctx.SourceNull)
            _ = ctx.SourceNull;

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
}
