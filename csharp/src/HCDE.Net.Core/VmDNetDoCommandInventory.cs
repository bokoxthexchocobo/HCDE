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
}
