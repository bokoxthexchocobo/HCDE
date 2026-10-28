namespace HCDE.Playsim;

/// <summary>
/// Weapon/inventory CCMD helpers and pause gating from <c>g_game.cpp</c> (**525–674**).
/// </summary>
public static class VmGGameCcmd998Slice
{
    public const int NumWeaponSlots = 10;
    public const int WeaponSlotNone = NumWeaponSlots;
    public const int WeaponSlotPrev = NumWeaponSlots + 1;
    public const int WeaponSlotNext = NumWeaponSlots + 2;

    public const uint CheatTotallyFrozen = 1u << 12;

    public const int NetDisablePauseOff = 0;
    public const int NetDisablePauseControllersOnly = 1;
    public const int NetDisablePauseFullyDisabled = 2;

    public enum PauseRejectReason
    {
        None,
        FullyDisabled,
        ControllersOnly,
    }

    public readonly struct LandCommandEffect
    {
        public bool RequestLand { get; init; }
    }

    public readonly struct Turn180CommandEffect
    {
        public bool RequestTurn180 { get; init; }
    }

    public readonly struct InventoryUseAllEffect
    {
        public bool UseAllSentinel { get; init; }
        public bool ClearFlechetteRequest { get; init; }
    }

    public static int ResolveExplicitWeaponSlot(int slot) =>
        slot >= 0 && slot < NumWeaponSlots ? slot : WeaponSlotNone;

    public static bool TryResolveWeaponSlotFromArgument(string? slotText, out int sendWeaponSlot)
    {
        sendWeaponSlot = WeaponSlotNone;
        if (string.IsNullOrWhiteSpace(slotText))
            return false;
        if (!int.TryParse(slotText, out var parsed))
            return false;
        sendWeaponSlot = ResolveExplicitWeaponSlot(parsed);
        return true;
    }

    public static int ResolveWeapNextSlot() => WeaponSlotNext;

    public static int ResolveWeapPrevSlot() => WeaponSlotPrev;

    public static bool ShouldBlockCenterview(uint playerCheats) =>
        (playerCheats & CheatTotallyFrozen) != 0;

    public static PauseRejectReason ResolveNetgamePauseRejectReason(
        bool netgame,
        int netDisablePause,
        bool paused,
        bool localCanControlSettings)
    {
        if (!netgame)
            return PauseRejectReason.None;

        if (netDisablePause == NetDisablePauseFullyDisabled
            && (!paused || !localCanControlSettings))
            return PauseRejectReason.FullyDisabled;

        if (netDisablePause == NetDisablePauseControllersOnly
            && !localCanControlSettings)
            return PauseRejectReason.ControllersOnly;

        return PauseRejectReason.None;
    }

    public static bool ShouldQueueSendPause(PauseRejectReason rejectReason) =>
        rejectReason == PauseRejectReason.None;

    public static LandCommandEffect ResolveLandCommand() =>
        new() { RequestLand = true };

    public static Turn180CommandEffect ResolveTurn180Command() =>
        new() { RequestTurn180 = true };

    public static bool ShouldShowDisplayNameTagHud(int displayNameTags) =>
        (displayNameTags & 1) != 0;

    public static bool CanApplySelectedInventoryUse(int inventoryTics, bool hasPlayerMo) =>
        inventoryTics == 0 && hasPlayerMo;

    public static int InventoryTicsAfterInvUseCommand() => 0;

    public static InventoryUseAllEffect ResolveInvUseAllCommand() =>
        new() { UseAllSentinel = true, ClearFlechetteRequest = true };

    public static bool ParseUseCommandSubclassFlag(string? thirdArgument)
    {
        if (string.IsNullOrEmpty(thirdArgument))
            return false;
        if (string.Equals(thirdArgument, "true", StringComparison.OrdinalIgnoreCase))
            return true;
        return int.TryParse(thirdArgument, out var value) && value != 0;
    }

    public static bool ShouldRequireUseCommandItemName(int argumentCount) =>
        argumentCount > 1;

    public static bool HasInventoryForQuery(bool hasPlayerMo, bool hasSelectedInventory) =>
        hasPlayerMo && hasSelectedInventory;
}
