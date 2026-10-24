namespace HCDE.Net.Core;

/// <summary>Client removal, timeout drop, runtime setup driver (<c>i_net.cpp</c> 2678–2918).</summary>
public static class VmINetRuntimeSetup
{
    public static void RemoveClientConnectionNative(
        int client,
        string? reason,
        Action<int, string?>? netClientDisconnected,
        Action<int>? clearSettingsController,
        Action<int>? clearRuntimeJoinState,
        Action<int>? clearClient,
        Action<int>? subtractNetworkClient,
        Action<int, string>? netLog,
        byte ncmdSetup,
        byte preDisconnect,
        Span<byte> netBuffer,
        ref int netBufferLength,
        int maxClients,
        Func<int, byte>? getStatus,
        byte cstatNone,
        Action<int, int, bool>? setClientAck,
        Func<int, uint>? getSessionToken,
        Action<int, (uint, ushort)>? sendDisconnectNotify)
    {
        netClientDisconnected?.Invoke(client, reason);
        clearSettingsController?.Invoke(client);
        clearRuntimeJoinState?.Invoke(client);
        clearClient?.Invoke(client);
        subtractNetworkClient?.Invoke(client);
        netLog?.Invoke(client, reason ?? "");
        netBuffer[0] = ncmdSetup;
        netBuffer[1] = preDisconnect;
        netBuffer[2] = (byte)client;
        for (var i = 1; i < maxClients; i++)
        {
            if (getStatus?.Invoke(i) == cstatNone)
                continue;
            setClientAck?.Invoke(i, client, false);
            VmINetPregameService.WriteBe32Native(netBuffer.Slice(3, 4), getSessionToken?.Invoke(i) ?? 0);
            netBufferLength = 7;
            sendDisconnectNotify?.Invoke(i, (0, 0));
        }
    }

    public static bool DropClientForHcdeTimeoutNative(
        int client,
        int maxClients,
        Func<int, byte>? getStatus,
        byte cstatNone,
        Func<int, bool>? isHcdeConnect,
        Func<int, bool>? isReadyWithRosterAndStartAck,
        Func<int, (byte Service, byte Key, uint Sequence, uint SendCount, ulong FirstSendTime)?>? findTimedOutService,
        Func<int, ulong>? getLastValidRxTime,
        ulong now,
        ulong serviceTimeoutMs,
        ulong hardTimeoutMs,
        Action<int, string>? netLog,
        Action<(uint, ushort), byte>? rejectConnection,
        Action<int, string>? removeClientConnection,
        ref int? connectedPlayers,
        Action<int, int>? updatePlayers,
        int maxClientsForUi)
    {
        if (client <= 0 || client >= maxClients || getStatus?.Invoke(client) == cstatNone || isHcdeConnect?.Invoke(client) != true)
            return false;
        if (isReadyWithRosterAndStartAck?.Invoke(client) == true)
            return false;
        var pending = findTimedOutService?.Invoke(client);
        if (pending is null)
            return false;
        var elapsed = now - pending.Value.FirstSendTime;
        var lastRx = getLastValidRxTime?.Invoke(client) ?? 0;
        if (lastRx > 0 && now - lastRx < serviceTimeoutMs && elapsed < hardTimeoutMs)
            return false;
        rejectConnection?.Invoke((0, 0), 5);
        removeClientConnection?.Invoke(client, "HCDE service setup timeout");
        if (connectedPlayers is not null && connectedPlayers > 0)
        {
            connectedPlayers--;
            updatePlayers?.Invoke(connectedPlayers.Value, maxClientsForUi);
        }
        return true;
    }

    public static void DriveRuntimeSetupStateForClientNative(
        int client,
        int connectedPlayers,
        int maxClients,
        byte statusConnecting,
        byte statusWaiting,
        byte statusReady,
        byte cstatNone,
        Func<int, byte>? getStatus,
        Func<int, bool>? clientGotAck,
        Func<int, bool>? beginConnectAckResend,
        Action<int, int, int, byte>? sendConnectAck,
        Func<int, bool>? beginConsolePlayerService,
        Action<int, int, int, byte>? writeConsolePlayerPayload,
        Func<int, bool>? commitConsolePlayerService,
        Action<int>? flushReliableServices,
        Func<int, bool>? beginUserInfoAckService,
        Func<int, bool>? commitUserInfoAckService,
        Func<int, bool>? hasMapLoadInfo,
        Func<int, bool>? beginMapLoadService,
        Action<int>? appendMapLoadInfo,
        Func<int, bool>? commitMapLoadService,
        Func<int, bool>? hasGameInfo,
        Func<int, bool>? beginGameInfoService,
        Action<int>? appendGameInfo,
        Func<int, bool>? commitGameInfoService,
        Func<int, bool>? hasRosterInfo,
        Func<int, bool>? queueRosterService,
        Func<int, bool>? isRuntimeJoin,
        Func<int, bool>? hasBootstrapInfo,
        Func<int, bool>? queueBootstrapService,
        Action<int>? markClientReady,
        Action<int>? netClientUpdated,
        Func<int, bool>? beginStartGameService,
        Func<int, bool>? commitStartGameService)
    {
        if (client <= 0 || client >= maxClients || getStatus?.Invoke(client) == cstatNone)
            return;
        var status = getStatus!.Invoke(client);
        if (status == statusConnecting)
        {
            if (beginConnectAckResend?.Invoke(client) == true)
                sendConnectAck?.Invoke(client, connectedPlayers, maxClients, 0);
            if (beginConsolePlayerService?.Invoke(client) == true)
            {
                writeConsolePlayerPayload?.Invoke(client, connectedPlayers, maxClients, 0);
                commitConsolePlayerService?.Invoke(client);
            }
            flushReliableServices?.Invoke(client);
            return;
        }
        if (status == statusWaiting)
        {
            var ready = true;
            if (clientGotAck?.Invoke(client) != true)
            {
                if (beginUserInfoAckService?.Invoke(client) == true)
                    commitUserInfoAckService?.Invoke(client);
                ready = false;
            }
            if (hasMapLoadInfo?.Invoke(client) != true)
            {
                if (beginMapLoadService?.Invoke(client) == true)
                {
                    appendMapLoadInfo?.Invoke(client);
                    commitMapLoadService?.Invoke(client);
                }
                ready = false;
            }
            if (hasGameInfo?.Invoke(client) != true)
            {
                if (beginGameInfoService?.Invoke(client) == true)
                {
                    appendGameInfo?.Invoke(client);
                    commitGameInfoService?.Invoke(client);
                }
                ready = false;
            }
            if (hasRosterInfo?.Invoke(client) != true)
            {
                queueRosterService?.Invoke(client);
                ready = false;
            }
            if (isRuntimeJoin?.Invoke(client) == true && hasBootstrapInfo?.Invoke(client) != true)
            {
                queueBootstrapService?.Invoke(client);
                ready = false;
            }
            if (ready)
            {
                markClientReady?.Invoke(client);
                netClientUpdated?.Invoke(client);
            }
            flushReliableServices?.Invoke(client);
            return;
        }
        if (status == statusReady)
        {
            if (beginStartGameService?.Invoke(client) == true)
                commitStartGameService?.Invoke(client);
        }
        flushReliableServices?.Invoke(client);
    }
}
