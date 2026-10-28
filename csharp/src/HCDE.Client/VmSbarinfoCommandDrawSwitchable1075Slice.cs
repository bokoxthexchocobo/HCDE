namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawSwitchableImage</c> compare operators and condition parse head from <c>sbarinfo_commands.cpp</c> (**301–450**).
/// Chunk **1075**; follows **1074** <c>GetIcon</c> seam (**300–304** excluded). <c>LookupKeySpecies</c> actor scan remains host.
/// </summary>
public static class VmSbarinfoCommandDrawSwitchable1075Slice
{
    public const int UnsetConditionalImage = -1;

    public enum SwitchableCompareOperator
    {
        Equal,
        Less,
        Greater,
        LessOrEqual,
        GreaterOrEqual,
        NotEqual
    }

    public enum SwitchableDrawCondition
    {
        Inventory,
        WeaponSlot,
        Invulnerability,
        KeySlot,
        ArmorType
    }

    public static (SwitchableCompareOperator Operator, int CompareValue, bool UsedDefault) ResolveCompareOperationFromTokens(
        bool sawEqual,
        bool sawLess,
        bool sawGreater,
        bool sawLessOrEqual,
        bool sawGreaterOrEqual,
        bool sawNotEqual,
        int parsedIntConst)
    {
        if (sawEqual)
            return (SwitchableCompareOperator.Equal, parsedIntConst, false);
        if (sawLess)
            return (SwitchableCompareOperator.Less, parsedIntConst, false);
        if (sawGreater)
            return (SwitchableCompareOperator.Greater, parsedIntConst, false);
        if (sawLessOrEqual)
            return (SwitchableCompareOperator.LessOrEqual, parsedIntConst, false);
        if (sawGreaterOrEqual)
            return (SwitchableCompareOperator.GreaterOrEqual, parsedIntConst, false);
        if (sawNotEqual)
            return (SwitchableCompareOperator.NotEqual, parsedIntConst, false);
        return (SwitchableCompareOperator.Greater, 0, true);
    }

    public static bool EvaluateSwitchableCompare(
        SwitchableCompareOperator op,
        int threshold,
        int sample) =>
        op switch
        {
            SwitchableCompareOperator.Equal => sample == threshold,
            SwitchableCompareOperator.Less => sample < threshold,
            SwitchableCompareOperator.Greater => sample > threshold,
            SwitchableCompareOperator.LessOrEqual => sample <= threshold,
            SwitchableCompareOperator.GreaterOrEqual => sample >= threshold,
            SwitchableCompareOperator.NotEqual => sample != threshold,
            _ => sample > threshold
        };

    public static (bool Recognized, SwitchableDrawCondition Condition) TryClassifySwitchableConditionKeyword(
        string identifier)
    {
        if (identifier.Equals("weaponslot", StringComparison.OrdinalIgnoreCase))
            return (true, SwitchableDrawCondition.WeaponSlot);
        if (identifier.Equals("invulnerable", StringComparison.OrdinalIgnoreCase))
            return (true, SwitchableDrawCondition.Invulnerability);
        if (identifier.Equals("keyslot", StringComparison.OrdinalIgnoreCase))
            return (true, SwitchableDrawCondition.KeySlot);
        if (identifier.Equals("armortype", StringComparison.OrdinalIgnoreCase))
            return (true, SwitchableDrawCondition.ArmorType);
        return (false, SwitchableDrawCondition.Inventory);
    }

    public static (int Image0, int Image1, int Image2, int Value0, int Value1, int Armor0, int Armor1)
        InitializeSwitchableImageFields() =>
        (UnsetConditionalImage, UnsetConditionalImage, UnsetConditionalImage, 0, 0, 0, 0);

    public static bool ShouldParseInventoryConditionString(bool tokenWasIdentifier, bool conditionStillInventory) =>
        !tokenWasIdentifier && conditionStillInventory;

    public static bool ShouldParseWeaponSlotIndex(SwitchableDrawCondition condition) =>
        condition == SwitchableDrawCondition.WeaponSlot;

    public static bool ShouldParseKeySlotIndex(SwitchableDrawCondition condition) =>
        condition == SwitchableDrawCondition.KeySlot;

    public static bool ShouldParseArmorTypeIdentifier(SwitchableDrawCondition condition) =>
        condition == SwitchableDrawCondition.ArmorType;
}
