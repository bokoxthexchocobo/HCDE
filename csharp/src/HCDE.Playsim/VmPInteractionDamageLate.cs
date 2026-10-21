namespace HCDE.Playsim;

/// <summary><c>DamageMobj</c> team thrust, player health, drain, and death tail from <c>p_interaction.cpp</c> (1320–1571).</summary>
public static class VmPInteractionDamageLate
{
    public const int DmgForced = 1;
    public const int DmgNoArmor = 1;
    public const int Mf5Dontdrain = 1;
    public const int Mf7Buddha = 1;
    public const int Mf7Foilbuddha = 1;
    public const int Mf7Laxtelefragdmg = 1;
    public const int DmgFoilbuddha = 1;
    public const int Mf5Specialfiredamage = 1;
    public const int Mf5Summonedmonster = 1;
    public const int SecfEndlevel = 1;

    public static bool TeamDamageScales(bool forced, bool hasSource, bool teammate) =>
        !forced && hasSource && teammate;

    public static bool TelefragAllowsTeamScale(bool telefrag, int flags7) =>
        !telefrag || (flags7 & Mf7Laxtelefragdmg) != 0;

    public static bool ZeroTeamDamageStops(int damage) => damage <= 0;

    public static bool ClearsForcedOnGod2(bool god2, bool buddha2, bool nodamage) =>
        god2 || buddha2 || nodamage;

    public static bool EndLevelHellHack(bool endSector, int damage, int health) =>
        endSector && damage >= health;

    public static bool PlayerInvulnBlocks(bool telefrag, bool invuln, bool god, bool god2, bool nodamage) =>
        god2 || nodamage || (!telefrag && (invuln || god));

    public static bool ArmorAbsorbPath(bool noArmor, bool hasInventory, int damage) =>
        !noArmor && hasInventory && damage > 0;

    public static bool TelefragPreservesRaw(bool telefrag, int flags7) =>
        telefrag && (flags7 & Mf7Laxtelefragdmg) == 0;

    public static bool AutoUseHealth(bool damageGteHealth, bool autoUse, bool dm, bool altNull) =>
        damageGteHealth && (autoUse || dm) && altNull;

    public static bool VoodooZombieCompat(bool compat) => compat;

    public static bool MirrorHealthToMo(bool compat) => !compat;

    public static bool StrifeHealthAuto(bool low, bool dm, bool forced) => low && !dm && !forced;

    public static bool BuddhaSavePlayer(int buddha, bool forced, bool telefrag) =>
        buddha != 0 && !forced && !(telefrag && buddha == 1);

    public static bool DrainEligible(bool hasSourcePlayer, bool dontDrain, bool samePlayer) =>
        hasSourcePlayer && !dontDrain && !samePlayer;

    public static bool PickDrainStrength(double candidate, double best) => candidate > best;

    public static bool DrainPositive(double strength) => strength > 0;

    public static bool OnDrainVirtual(bool hasVirtual) => hasVirtual;

    public static bool GiveBodySucceeded(bool gave) => gave;

    public static bool MonsterBuddha(bool forced, bool buddhaFlag, bool telefrag, bool foil, bool inflictorFoil) =>
        !forced && buddhaFlag && !telefrag && !foil && !inflictorFoil;

    public static bool DeathUsesInflictorType(bool hasInflictor, bool deathTypeSet) =>
        hasInflictor && deathTypeSet;

    public static bool FlameDeathPlayer(bool fire, bool player, bool altNull, int health, int damage, bool specialFire) =>
        fire && player && altNull && (health > -50 && damage > 25 || !specialFire);

    public static bool MinotaurKillCredit(bool summoned, bool tracerPlayer, bool tracerIsMo) =>
        summoned && tracerPlayer && tracerIsMo;

    public static bool EmitWorldDamaged(bool needEvent) => needEvent;

    public static int ReturnNonNegative(int damage) => damage < 0 ? -1 : Math.Max(0, damage);

    public readonly record struct TeamDamageContext(
        bool Forced,
        bool HasSource,
        bool Teammate,
        bool Telefrag,
        int Flags7,
        int ScaledDamage);

    public static bool EvaluateTeamDamage(TeamDamageContext ctx)
    {
        if (!TeamDamageScales(ctx.Forced, ctx.HasSource, ctx.Teammate)) return false;
        if (!TelefragAllowsTeamScale(ctx.Telefrag, ctx.Flags7)) return false;
        return !ZeroTeamDamageStops(ctx.ScaledDamage);
    }

    public readonly record struct PlayerDamageContext(
        bool Telefrag,
        bool Invuln,
        bool God,
        bool God2,
        bool Nodamage,
        bool NoArmor,
        bool HasInventory,
        int Damage,
        bool AutoUse,
        bool Deathmatch,
        bool AltNull,
        int Health);

    public static bool EvaluatePlayerSurvivesInvuln(PlayerDamageContext ctx) =>
        !PlayerInvulnBlocks(ctx.Telefrag, ctx.Invuln, ctx.God, ctx.God2, ctx.Nodamage);

    public static bool EvaluatePlayerArmor(PlayerDamageContext ctx) =>
        ArmorAbsorbPath(ctx.NoArmor, ctx.HasInventory, ctx.Damage);

    public readonly record struct DeathTailContext(
        int Health,
        bool Forced,
        bool Buddha,
        bool Telefrag,
        bool FoilBuddha,
        bool InflictorFoilBuddha,
        bool FireMod,
        bool Player,
        bool AltNull);

    public static bool EvaluateMonsterBuddha(DeathTailContext ctx) =>
        MonsterBuddha(ctx.Forced, ctx.Buddha, ctx.Telefrag, ctx.FoilBuddha, ctx.InflictorFoilBuddha);

    public static bool EvaluateDeathProceeds(DeathTailContext ctx) =>
        ctx.Health <= 0 && !EvaluateMonsterBuddha(ctx);

    public static int ScaleTeamDamage(int damage, double teamDamage) =>
        (int)(damage * teamDamage);

    public static int ClampEndLevelDamage(int damage, int health) =>
        damage >= health ? health - 1 : damage;

    public static int ClampDamageCount(int count) => count > 100 ? 100 : count;

    public static int TactilePulseDamage(int damage) => damage < 100 ? damage : 100;

    public static bool MonsterArmorEligible(int flags, bool hasInventory, int damage) =>
        (flags & (DmgNoArmor | DmgForced)) == 0 && hasInventory && damage > 0;

    public static bool ForcedClearsBuddhaOnPlayer(int flags, int cheats, bool nodamage) =>
        (flags & DmgForced) != 0 && ((cheats & (1 | 2)) != 0 || nodamage);

    public readonly record struct PlayerDamagePhaseContext(
        bool HasPlayer,
        int Flags,
        int Cheats,
        bool Nodamage,
        bool EndLevelSector,
        int Damage,
        int Health,
        bool Telefrag,
        bool Invuln,
        bool God,
        bool God2,
        bool NoArmor,
        bool HasInventory,
        bool AutoUse,
        bool Deathmatch,
        bool AltNull,
        bool VoodooCompat,
        int BuddhaLevel);

    public enum PlayerDamagePhaseOutcome
    {
        Continue,
        StopZero,
        StopCancel,
    }

    public static PlayerDamagePhaseOutcome EvaluatePlayerDamagePhase(PlayerDamagePhaseContext ctx)
    {
        if (!ctx.HasPlayer) return PlayerDamagePhaseOutcome.Continue;
        var flags = ctx.Flags;
        if (ClearsForcedOnGod2(ctx.God2, false, ctx.Nodamage))
            flags &= ~DmgForced;
        var damage = EndLevelHellHack(ctx.EndLevelSector, ctx.Damage, ctx.Health)
            ? ClampEndLevelDamage(ctx.Damage, ctx.Health)
            : ctx.Damage;
        if ((flags & DmgForced) == 0)
        {
            if (PlayerInvulnBlocks(ctx.Telefrag, ctx.Invuln, ctx.God, ctx.God2, ctx.Nodamage))
                return PlayerDamagePhaseOutcome.StopZero;
            if (ArmorAbsorbPath(ctx.NoArmor, ctx.HasInventory, damage) && damage <= 0)
                return damage < 0 ? PlayerDamagePhaseOutcome.StopCancel : PlayerDamagePhaseOutcome.StopZero;
        }
        return PlayerDamagePhaseOutcome.Continue;
    }

    public readonly record struct PowerDrainContext(
        bool SourcePlayer,
        bool DontDrain,
        bool SamePlayerVictim,
        double BestStrength,
        double CandidateStrength,
        int Damage,
        bool HasVirtual,
        bool GaveBody);

    public static int EvaluatePowerDrainDamage(PowerDrainContext ctx)
    {
        if (!DrainEligible(ctx.SourcePlayer, ctx.DontDrain, ctx.SamePlayerVictim)) return 0;
        var strength = ctx.BestStrength;
        if (PickDrainStrength(ctx.CandidateStrength, strength)) strength = ctx.CandidateStrength;
        if (!DrainPositive(strength)) return 0;
        return (int)(strength * ctx.Damage);
    }

    public static bool ShouldPlayDrainSound(PowerDrainContext ctx, int drainDamage) =>
        drainDamage > 0 && ctx.GaveBody;

    public readonly record struct DeathTypeContext(
        int Mod,
        int FireMod,
        bool Player,
        bool AltNull,
        bool HasInflictor,
        int Health,
        int Damage,
        bool SpecialFireDamage,
        bool InflictorDeathTypeSet,
        int InflictorDeathMod);

    public static int ResolveDamageType(DeathTypeContext ctx)
    {
        if (DeathUsesInflictorType(ctx.HasInflictor, ctx.InflictorDeathTypeSet))
            return ctx.InflictorDeathMod;
        if (ctx.Mod != ctx.FireMod) return ctx.Mod;
        if (FlameDeathPlayer(true, ctx.Player, ctx.AltNull, ctx.Health, ctx.Damage, ctx.SpecialFireDamage))
            return ctx.FireMod;
        return ctx.Player && ctx.AltNull ? ctx.Mod : ctx.FireMod;
    }

    public static bool RedirectKillCredit(bool summoned, bool tracerPlayer, bool tracerIsMo) =>
        MinotaurKillCredit(summoned, tracerPlayer, tracerIsMo);
}
