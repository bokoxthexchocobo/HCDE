namespace HCDE.Net.Core;

/// <summary>Net event pump, writers, dynamic buffer, and weapon helpers (<c>d_net.cpp</c> 8269–8510).</summary>
public static class VmDNetNetEventWrite
{
    public static bool RejectAdoptRoomWhenAuthorityNative(bool localAuthority) => localAuthority;

    public static bool RejectAdoptRoomOutOfRangeNative(int room) => room < 0 || room > 255;

    public static int GrowBufferLengthNative(int len, int bufferLen) =>
        len > bufferLen ? (len + 255) & ~255 : bufferLen;

    public static bool ShouldDestroyRemovedClassActorNative(bool isKind, bool skippedLivePlayer, bool skippedInventory) =>
        isKind && !skippedLivePlayer && !skippedInventory;

    public static int IncrementRemoveClassCountNative(int count) => count + 1;

    public static bool SelectWeaponLevelGateNative(int gameState, int levelState, bool paused, int playerState, int liveState) =>
        gameState == levelState && !paused && playerState == liveState;

    public static bool SelectWeaponSlotInRangeNative(int slot, int numWeaponSlots) =>
        slot >= 0 && slot < numWeaponSlots;

    public static bool InventoryScanMatchesIdNative(uint inventoryId, uint targetId) => inventoryId == targetId;

    public static bool UseFlechetteLevelGateNative(int gameState, int levelState, bool paused, int playerState, int liveState) =>
        gameState == levelState && !paused && playerState == liveState;

    public static bool WriteBytesContinueNative(int remaining) => remaining > 0;

    public static int WriteBytesRemainingAfterStepNative(int remaining) => remaining - 1;
}
