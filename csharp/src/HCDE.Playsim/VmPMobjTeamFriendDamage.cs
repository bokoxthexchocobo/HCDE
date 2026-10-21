namespace HCDE.Playsim;

/// <summary>Team, friend/hostile, and special damage from <c>p_mobj.cpp</c> (8364–8606).</summary>
public static class VmPMobjTeamFriendDamage
{
    public const int TeamNone = 0;
    public const int MfFriendly = 1;
    public const int CfGodmode = 1;
    public const int CfGodmode2 = 1 << 1;
    public const int Mf3Ismonster = 1;
    public const int Mf4Noicedeath = 1;
    public const int NameNone = 0;
    public const int NameMassacre = 1;
    public const int NameIce = 2;
    public const int NameDeath = 3;
    public const int NameGenericFreezeDeath = 4;
    public const int SpecialDamageBlocked = -1;

    public static int GetTeamFromPlayer(int playerTeam) => playerTeam;

    public static int GetTeamFromDesignated(int designatedTeam, int friendPlayer, int friendPlayerTeam) =>
        designatedTeam != TeamNone
            ? designatedTeam
            : friendPlayer != 0
                ? friendPlayerTeam
                : TeamNone;

    public static bool IsTeammateNullOther(bool otherNull) => otherNull;

    public static bool CoopPlayerTeammate(bool deathmatch, bool bothPlayers, int flags, int otherFlags) =>
        !deathmatch && bothPlayers && ((flags ^ otherFlags) & MfFriendly) == 0;

    public static bool TeamplayTeammate(bool teamplay, int myTeam, int otherTeam) =>
        teamplay && myTeam != TeamNone && myTeam == otherTeam;

    public readonly record struct TeammateQuery(
        bool OtherNull,
        bool Deathmatch,
        bool Teamplay,
        bool SelfPlayer,
        bool OtherPlayer,
        int Flags,
        int OtherFlags,
        int MyTeam,
        int OtherTeam);

    public static bool IsTeammate(TeammateQuery q)
    {
        if (q.OtherNull)
            return false;
        if (CoopPlayerTeammate(q.Deathmatch, q.SelfPlayer && q.OtherPlayer, q.Flags, q.OtherFlags))
            return true;
        return TeamplayTeammate(q.Teamplay, q.MyTeam, q.OtherTeam);
    }

    public static bool SpeciesUsesExplicit(int species) => species != NameNone;

    public static bool WalkMonsterParentChain(bool parentIsMonster) => parentIsMonster;

    public static int InfightingGroupFromActorInfo(int group) => group;

    public static int ProjectileGroupFromActorInfo(int group) => group;

    public static int SplashGroupFromActorInfo(int group) => group;

    public static bool BothFriendly(int flags, int otherFlags) =>
        (flags & otherFlags & MfFriendly) != 0;

    public static bool FriendDmTeamplay(bool deathmatch, bool teamplay) => deathmatch && teamplay;

    public static bool FriendPlayersLinked(int friendPlayer, int otherFriendPlayer) =>
        friendPlayer != 0 && otherFriendPlayer != 0;

    public readonly record struct FriendQuery(
        int Flags,
        int OtherFlags,
        bool Deathmatch,
        bool Teamplay,
        bool Teammate,
        int FriendPlayer,
        int OtherFriendPlayer,
        bool FriendMoTeammate);

    public static bool IsFriend(FriendQuery q)
    {
        if (!BothFriendly(q.Flags, q.OtherFlags))
            return false;
        if (FriendDmTeamplay(q.Deathmatch, q.Teamplay))
        {
            return q.Teammate || (FriendPlayersLinked(q.FriendPlayer, q.OtherFriendPlayer) && q.FriendMoTeammate);
        }

        return !q.Deathmatch ||
               q.FriendPlayer == q.OtherFriendPlayer ||
               q.FriendPlayer == 0 ||
               q.OtherFriendPlayer == 0 ||
               q.FriendMoTeammate;
    }

    public static bool NeitherFriendly(int flags, int otherFlags) =>
        ((flags | otherFlags) & MfFriendly) == 0;

    public readonly record struct HostileQuery(
        int Flags,
        int OtherFlags,
        bool Deathmatch,
        bool Teamplay,
        bool Teammate,
        int FriendPlayer,
        int OtherFriendPlayer,
        bool FriendMoTeammate);

    public static bool IsHostile(HostileQuery q)
    {
        if (NeitherFriendly(q.Flags, q.OtherFlags))
            return false;
        if (BothFriendly(q.Flags, q.OtherFlags))
        {
            if (FriendDmTeamplay(q.Deathmatch, q.Teamplay))
            {
                return !q.Teammate &&
                       !(FriendPlayersLinked(q.FriendPlayer, q.OtherFriendPlayer) && q.FriendMoTeammate);
            }

            return q.Deathmatch &&
                   q.FriendPlayer != q.OtherFriendPlayer &&
                   q.FriendPlayer != 0 &&
                   q.OtherFriendPlayer != 0 &&
                   !q.FriendMoTeammate;
        }

        return true;
    }

    public static bool GodmodeBlocksSpecial(bool targetIsPlayerMo, int cheats, int damage) =>
        targetIsPlayerMo && damage < 1000 && (cheats & (CfGodmode | CfGodmode2)) != 0;

    public static bool LegacyPoisonHalve(bool hasPlayer, int poisonDamage, int poisonDurationMin) =>
        hasPlayer && poisonDamage > 0 && poisonDurationMin == int.MinValue;

    public static int HalveDamage(int damage) => damage >> 1;

    public readonly record struct DoSpecialDamageContext(
        bool TargetIsPlayerMo,
        int Cheats,
        int Damage,
        bool HasPlayer,
        int PoisonDamage,
        int PoisonDuration);

    public static int DoSpecialDamage(DoSpecialDamageContext ctx)
    {
        if (GodmodeBlocksSpecial(ctx.TargetIsPlayerMo, ctx.Cheats, ctx.Damage))
            return SpecialDamageBlocked;
        if (LegacyPoisonHalve(ctx.HasPlayer, ctx.PoisonDamage, ctx.PoisonDuration))
            return HalveDamage(ctx.Damage);
        return ctx.Damage;
    }

    public static bool UseVirtualDoSpecialDamage(bool hasVirtual) => hasVirtual;

    public static int CallDoSpecialDamageFallback(int damage) => damage;

    public static bool HasGenericDeathState(bool deathStateFound, bool hasSpecialDeathStates) =>
        deathStateFound || !hasSpecialDeathStates;

    public static bool MassacreBypassesDeathCheck(int damageType) => damageType == NameMassacre;

    public static bool TakeFromInflictorDeathType(bool inflictorExists, int inflictorDeathType) =>
        inflictorExists && inflictorDeathType != NameNone;

    public static bool IceFreezeFallback(bool noAutofreeze, int flags4, bool player, bool monster) =>
        !noAutofreeze && (flags4 & Mf4Noicedeath) == 0 && (player || monster);

    public static bool BlockDamageWhenNoDeath(bool deathStateFound) => !deathStateFound;

    public readonly record struct TakeSpecialDamageContext(
        bool DeathStateFound,
        bool HasSpecialDeathStates,
        int DamageType,
        bool InflictorExists,
        int InflictorDeathType,
        int ResolvedType,
        bool TypedDeathStateFound,
        bool IceDeathStateFound,
        bool NoAutofreeze,
        int Flags4,
        bool Player,
        bool Monster);

    public static int TakeSpecialDamageWithAmount(TakeSpecialDamageContext ctx, int damage)
    {
        if (HasGenericDeathState(ctx.DeathStateFound, ctx.HasSpecialDeathStates) ||
            MassacreBypassesDeathCheck(ctx.DamageType))
            return damage;

        var damagetype = TakeFromInflictorDeathType(ctx.InflictorExists, ctx.InflictorDeathType)
            ? ctx.InflictorDeathType
            : ctx.ResolvedType;

        if (damagetype == NameIce)
        {
            var death = ctx.IceDeathStateFound ||
                        IceFreezeFallback(ctx.NoAutofreeze, ctx.Flags4, ctx.Player, ctx.Monster);
            return death ? damage : SpecialDamageBlocked;
        }

        return ctx.TypedDeathStateFound ? damage : SpecialDamageBlocked;
    }

    public static bool UseVirtualTakeSpecialDamage(bool hasVirtual) => hasVirtual;
}
