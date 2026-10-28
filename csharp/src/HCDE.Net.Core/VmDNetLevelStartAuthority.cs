using System.Collections.Generic;

namespace HCDE.Net.Core;

/// <summary>Level-start waiting, playable client masks, and authority slot selection from <c>d_net.cpp</c> (4993–5579).</summary>
public static class VmDNetLevelStartAuthority
{
    public const int LevelStartReady = 0;
    public const int GameStateLevel = 0;

    public static bool SetWaitingNative(
        bool netgame,
        bool demoPlayback,
        bool hasPlayableNetworkClients,
        out bool shouldResetLevelStartAck,
        out bool shouldResetLatencyCycle,
        out bool shouldSetWaitingStatus)
    {
        shouldResetLevelStartAck = false;
        shouldResetLatencyCycle = false;
        shouldSetWaitingStatus = false;
        if (!netgame || demoPlayback || !hasPlayableNetworkClients)
            return false;
        shouldResetLevelStartAck = true;
        shouldResetLatencyCycle = true;
        shouldSetWaitingStatus = true;
        return true;
    }

    public static int SelectNextServiceAuthoritySlotNative(
        IEnumerable<int> networkClients,
        int leavingAuthority,
        Func<int, bool> isServerReservedSlot,
        bool usesDedicatedServerSlot,
        int reservedServerSlot)
    {
        foreach (var client in networkClients)
        {
            if (client == leavingAuthority || isServerReservedSlot(client))
                continue;
            return client;
        }

        foreach (var client in networkClients)
        {
            if (client != leavingAuthority)
                return client;
        }

        return usesDedicatedServerSlot ? reservedServerSlot : leavingAuthority;
    }

    public static bool IsMapLoadedNative(int gameState) => gameState == GameStateLevel;

    public static ulong LevelStartPlayableMaskNative(
        IEnumerable<int> networkClients,
        Func<int, bool> isAuthoritySlot,
        Func<int, bool> isLateJoinSyncPending)
    {
        ulong mask = 0;
        foreach (var client in networkClients)
        {
            if (isAuthoritySlot(client) || isLateJoinSyncPending(client))
                continue;
            mask |= 1ul << client;
        }

        return mask;
    }

    public static int AuthorityLevelStartStatusAfterReleaseNative() => LevelStartReady;

    public static int PlayableNetworkClientCountNative(
        IEnumerable<int> networkClients,
        Func<int, bool> isAuthoritySlot,
        Func<int, bool> isLateJoinSyncPending)
    {
        var count = 0;
        foreach (var client in networkClients)
        {
            if (isAuthoritySlot(client) || isLateJoinSyncPending(client))
                continue;
            count++;
        }

        return count;
    }

    public static bool HasPlayableNetworkClientsNative(int playableCount) => playableCount > 0;
}
