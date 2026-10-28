namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> inventory cases (<c>d_net.cpp</c>).</summary>
public static class VmDNetDoCommandInventory
{
    public static bool ShouldRunInventoryLevelCommandNative(
        int gameState,
        int levelState,
        bool paused,
        int playerState,
        int deadState) =>
        gameState == levelState && !paused && playerState != deadState;

    public static bool InventoryChainHasNext(object? item) => item is not null;

    public static bool InventoryIdMatches(uint itemId, uint targetId) => itemId == targetId;

    public static bool IsInvDropCommandNative(int cmd, int demInvDrop) => cmd == demInvDrop;

    public static bool ShouldUseInventoryNative(int cmd, int demInvUse) => cmd == demInvUse;

    public static bool InvDropReadsAmountNative(int cmd, int demInvDrop) => cmd == demInvDrop;

    public static int DefaultInvDropAmountNative() => -1;

    public static bool ShouldContinueInventoryScanNative(object? item, uint inventoryId, uint targetId) =>
        item is not null && inventoryId != targetId;

    public static bool ShouldInvokeUseAllVirtualNative(object? item) => item is not null;

    public static bool ShouldApplyInvUseNative(int cmd, int demInvUse, object? item) =>
        cmd == demInvUse && item is not null;

    public static bool ShouldApplyInvDropNative(int cmd, int demInvDrop, object? item) =>
        cmd == demInvDrop && item is not null;
}
