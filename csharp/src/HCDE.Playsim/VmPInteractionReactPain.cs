namespace HCDE.Playsim;

/// <summary>Auto-use health and react-to-damage / pain from <c>p_interaction.cpp</c> (826–1059).</summary>
public static class VmPInteractionReactPain
{
    public const int DmgNoPain = 1;
    public const int Mf5Nopain = 1;
    public const int Mf5Painless = 1 << 1;
    public const int Mf5Nodamage = 1 << 2;
    public const int Mf6Forcepain = 1 << 3;
    public const int Mf7Allowpain = 1 << 4;
    public const int Mf7Causepain = 1 << 5;
    public const int MfSkullfly = 1 << 6;
    public const int CfGodmode = 1;
    public const int CfGodmode2 = 1 << 1;
    public const int TelefragDamage = 1000000;
    public const int NameElectric = 1;
    public const int NamePoisoncloud = 2;

    public static bool MustForcePain(bool inflictorExists, int inflictorFlags6) =>
        inflictorExists && (inflictorFlags6 & Mf6Forcepain) != 0;

    public static bool IsFakePain(int targetFlags7, int targetFlags5, int damage, bool inflictorExists, int inflictorFlags7) =>
        ((targetFlags7 & Mf7Allowpain) != 0 || (targetFlags5 & Mf5Nodamage) != 0) && damage > 0 ||
        inflictorExists && (inflictorFlags7 & Mf7Causepain) != 0;

    public static bool DeadOrNoDamage(bool targetNull, int health, int damage) =>
        targetNull || health < 1 || damage < 0;

    public static bool GodmodeBlocksReact(int cheats, int flags5, int damage) =>
        (cheats & CfGodmode2) != 0 || (flags5 & Mf5Nopain) != 0 ||
        (cheats & CfGodmode) != 0 && damage < TelefragDamage;

    public static bool WoundStateTriggers(int health, int woundHealth) => health <= woundHealth;

    public static bool NoPainFlag(int flags, int targetFlags5, bool inflictorPainless) =>
        (flags & DmgNoPain) != 0 || (targetFlags5 & Mf5Nopain) != 0 || inflictorPainless;

    public static bool ShouldWakeUp(int damage, bool fakePain, bool forcedPain) =>
        damage > 0 || fakePain || forcedPain;

    public static bool PainRollEligible(
        bool noPain,
        bool wakeup,
        bool hasPlayer,
        bool noSkillPain,
        int flags,
        bool forcedPain,
        int damage,
        int painThreshold) =>
        !noPain && wakeup &&
        (hasPlayer || !noSkillPain) && (flags & MfSkullfly) == 0 &&
        (forcedPain || damage >= painThreshold);

    public static bool SetJustHit(bool justHit, bool targetIsSource, bool hasTarget, bool targetIsFriend) =>
        justHit && (targetIsSource || !hasTarget || !targetIsFriend);

    public static bool TriggerPainBlocked(bool targetNull, int flags5, int health) =>
        targetNull || (flags5 & Mf5Nopain) != 0 || health < 1;

    public static bool PainRollSucceeded(bool forcedPain, int randomRoll, int painChance) =>
        forcedPain || randomRoll < painChance;

    public static bool ElectricFlinchRoll(int lightningRoll) => lightningRoll < 96;

    public static bool ElectricHowlRoll(bool monster, int roll) => monster && roll < 128;

    public static bool PoisonCloudHowlRoll(bool monster, int roll) => monster && roll < 128;

    public readonly record struct ReactDamageContext(
        bool TargetNull,
        int Health,
        int Damage,
        int Cheats,
        int TargetFlags5,
        bool HasWoundState,
        int WoundHealth,
        int Flags,
        int TargetFlags5Pain,
        bool InflictorExists,
        int InflictorFlags5,
        int InflictorFlags6,
        int InflictorFlags7,
        int TargetFlags7,
        int OriginalDamage,
        bool HasPlayer,
        bool NoSkillPain,
        int ActorFlags,
        int PainThreshold,
        bool InflictorPainTypeSet,
        int InflictorPainType);

    public static bool TryReactToDamage(ReactDamageContext ctx, out bool woundApplied, out bool justHit)
    {
        woundApplied = false;
        justHit = false;
        if (DeadOrNoDamage(ctx.TargetNull, ctx.Health, ctx.Damage))
            return false;
        if (GodmodeBlocksReact(ctx.Cheats, ctx.TargetFlags5, ctx.Damage))
            return false;
        if (ctx.HasWoundState && WoundStateTriggers(ctx.Health, ctx.WoundHealth))
        {
            woundApplied = true;
            return true;
        }

        var noPain = NoPainFlag(ctx.Flags, ctx.TargetFlags5Pain,
            ctx.InflictorExists && (ctx.InflictorFlags5 & Mf5Painless) != 0);
        var fakePain = IsFakePain(ctx.TargetFlags7, ctx.TargetFlags5, ctx.OriginalDamage,
            ctx.InflictorExists, ctx.InflictorFlags7);
        var forced = MustForcePain(ctx.InflictorExists, ctx.InflictorFlags6);
        var wakeup = ShouldWakeUp(ctx.Damage, fakePain, forced);
        if (PainRollEligible(noPain, wakeup, ctx.HasPlayer, ctx.NoSkillPain, ctx.ActorFlags, forced,
                ctx.Damage, ctx.PainThreshold))
        {
            justHit = true;
        }

        return true;
    }

    public readonly record struct PainChanceContext(
        bool TargetNull,
        int Flags5,
        int Health,
        bool ForcedPain,
        int RandomRoll,
        int PainChance,
        int Mod,
        bool Monster,
        int LightningRoll,
        int PoisonRoll,
        bool HasPainState,
        bool Zscript);

    public static bool EvaluateTriggerPain(PainChanceContext ctx, out bool flinched)
    {
        flinched = false;
        if (TriggerPainBlocked(ctx.TargetNull, ctx.Flags5, ctx.Health))
            return false;
        if (!PainRollSucceeded(ctx.ForcedPain, ctx.RandomRoll, ctx.PainChance))
            return false;
        if (ctx.Mod == NameElectric)
        {
            if (ElectricFlinchRoll(ctx.LightningRoll) && ctx.HasPainState)
            {
                flinched = true;
                return true;
            }

            return ElectricHowlRoll(ctx.Monster, ctx.LightningRoll);
        }

        if (ctx.HasPainState)
            flinched = true;
        if (ctx.Mod == NamePoisoncloud)
            _ = PoisonCloudHowlRoll(ctx.Monster, ctx.PoisonRoll);
        return ctx.Zscript ? flinched : true;
    }
}
