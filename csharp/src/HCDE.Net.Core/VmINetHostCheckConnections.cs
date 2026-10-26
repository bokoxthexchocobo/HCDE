namespace HCDE.Net.Core;

/// <summary>Pregame host connection loop (<c>i_net.cpp</c> 3054–3364).</summary>
public static class VmINetHostCheckConnections
{
    public static bool ApplyDedicatedAutostart(
        HostCheckForConnectionsContext ctx,
        int connectedPlayers,
        bool forceStarting)
    {
        if (ctx.DedicatedServerMode && !forceStarting && ctx.SvDedicatedAutostart && connectedPlayers > 1)
        {
            ctx.SetDedicatedServerStartRequested?.Invoke(true);
            forceStarting = true;
            ctx.Printf?.Invoke($"NetServer:: Auto-start requested ({Math.Max(connectedPlayers - 1, 0)}/{Math.Max(ctx.MaxClients - 1, 0)} playable clients connected).");
        }

        if (ctx.DedicatedServerMode && forceStarting && connectedPlayers <= 1)
        {
            ctx.SetDedicatedServerStartRequested?.Invoke(false);
            forceStarting = false;
            ctx.Printf?.Invoke("NetServer:: Start requested, but no playable clients are connected yet.");
        }

        return forceStarting;
    }

    public static int ProcessKickClients(HostCheckForConnectionsContext ctx, int connectedPlayers)
    {
        foreach (var client in ctx.GetKickClients?.Invoke() ?? [])
        {
            if (client <= 0 || client >= ctx.MaxClients || client >= ctx.MaxPlayers
                || ctx.GetStatus?.Invoke(client) == ctx.CstatNone)
                continue;
            var booted = ctx.GetClientAddress?.Invoke(client) ?? default;
            ctx.RemoveClientConnection?.Invoke(client, "kicked during setup");
            connectedPlayers--;
            ctx.UpdatePlayers?.Invoke(connectedPlayers, ctx.MaxClients);
            ctx.RejectConnection?.Invoke(booted, ctx.PreKicked);
        }

        return connectedPlayers;
    }

    public static int ProcessBanClients(HostCheckForConnectionsContext ctx, int connectedPlayers)
    {
        foreach (var client in ctx.GetBanClients?.Invoke() ?? [])
        {
            if (client <= 0 || client >= ctx.MaxClients || client >= ctx.MaxPlayers
                || ctx.GetStatus?.Invoke(client) == ctx.CstatNone)
                continue;
            var booted = ctx.GetClientAddress?.Invoke(client) ?? default;
            ctx.PushBannedConnection?.Invoke(booted);
            ctx.RemoveClientConnection?.Invoke(client, "banned during setup");
            connectedPlayers--;
            ctx.UpdatePlayers?.Invoke(connectedPlayers, ctx.MaxClients);
            ctx.RejectConnection?.Invoke(booted, ctx.PreBanned);
        }

        return connectedPlayers;
    }

    public static bool ShouldIgnoreHcdeServicePacket(HostCheckForConnectionsContext ctx, int remote) =>
        remote < 0 || remote >= ctx.MaxClients || ctx.IsHcdeConnect?.Invoke(remote) != true;

    public static void ProcessHcdeServicePacket(HostCheckForConnectionsContext ctx, int remote)
    {
        if (ctx.NetBufferLength < ctx.HcdeServiceHeaderSize)
        {
            ctx.DebugMark?.Invoke($"host HCDE service packet too short len={ctx.NetBufferLength}");
            return;
        }

        var service = ctx.NetBuffer[2];
        switch (service)
        {
        case 2:
            if (ctx.GetStatus?.Invoke(remote) != ctx.CstatConnecting)
                break;
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service userinfo") != true)
                break;
            ctx.ReadUserInfo?.Invoke(remote, ctx.HcdeServiceHeaderSize);
            ctx.SetStatus?.Invoke(remote, ctx.CstatWaiting);
            ctx.NetClientConnected?.Invoke(remote, 16);
            break;
        case 3:
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize + 1, "host service userinfo ack") != true)
                break;
            var slot = ctx.NetBuffer[ctx.HcdeServiceHeaderSize];
            if (slot >= ctx.MaxClients || slot >= ctx.MaxPlayers)
                break;
            ctx.SetClientAck?.Invoke(remote, slot, true);
            break;
        case 5:
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service gameinfo ack") == true)
                ctx.SetHasGameInfo?.Invoke(remote, true);
            break;
        case 10:
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service mapload ack") == true)
                ctx.SetHasMapLoadInfo?.Invoke(remote, true);
            break;
        case 12:
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service roster ack") == true)
                ctx.SetHasRosterInfo?.Invoke(remote, true);
            break;
        case 14:
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service bootstrap ack") == true)
            {
                if (ctx.HasBootstrapInfo?.Invoke(remote) != true)
                    ctx.BeginRuntimeBootstrap?.Invoke(remote, "hcde-service-bootstrap");
                ctx.SetHasBootstrapInfo?.Invoke(remote, true);
            }
            break;
        case 15:
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service resync request") == true)
            {
                ctx.RequestRuntimeResync?.Invoke(remote, "hcde-service-request");
                ctx.QueueBootstrapResync?.Invoke(remote);
            }
            break;
        case 17:
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service resync ack") == true)
                ctx.DebugMark?.Invoke($"resync ack client={remote}");
            break;
        case 11:
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service start ack") == true)
            {
                if (ctx.HasStartGameAck?.Invoke(remote) != true)
                    ctx.NetLog?.Invoke(remote, "acknowledged HCDE service start");
                ctx.SetHasStartGameAck?.Invoke(remote, true);
            }
            break;
        case 1:
            ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service heartbeat");
            break;
        default:
            ctx.IncrementUnsupported?.Invoke();
            ctx.DebugMark?.Invoke($"ignored unsupported host HCDE service {service}");
            break;
        }
    }

    public static int ProcessInboundSetupPackets(
        HostCheckForConnectionsContext ctx,
        int connectedPlayers,
        bool hasPassword,
        bool forceStarting)
    {
        HostGetConnectionResult conn;
        while ((conn = ctx.GetConnection?.Invoke() ?? default).GotPacket)
        {
            _ = conn.From;
            if (ctx.NetBuffer[0] == ctx.NcmdExit)
            {
                if (ctx.RemoteClient >= 0)
                {
                    ctx.RemoveClientConnection?.Invoke(ctx.RemoteClient, "exit packet during setup");
                    connectedPlayers--;
                    ctx.UpdatePlayers?.Invoke(connectedPlayers, ctx.MaxClients);
                }

                continue;
            }

            if (ctx.NetBuffer[0] != ctx.NcmdSetup)
                continue;
            if (ctx.NetBuffer[1] == ctx.PreConnect)
            {
                if (ctx.RemoteClient >= 0)
                    continue;
                var setup = ctx.TryProcessSetupConnect?.Invoke(conn.From, hasPassword, forceStarting, false, connectedPlayers);
                if (setup is not null)
                    connectedPlayers = setup.Value;
            }
            else if (ctx.NetBuffer[1] == ctx.PreHcdeService)
            {
                var remote = ctx.RemoteClient;
                if (ShouldIgnoreHcdeServicePacket(ctx, remote))
                {
                    ctx.DebugMark?.Invoke($"ignored HCDE service packet from unnegotiated client {remote}");
                    continue;
                }

                ProcessHcdeServicePacket(ctx, remote);
            }
        }

        return connectedPlayers;
    }

    public static bool DriveWaitingClient(HostCheckForConnectionsContext ctx, int client)
    {
        var clientReady = true;
        if (ctx.ClientGotAck?.Invoke(client, client) != true)
        {
            if (ctx.BeginUserInfoAckService?.Invoke(client) == true)
                ctx.CommitUserInfoAckService?.Invoke(client);
            clientReady = false;
        }

        if (ctx.HasMapLoadInfo?.Invoke(client) != true)
        {
            if (ctx.BeginMapLoadService?.Invoke(client) == true)
            {
                ctx.AppendMapLoadInfo?.Invoke(client);
                ctx.CommitMapLoadService?.Invoke(client);
            }

            clientReady = false;
        }

        if (ctx.HasGameInfo?.Invoke(client) != true)
        {
            if (ctx.BeginGameInfoService?.Invoke(client) == true)
            {
                ctx.AppendGameInfo?.Invoke(client);
                ctx.CommitGameInfoService?.Invoke(client);
            }

            clientReady = false;
        }

        if (ctx.HasRosterInfo?.Invoke(client) != true)
        {
            ctx.QueueRosterService?.Invoke(client);
            clientReady = false;
        }

        if (ctx.IsRuntimeJoin?.Invoke(client) == true && ctx.HasBootstrapInfo?.Invoke(client) != true)
        {
            ctx.QueueBootstrapService?.Invoke(client);
            clientReady = false;
        }

        if (clientReady)
        {
            ctx.SetStatus?.Invoke(client, ctx.CstatReady);
            ctx.NetClientUpdated?.Invoke(client);
        }

        return clientReady;
    }

    public static bool DriveClientSetupState(
        HostCheckForConnectionsContext ctx,
        int client,
        ref int connectedPlayers,
        bool forceStarting,
        ref bool ready)
    {
        var drop = ctx.DropClientForHcdeTimeout?.Invoke(client, connectedPlayers, "pregame setup");
        if (drop is { Dropped: true } dropped)
        {
            connectedPlayers = dropped.ConnectedPlayers;
            ready = false;
            return false;
        }

        ctx.FlushReliableServices?.Invoke(client);
        var status = ctx.GetStatus?.Invoke(client) ?? ctx.CstatNone;
        if (status != ctx.CstatReady && (!forceStarting || status != ctx.CstatNone))
            ready = false;

        if (status == ctx.CstatConnecting)
        {
            if (ctx.ShouldResendConnectAck?.Invoke(client) == true)
                ctx.SendConnectAckPacket?.Invoke(client, connectedPlayers);
            if (ctx.BeginConsolePlayerService?.Invoke(client) == true)
            {
                ctx.WriteConsolePlayerPayload?.Invoke(client, connectedPlayers);
                ctx.CommitConsolePlayerService?.Invoke(client);
            }
        }
        else if (status == ctx.CstatWaiting)
        {
            if (!DriveWaitingClient(ctx, client))
                ready = false;
        }
        else if (status == ctx.CstatReady)
        {
            if (ctx.HasPendingReliableService?.Invoke(client) != true)
                ctx.SendHeartbeat?.Invoke(client, connectedPlayers);
        }

        return true;
    }

    public static bool ComputeShouldStartGame(
        HostCheckForConnectionsContext ctx,
        bool ready,
        int connectedPlayers,
        bool forceStarting)
    {
        var shouldStart = ready && (connectedPlayers >= ctx.MaxClients || forceStarting);
        if (shouldStart && forceStarting)
            ctx.SetDedicatedServerStartRequested?.Invoke(false);
        return shouldStart;
    }

    public static bool HostCheckForConnectionsNative(HostCheckForConnectionsContext ctx)
    {
        var hasPassword = ctx.HasNetPassword?.Invoke() == true;
        var connectedPlayers = ctx.ConnectedPlayers;
        var forceStarting = ctx.ShouldStartNetGame?.Invoke() == true;
        forceStarting = ApplyDedicatedAutostart(ctx, connectedPlayers, forceStarting);
        connectedPlayers = ProcessKickClients(ctx, connectedPlayers);
        connectedPlayers = ProcessBanClients(ctx, connectedPlayers);
        connectedPlayers = ProcessInboundSetupPackets(ctx, connectedPlayers, hasPassword, forceStarting);

        ctx.UpdateMaster?.Invoke();
        ctx.NetBuffer[0] = ctx.NcmdSetup;
        var ready = true;
        for (var client = 1; client < ctx.MaxClients; client++)
            DriveClientSetupState(ctx, client, ref connectedPlayers, forceStarting, ref ready);

        ctx.ConnectedPlayers = connectedPlayers;
        return ComputeShouldStartGame(ctx, ready, connectedPlayers, forceStarting);
    }
}
