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

    public static bool AutoUseHealthVirtual(bool hasVirtual) => hasVirtual;

    public static bool AutoUseStrifeVirtual(bool hasVirtual) => hasVirtual;

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

    public static bool SameTargetWake(bool sourceIsTarget) => sourceIsTarget;

    public static bool SwitchTargetWake(bool canSwitch) => canSwitch;

    public static bool SetJustHit(bool justHit, bool targetIsSource, bool hasTarget, bool targetIsFriend) =>
        justHit && (targetIsSource || !hasTarget || !targetIsFriend);

    public static bool CallReactVirtual(bool hasVirtual) => hasVirtual;

    public static bool TriggerPainBlocked(bool targetNull, int flags5, int health) =>
        targetNull || (flags5 & Mf5Nopain) != 0 || health < 1;

    public static bool PainRollSucceeded(bool forcedPain, int randomRoll, int painChance) =>
        forcedPain || randomRoll < painChance;

    public static bool ElectricFlinchRoll(int lightningRoll) => lightningRoll < 96;

    public static bool ElectricHowlRoll(bool monster, int roll) => monster && roll < 128;

    public static bool PoisonCloudHowlRoll(bool monster, int roll) => monster && roll < 128;

    public static bool ZscriptReturnsFlinch(bool zscript) => zscript;
}
