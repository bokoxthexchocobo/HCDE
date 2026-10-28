namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawSwitchableImage</c> parse tail and <c>Tick</c> draw-alt policy from <c>sbarinfo_commands.cpp</c> (**451–600**).
/// Chunk **1076**; follows **1075** condition parse head (**434–450**). Compare evaluation delegates to <see cref="VmSbarinfoCommandDrawSwitchable1075Slice"/>.
/// </summary>
public static class VmSbarinfoCommandDrawSwitchable1076Slice
{
    public const int CheatGodMode = 1 << 1;
    public const int CheatGodMode2 = 1 << 25;
    public const int CheatInvulnerabilityMask = CheatGodMode | CheatGodMode2;

    public static bool ShouldEnableConditionAndParse(
        bool sawAndAndToken,
        VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition condition) =>
        sawAndAndToken && condition != VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition.Invulnerability;

    public static int CountConditionalImagesToLoad(bool conditionAnd) => conditionAnd ? 3 : 1;

    public static bool ShouldParseSecondSlotValueForAnd(
        VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition condition) =>
        condition == VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition.KeySlot
        || condition == VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition.WeaponSlot;

    public static int ResolveWeaponSlotDrawAlt(bool foundOwnedWeaponInSlot) => foundOwnedWeaponInSlot ? 0 : 1;

    public static int ResolveInvulnerabilityDrawAlt(int playerCheats) =>
        (playerCheats & CheatInvulnerabilityMask) != 0 ? 1 : 0;

    public static (bool FoundFirst, bool FoundSecond) ApplyKeySlotNumberMatch(
        int keyNumber,
        int expectedFirst,
        int expectedSecond,
        bool conditionAnd,
        bool foundFirst,
        bool foundSecond)
    {
        if (keyNumber == 0)
            return (foundFirst, foundSecond);
        if (keyNumber == expectedFirst)
            foundFirst = true;
        if (conditionAnd && keyNumber == expectedSecond)
            foundSecond = true;
        return (foundFirst, foundSecond);
    }

    public static (bool FoundFirst, bool FoundSecond) ApplyKeySlotSpeciesMatch(
        bool speciesMatchesFirst,
        bool speciesMatchesSecond,
        bool conditionAnd,
        bool foundFirst,
        bool foundSecond)
    {
        if (speciesMatchesFirst)
            foundFirst = true;
        if (conditionAnd && speciesMatchesSecond)
            foundSecond = true;
        return (foundFirst, foundSecond);
    }

    public static int ResolveKeySlotDrawAlt(bool conditionAnd, bool foundFirst, bool foundSecond, int defaultAlt = 1)
    {
        if (conditionAnd)
        {
            if (foundFirst && foundSecond)
                return 0;
            if (foundFirst)
                return 2;
            if (foundSecond)
                return 3;
            return defaultAlt;
        }
        return foundFirst ? 0 : defaultAlt;
    }

    public static bool EvaluateArmorTypeMatch(
        int armorNameIndex,
        int expectedArmorTypeIndex,
        VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableCompareOperator op,
        int threshold,
        int armorAmount) =>
        armorNameIndex == expectedArmorTypeIndex
        && VmSbarinfoCommandDrawSwitchable1075Slice.EvaluateSwitchableCompare(op, threshold, armorAmount);

    public static int ResolveArmorTypeDrawAlt(bool conditionAnd, bool matchesFirst, bool matchesSecond, int defaultAlt = 1)
    {
        if (conditionAnd)
        {
            if (matchesFirst && matchesSecond)
                return 0;
            if (matchesSecond)
                return 3;
            if (matchesFirst)
                return 2;
            return defaultAlt;
        }
        return matchesFirst ? 0 : defaultAlt;
    }
}
