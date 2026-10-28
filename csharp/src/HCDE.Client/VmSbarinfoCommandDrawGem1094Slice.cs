namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawGem::Parse</c> tail, <c>Reset</c>, and <c>Tick</c> from <c>sbarinfo_commands.cpp</c> (**3151–3196**).
/// Chunk **1094**; **1093** owns parse flags and signed padding (**3116–3150**).
/// </summary>
public static class VmSbarinfoCommandDrawGem1094Slice
{
    public static (bool Valid, int ChainSize) ValidateDrawGemChainSizeFromToken(int tokenValue) =>
        tokenValue < 0
            ? (Valid: false, ChainSize: VmSbarinfoCommandDrawGem1093Slice.DefaultChainSize)
            : (Valid: true, ChainSize: tokenValue + 1);

    public static int ResolveDrawGemInterpolationSpeedForScript(
        bool armorMode,
        bool scriptInterpolateHealth,
        int scriptHealthInterpolationSpeed,
        bool scriptInterpolateArmor,
        int scriptArmorInterpolationSpeed,
        int parsedInterpolationSpeed) =>
        armorMode
            ? (scriptInterpolateArmor ? scriptArmorInterpolationSpeed : parsedInterpolationSpeed)
            : (scriptInterpolateHealth ? scriptHealthInterpolationSpeed : parsedInterpolationSpeed);

    public static (double DrawValue, int GoalValue) ApplyCommandDrawGemResetState(
        double currentDrawValue,
        int currentGoalValue) => (0, currentGoalValue);

    public static int ResolveDrawGemRawGoalStat(
        bool armorMode,
        bool armorPresent,
        int armorAmount,
        int health) =>
        armorMode ? (armorPresent ? armorAmount : 0) : health;

    public static int ResolveDrawGemGoalMax(bool armorMode, int classMaxHealth) =>
        armorMode ? 100 : classMaxHealth;

    public static int ComputeDrawGemGoalPercent(int goalStat, int maxStat)
    {
        if (maxStat == 0 || goalStat <= 0)
            return 0;
        var percent = goalStat * 100 / maxStat;
        return percent > 100 ? 100 : percent;
    }

    public static int ApplyDrawGemReverseGoalPercent(bool reverse, int goalPercent) =>
        reverse ? 100 - goalPercent : goalPercent;

    public static bool ShouldApplyDrawGemTickInterpolation(
        int interpolationSpeed,
        bool hudChanged,
        int levelTime) =>
        interpolationSpeed != 0 && (!hudChanged || levelTime == 1);

    public static int StepDrawGemDrawValueTowardGoal(
        int drawValue,
        int goalValue,
        int interpolationSpeed)
    {
        if (goalValue < drawValue)
            return drawValue - Math.Clamp((drawValue - goalValue) >> 2, 1, interpolationSpeed);
        if (drawValue < goalValue)
            return drawValue + Math.Clamp((goalValue - drawValue) >> 2, 1, interpolationSpeed);
        return drawValue;
    }

    public static int ResolveDrawGemTickDrawValue(
        int drawValue,
        int goalValue,
        int interpolationSpeed,
        bool hudChanged,
        int levelTime) =>
        ShouldApplyDrawGemTickInterpolation(interpolationSpeed, hudChanged, levelTime)
            ? StepDrawGemDrawValueTowardGoal(drawValue, goalValue, interpolationSpeed)
            : goalValue;

    public static int ResolveDrawGemChainWiggleBit(bool wiggleEnabled, int levelTime, int randomValue) =>
        wiggleEnabled && (levelTime & 1) != 0 ? randomValue & 1 : 0;
}

/// <summary>
/// <c>CommandWeaponAmmo</c> ctor, parse, and tick head from the same native band (**3221–3300**).
/// </summary>
public static class VmSbarinfoCommandWeaponAmmo1094Slice
{
    public static (bool ConditionAnd, bool HasSecondAmmoSlot) CreateWeaponAmmoCtorDefaults() =>
        (ConditionAnd: false, HasSecondAmmoSlot: false);

    public static bool ShouldReplaceWeaponAmmoClass(bool classFound, bool isAmmoKind) =>
        !classFound || !isAmmoKind;

    public static (bool ParseSecondAmmo, bool ConditionAnd) ResolveWeaponAmmoSecondAmmoSeparator(
        bool orOrToken,
        bool andAndToken) =>
        orOrToken ? (ParseSecondAmmo: true, ConditionAnd: false)
        : andAndToken ? (ParseSecondAmmo: true, ConditionAnd: true)
        : (ParseSecondAmmo: false, ConditionAnd: false);

    public static bool ResolveWeaponAmmoDualTypeFlowTruth(
        bool conditionAnd,
        bool primarySlotMatch,
        bool secondarySlotMatch) =>
        conditionAnd ? primarySlotMatch && secondarySlotMatch : primarySlotMatch || secondarySlotMatch;

    public static bool ResolveWeaponAmmoSingleConfiguredTypeTruth(
        bool usesPrimaryAmmo,
        bool primaryMatches,
        bool usesSecondaryAmmo,
        bool secondaryMatches) =>
        (usesPrimaryAmmo && primaryMatches) || (usesSecondaryAmmo && secondaryMatches);
}
