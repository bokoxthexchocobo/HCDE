namespace HCDE.Playsim;

/// <summary><c>AActor::Die</c> morph, missile, flags, and kill credit (part 1) from <c>p_interaction.cpp</c> (329–620).</summary>
public static class VmPInteractionActorDieMorph
{
    public const int MfUnmorphed = 1;
    public const int MfMissile = 1 << 1;
    public const int MfSpecial = 1 << 2;
    public const int Mf3Ismonster = 1 << 3;
    public const int Mf6Killed = 1 << 4;
    public const int Mf6Dontcorpse = 1 << 5;
    public const int MfCorpse = 1 << 6;
    public const int Mf7Nokillscripts = 1 << 7;
    public const int Mf7Usekillscripts = 1 << 8;
    public const int Mf4Dontfall = 1 << 9;
    public const int MfNogravity = 1 << 10;
    public const int MfDroppoff = 1 << 11;
    public const int MorphUndobydeathsaves = 1;

    public static bool RunMorphedDeath(bool hasAlt, bool unmorphedFlag) => hasAlt && !unmorphedFlag;

    public static bool KillDummyMorph(bool realMoKilled, bool stillAlt, bool playerNull, bool undoSaves) =>
        realMoKilled && ((stillAlt && playerNull) || (!stillAlt && !undoSaves));

    public static int ClampRealMoHealthForGib(bool wasGibbed, int health, int gibHealth) =>
        wasGibbed && health >= gibHealth ? gibHealth - 1 : health;

    public static bool MissileExplodesOnDeath(int flags) => (flags & MfMissile) != 0;

    public static bool RunKillScripts(bool player, int flags7, bool forceKillScripts) =>
        !player && (flags7 & Mf7Nokillscripts) == 0 && (forceKillScripts || (flags7 & Mf7Usekillscripts) != 0);

    public static bool ClearShootableFloatSkullfly(int flags) => (flags & MfMissile) == 0;

    public static bool ApplyCorpseFlag(bool monster, bool raiseState, bool playerPawn, bool dontCorpse) =>
        (monster || raiseState || playerPawn) && !dontCorpse;

    public static bool RunDeathSpecial(int special, bool specialFlag, bool monster, bool noDeathSpecial) =>
        special != 0 && (!specialFlag || monster) && !noDeathSpecial;

    public static bool FragRequiresMaptime(int maptime) => maptime != 0;

    public static bool SpreeMessageAt(int spree, int threshold) => spree >= threshold;

    public static bool MultikillWindowExpired(int lastKill, int now, int ticrate) => lastKill < now - 3 * ticrate;

    public static bool FraglimitReached(int fraglimit, int frags) => fraglimit > 0 && frags >= fraglimit;

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
        if (!ctx.Deathmatch || ctx.SamePlayer)
            return fragCount - 1;
        if (ctx.Teammate)
            return fragCount - 1;
        return fragCount + 1;
    }

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

    public static bool DropGravityUnlessDontFall(int flags4) => (flags4 & Mf4Dontfall) == 0;

    public static int MarkKilledFlags6(int flags6) => flags6 | Mf6Killed;

    public static bool SetTargetFromSource(bool sourceNull) => !sourceNull;

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
}
