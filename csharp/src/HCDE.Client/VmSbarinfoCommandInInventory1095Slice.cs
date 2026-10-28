namespace HCDE.Client;

/// <summary>
/// Status-bar flow commands from <c>sbarinfo_commands.cpp</c> (**3304–3450**): inventory, alpha block, health, invulnerable, water-level hull head.
/// Chunk **1095**; **1094** owns weapon-ammo OR/AND helpers and draw-gem tick math.
/// </summary>
public static class VmSbarinfoCommandInInventory1095Slice
{
    public static (bool ConditionAnd, int Amount0, int Amount1) CreateInInventoryCtorDefaults() =>
        (ConditionAnd: false, Amount0: 0, Amount1: 0);

    public static bool ShouldReplaceInInventoryItemClass(bool classFound, bool isInventoryKind) =>
        !classFound || !isInventoryKind;

    public static int ResolveInInventoryAmountAfterOptionalComma(
        bool commaPresent,
        int parsedAmount,
        int priorAmount) =>
        commaPresent ? parsedAmount : priorAmount;

    public static (bool ContinueSecondItemParse, bool ConditionAnd) ResolveInInventorySecondItemListContinuation(
        bool orOrToken,
        bool andAndToken) =>
        orOrToken ? (ContinueSecondItemParse: true, ConditionAnd: false)
        : andAndToken ? (ContinueSecondItemParse: true, ConditionAnd: true)
        : (ContinueSecondItemParse: false, ConditionAnd: false);

    public static bool ShouldNullifyInInventorySlotForInsufficientAmount(
        bool slotPresent,
        int requiredAmount,
        int heldAmount) =>
        slotPresent && requiredAmount > 0 && heldAmount < requiredAmount;

    public static bool ResolveInInventoryDualConfiguredItemFlowTruth(
        bool conditionAnd,
        bool firstSlotCounts,
        bool secondSlotCounts) =>
        conditionAnd ? firstSlotCounts && secondSlotCounts : firstSlotCounts || secondSlotCounts;

    public static bool ResolveInInventoryFlowTruthWhenSecondItemConfigured(
        bool secondItemConfigured,
        bool conditionAnd,
        bool firstSlotCounts,
        bool secondSlotCounts) =>
        secondItemConfigured
            ? ResolveInInventoryDualConfiguredItemFlowTruth(conditionAnd, firstSlotCounts, secondSlotCounts)
            : firstSlotCounts;
}

public static class VmSbarinfoCommandAlpha1095Slice
{
    public static (bool ForceScaled, bool FullScreenOffsets, int BlockAlpha) ResolveCommandAlphaMainBlockDrawInvocation(
        bool parentForceScaled,
        bool parentFullScreenOffsets,
        int parentBlockAlpha) =>
        (parentForceScaled, parentFullScreenOffsets, parentBlockAlpha);

    public static (float Alpha, bool DelegateToFlowControlParse) ResolveCommandAlphaParseAlphaToken(float tokenValue) =>
        (Alpha: tokenValue, DelegateToFlowControlParse: true);
}

public static class VmSbarinfoCommandIfHealth1095Slice
{
    public static (bool Percentage, int HpAmount) CreateIfHealthCtorDefaults() =>
        (Percentage: false, HpAmount: 0);

    public static (bool Percentage, int HpAmount) ResolveIfHealthParseNegatableThreshold(
        int parsedNumber,
        bool percentTokenFollows) =>
        (Percentage: percentTokenFollows, HpAmount: parsedNumber);

    public static int ResolveIfHealthPlayerHealthForCompare(
        bool percentage,
        int health,
        int maxHealth) =>
        percentage && maxHealth != 0 ? health * 100 / maxHealth : health;

    public static bool ResolveIfHealthFlowTruth(int playerHealthForCompare, int hpAmount) =>
        playerHealthForCompare >= hpAmount;
}

public static class VmSbarinfoCommandIfInvulnerable1095Slice
{
    public static bool ResolveIfInvulnerableFlowTruth(
        bool invulnerableActorFlag,
        bool godmodeCheat,
        bool godmode2Cheat) =>
        invulnerableActorFlag || godmodeCheat || godmode2Cheat;
}

public static class VmSbarinfoCommandIfWaterLevel1095Slice
{
    /// <summary>Hull opens at **3449**; parse/tick land in the next band.</summary>
    public static (bool NegatableFlowControl, int DefaultThreshold) CreateCommandIfWaterLevelHullHeadDefaults() =>
        (NegatableFlowControl: true, DefaultThreshold: 0);
}
