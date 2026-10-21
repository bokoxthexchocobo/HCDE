namespace HCDE.Net.Core;

/// <summary>Net event pump, writers, dynamic buffer, and weapon helpers (<c>d_net.cpp</c> 8269–8551).</summary>
public static class VmDNetNetEventWrite
{
    public static bool RejectAdoptRoomWhenAuthorityNative(bool localAuthority) => localAuthority;

    public static bool RejectAdoptRoomOutOfRangeNative(int room) => room < 0 || room > 255;

    public static int PendingAdoptRoomNative(int room) => room;

    public static int GrowBufferLengthNative(int len, int bufferLen) =>
        len > bufferLen ? (len + 255) & ~255 : bufferLen;

    public static bool ShouldReallocBufferNative(int len, int bufferLen) => len > bufferLen;

    public static int BufferLengthAfterSetNative(int len, bool hasData) => hasData ? len : 0;

    public static bool ShouldReturnBufferDataNative(int len) => len > 0;

    public static bool ShouldSkipRemoveClassLivePlayerNative(object? playerActor) => playerActor is not null;

    public static bool ShouldSkipRemoveClassNonMapActorNative(bool isMapActor) => !isMapActor;

    public static bool ShouldDestroyRemovedClassActorNative(bool isKind, bool skippedLivePlayer, bool skippedInventory) =>
        isKind && !skippedLivePlayer && !skippedInventory;

    public static int IncrementRemoveClassCountNative(int count) => count + 1;

    public static bool RemoveClassReportsLivePlayersNative(bool sawLivePlayer) => sawLivePlayer;

    public static bool SelectWeaponRequiresMobjNative(object? mobj) => mobj is not null;

    public static bool SelectWeaponLevelGateNative(int gameState, int levelState, bool paused, int playerState, int liveState) =>
        gameState == levelState && !paused && playerState == liveState;

    public static bool SelectWeaponSlotInRangeNative(int slot, int numWeaponSlots) =>
        slot >= 0 && slot < numWeaponSlots;

    public static bool SelectWeaponUsesNextSlotNative(int slot, int wstNext) => slot == wstNext;

    public static bool SelectWeaponUsesPrevSlotNative(int slot, int wstPrev) => slot == wstPrev;

    public static bool SelectWeaponCheckAmmoNative(int dmflags2, int dontCheckAmmoMask) =>
        (dmflags2 & dontCheckAmmoMask) == 0;

    public static bool InventoryScanMatchesIdNative(uint inventoryId, uint targetId) => inventoryId == targetId;

    public static bool ShouldAdvanceInventoryScanNative(object? item) => item is not null;

    public static bool ResolvedWeaponMatchesInventoryNative(object? invItem, object? weap) => invItem == weap;

    public static bool ShouldChangeWeaponForConsolePlayerNative(int player, int consolePlayer) =>
        player == consolePlayer;

    public static bool ShouldPlayWeaponChangeSoundNative(int player, int consolePlayer, object? readyWeapon, object? weap) =>
        player == consolePlayer && readyWeapon != weap;

    public static bool ShouldShowWeaponNameTagNative(int displayNameTags, int weaponNameBit, object? statusBar, object? smallFont) =>
        (displayNameTags & weaponNameBit) != 0 && statusBar is not null && smallFont is not null;

    public static bool UseFlechetteLevelGateNative(int gameState, int levelState, bool paused, int playerState, int liveState) =>
        gameState == levelState && !paused && playerState == liveState;

    public static bool UseFlechetteRequiresMobjNative(object? mobj) => mobj is not null;

    public static bool UseFlechetteResolvedItemMatchesNative(object? invItem, object? item) => invItem == item;

    public static bool ShouldUseFlechetteWhenResolvedNative(object? invItem, object? item) =>
        invItem == item && item is not null;

    public static bool NetWriteBytesHasRemainingNative(int len) => len > 0;

    public static int NetWriteBytesNextIndexNative(int index) => index + 1;

    public static bool DebugNetCommandVerboseNative(int netEventDebugLevel) => netEventDebugLevel >= 2;

    public static bool ShouldPickWeaponFromSlotNative(int slot, int numWeaponSlots, int wstNext, int wstPrev) =>
        SelectWeaponSlotInRangeNative(slot, numWeaponSlots)
        || SelectWeaponUsesNextSlotNative(slot, wstNext)
        || SelectWeaponUsesPrevSlotNative(slot, wstPrev);

    public static bool ShouldAbortSelectWeaponWhenUnresolvedNative(object? weap) => weap is null;

    public static bool ShouldAbortWhenInventoryMismatchNative(bool invMatches) => !invMatches;

    public static bool ShouldCallUseInventoryNative(bool invMatches) => invMatches;

    public static int DynamicBufferRoundUpNative(int len) => (len + 255) & ~255;

    public static bool DynamicBufferCopyWhenDataPresentNative(bool dataPresent) => dataPresent;

    public static bool DynamicBufferClearWhenNullDataNative(bool dataPresent) => !dataPresent;

    public static bool RemoveClassIteratorContinuesNative(object? actor) => actor is not null;

    public static bool RemoveClassIsRequestedKindNative(bool isKind) => isKind;

    public static bool AdoptRoomIgnoresInvalidGuestNative(bool localAuthority, int room) =>
        RejectAdoptRoomWhenAuthorityNative(localAuthority) || RejectAdoptRoomOutOfRangeNative(room);

    public static bool NewClientTicDispatchesNative(bool hasEvents) => hasEvents;

    public static bool WriteBytesContinueNative(int remaining) => remaining > 0;

    public static int WriteBytesRemainingAfterStepNative(int remaining) => remaining - 1;

    public static bool SetDataClearsLengthWhenNullNative(bool dataPresent) => !dataPresent;

    public static bool SetDataCopiesWhenPresentNative(bool dataPresent) => dataPresent;

    public static bool GetDataReturnsNullWhenEmptyNative(int len) => len == 0;

    public static bool TArrayViewUsesBufferNative(int len) => len > 0;

    public static bool SelectWeaponStopsWhenWeapNullNative(object? weap) => weap is null;

    public static bool SelectWeaponStopsWhenInventoryMismatchNative(bool matches) => !matches;

    public static bool FlechetteStopsWhenItemNullNative(object? item) => item is null;

    public static bool FlechetteStopsWhenInventoryMismatchNative(bool matches) => !matches;

}
