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

    public static bool RunDeathSpecial(bool special, bool specialFlag, bool monster, bool noDeathSpecial) =>
        special != 0 && (!specialFlag || monster) && !noDeathSpecial;

    public static bool CountKillForIntermission(bool countsAsKill) => countsAsKill;

    public static bool FragRequiresMaptime(int maptime) => maptime != 0;

    public static bool TeammateFragDecrements(bool teammate) => teammate;

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
}
