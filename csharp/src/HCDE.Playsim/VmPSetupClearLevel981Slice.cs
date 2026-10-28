namespace HCDE.Playsim;

/// <summary>
/// `ClearLevelData` tail, `P_FreeLevelData`, and `P_SetupLevel` head from `p_setup.cpp` (**301–450**).
/// </summary>
public static class VmPSetupClearLevel981Slice
{
    public const int UdmfKeySlotCount = 4;
    public const int MaptypeUnknown = 0;
    public const long ViewzUnset = -1;

    public static bool UdmfKeySlotInRange(int slotIndex) =>
        slotIndex >= 0 && slotIndex < UdmfKeySlotCount;

    public static bool ShouldDrainStrifeDialogueQueue(bool nodePopped, bool queueMayHoldMore) =>
        nodePopped || queueMayHoldMore;

    public static bool ShouldDeleteSubsectorBsp(object? bsp, bool subsectorAllocated) =>
        subsectorAllocated && bsp is not null;

    public static bool ShouldDestroySpotState(object? spotState, bool spotStateOwned) =>
        spotStateOwned && spotState is not null;

    public static bool ShouldDeletePolyblockLink(object? link, bool polyBlockMapUsed) =>
        polyBlockMapUsed && link is not null;

    public static int DeletePolyblockChain(object? head, Func<object?, object?> getNext)
    {
        var deleted = 0;
        var link = head;
        while (ShouldDeletePolyblockLink(link, polyBlockMapUsed: link is not null))
        {
            var next = getNext(link);
            deleted++;
            link = next;
        }

        return deleted;
    }

    public static bool ShouldDestroyAutomap(object? automap, bool levelHadAutomap) =>
        levelHadAutomap && automap is not null;

    public static bool ShouldDeleteSpatialAccelerators(object? aabbTree, object? levelMesh) =>
        aabbTree is not null || levelMesh is not null;

    public static bool ShouldClearNetworkSectorBaselines(bool replicationEnabled) => replicationEnabled;

    public static bool ShouldIterateFreeLevelData(bool levelsExist, bool requestFullGc) =>
        levelsExist && requestFullGc;

    public static bool SetupLevelResetsScoreCounts(bool savegameRestore, bool levelLoaded) =>
        !savegameRestore && levelLoaded;

    public static bool SetupLevelClearsPlayerMobReference(bool playerSlotUsed, int maxPlayers, int slotIndex) =>
        playerSlotUsed && slotIndex >= 0 && slotIndex < maxPlayers;

    public static bool MaptypeAwaitingMapinfoClassification(int maptype, int unknownValue) =>
        maptype == unknownValue;

    public static bool ConsolePlayerNeedsViewHeightReset(object? consolePlayer, bool hasConsolePlayer) =>
        hasConsolePlayer && consolePlayer is not null;

    public static long SelectInitialViewHeight(bool hasConsolePlayer) =>
        hasConsolePlayer ? ViewzUnset : 0;

    public static bool TryPopStrifeDialogue(Func<bool> tryPop, Action<object?> releaseNode, out int nodesFreed)
    {
        nodesFreed = 0;
        while (tryPop())
        {
            releaseNode(null);
            nodesFreed++;
        }

        return nodesFreed > 0;
    }
}
