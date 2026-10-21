namespace HCDE.Net.Core;

/// <summary>Pregame host connection loop (<c>i_net.cpp</c> 3054–3364).</summary>
public static class VmINetHostCheckConnections
{
    public static bool HostCheckForConnectionsNative(HostCheckForConnectionsContext ctx)
    {
        var hasPassword = ctx.HasNetPassword?.Invoke() == true;
        var connectedPlayers = ctx.ConnectedPlayers;
        var forceStarting = ctx.ShouldStartNetGame?.Invoke() == true;
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

        HostGetConnectionResult conn;
        while ((conn = ctx.GetConnection?.Invoke() ?? default).GotPacket)
        {
            var from = conn.From;
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
                var setup = ctx.TryProcessSetupConnect?.Invoke(from, hasPassword, forceStarting, false, connectedPlayers);
                if (setup is not null)
                    connectedPlayers = setup.Value;
            }
            else if (ctx.NetBuffer[1] == ctx.PreHcdeService)
            {
                ProcessHostHcdeServicePacket(ctx, ref connectedPlayers);
            }
        }

        ctx.UpdateMaster?.Invoke();
        var ready = true;
        ctx.NetBuffer[0] = ctx.NcmdSetup;
        for (var client = 1; client < ctx.MaxClients; client++)
        {
            var drop = ctx.DropClientForHcdeTimeout?.Invoke(client, connectedPlayers, "pregame setup");
            if (drop is { Dropped: true } dropped)
            {
                connectedPlayers = dropped.ConnectedPlayers;
                ready = false;
                continue;
            }
            ctx.FlushReliableServices?.Invoke(client);
            var status = ctx.GetStatus?.Invoke(client) ?? ctx.CstatNone;
            if (status != ctx.CstatReady && (!forceStarting || status != ctx.CstatNone))
                ready = false;
            if (status == ctx.CstatConnecting)
                DriveConnectingClient(ctx, client, ref connectedPlayers);
            else if (status == ctx.CstatWaiting)
            {
                if (!DriveWaitingClient(ctx, client))
                    ready = false;
            }
            else if (status == ctx.CstatReady)
                DriveReadyClient(ctx, client, connectedPlayers);
        }

        var shouldStart = ready && (connectedPlayers >= ctx.MaxClients || forceStarting);
        if (shouldStart && forceStarting)
            ctx.SetDedicatedServerStartRequested?.Invoke(false);
        ctx.ConnectedPlayers = connectedPlayers;
        return shouldStart;
    }

    static void ProcessHostHcdeServicePacket(HostCheckForConnectionsContext ctx, ref int connectedPlayers)
    {
        var remote = ctx.RemoteClient;
        if (remote < 0 || remote >= ctx.MaxClients || ctx.IsHcdeConnect?.Invoke(remote) != true)
        {
            ctx.DebugMark?.Invoke($"ignored HCDE service packet from unnegotiated client {remote}");
            return;
        }
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

    static void DriveConnectingClient(HostCheckForConnectionsContext ctx, int client, ref int connectedPlayers)
    {
        if (ctx.ShouldResendConnectAck?.Invoke(client) == true)
        {
            ctx.SendConnectAckPacket?.Invoke(client, connectedPlayers);
        }
        if (ctx.BeginConsolePlayerService?.Invoke(client) == true)
        {
            ctx.WriteConsolePlayerPayload?.Invoke(client, connectedPlayers);
            ctx.CommitConsolePlayerService?.Invoke(client);
        }
    }

    static bool DriveWaitingClient(HostCheckForConnectionsContext ctx, int client)
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

    static void DriveReadyClient(HostCheckForConnectionsContext ctx, int client, int connectedPlayers)
    {
        if (ctx.HasPendingReliableService?.Invoke(client) == true)
            return;
        ctx.SendHeartbeat?.Invoke(client, connectedPlayers);
    }
}

public sealed class HostCheckForConnectionsContext
{
    public int ConnectedPlayers { get; set; }
    public int MaxClients { get; init; }
    public int MaxPlayers { get; init; }
    public int RemoteClient { get; init; }
    public int NetBufferLength { get; init; }
    public byte[] NetBuffer { get; init; } = [];
    public bool DedicatedServerMode { get; init; }
    public bool SvDedicatedAutostart { get; init; }
    public byte NcmdSetup { get; init; }
    public byte NcmdExit { get; init; }
    public byte PreConnect { get; init; }
    public byte PreHcdeService { get; init; }
    public byte PreKicked { get; init; }
    public byte PreBanned { get; init; }
    public byte CstatNone { get; init; }
    public byte CstatConnecting { get; init; }
    public byte CstatWaiting { get; init; }
    public byte CstatReady { get; init; }
    public int HcdeServiceHeaderSize { get; init; }
    public Func<bool>? HasNetPassword { get; init; }
    public Func<bool>? ShouldStartNetGame { get; init; }
    public Action<bool>? SetDedicatedServerStartRequested { get; init; }
    public Action<string>? Printf { get; init; }
    public Func<IReadOnlyList<int>>? GetKickClients { get; init; }
    public Func<IReadOnlyList<int>>? GetBanClients { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Func<int, (uint, ushort)>? GetClientAddress { get; init; }
    public Action<int, string>? RemoveClientConnection { get; init; }
    public Action<int, int>? UpdatePlayers { get; init; }
    public Action<(uint, ushort), byte>? RejectConnection { get; init; }
    public Action<(uint, ushort)>? PushBannedConnection { get; init; }
    public Func<HostGetConnectionResult>? GetConnection { get; init; }
    public Func<(uint, ushort), bool, bool, bool, int, int?>? TryProcessSetupConnect { get; init; }
    public Action? UpdateMaster { get; init; }
    public Func<int, int, string, (bool Dropped, int ConnectedPlayers)>? DropClientForHcdeTimeout { get; init; }
    public Action<int>? FlushReliableServices { get; init; }
    public Func<int, bool>? IsHcdeConnect { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Func<int, int, string, bool>? CheckHcdePregameService { get; init; }
    public Action<int, int>? ReadUserInfo { get; init; }
    public Action<int, byte>? SetStatus { get; init; }
    public Action<int, uint>? NetClientConnected { get; init; }
    public Action<int, int, bool>? SetClientAck { get; init; }
    public Action<int, bool>? SetHasGameInfo { get; init; }
    public Action<int, bool>? SetHasMapLoadInfo { get; init; }
    public Action<int, bool>? SetHasRosterInfo { get; init; }
    public Func<int, bool>? HasBootstrapInfo { get; init; }
    public Action<int, string>? BeginRuntimeBootstrap { get; init; }
    public Action<int, bool>? SetHasBootstrapInfo { get; init; }
    public Action<int, string>? RequestRuntimeResync { get; init; }
    public Action<int>? QueueBootstrapResync { get; init; }
    public Func<int, bool>? HasStartGameAck { get; init; }
    public Action<int, string>? NetLog { get; init; }
    public Action<int, bool>? SetHasStartGameAck { get; init; }
    public Action? IncrementUnsupported { get; init; }
    public Func<int, bool>? ShouldResendConnectAck { get; init; }
    public Action<int, int>? SendConnectAckPacket { get; init; }
    public Func<int, bool>? BeginConsolePlayerService { get; init; }
    public Action<int, int>? WriteConsolePlayerPayload { get; init; }
    public Func<int, bool>? CommitConsolePlayerService { get; init; }
    public Func<int, int, bool>? ClientGotAck { get; init; }
    public Func<int, bool>? BeginUserInfoAckService { get; init; }
    public Func<int, bool>? CommitUserInfoAckService { get; init; }
    public Func<int, bool>? HasMapLoadInfo { get; init; }
    public Func<int, bool>? BeginMapLoadService { get; init; }
    public Action<int>? AppendMapLoadInfo { get; init; }
    public Func<int, bool>? CommitMapLoadService { get; init; }
    public Func<int, bool>? HasGameInfo { get; init; }
    public Func<int, bool>? BeginGameInfoService { get; init; }
    public Action<int>? AppendGameInfo { get; init; }
    public Func<int, bool>? CommitGameInfoService { get; init; }
    public Func<int, bool>? HasRosterInfo { get; init; }
    public Func<int, bool>? QueueRosterService { get; init; }
    public Func<int, bool>? IsRuntimeJoin { get; init; }
    public Func<int, bool>? QueueBootstrapService { get; init; }
    public Action<int>? NetClientUpdated { get; init; }
    public Func<int, bool>? HasPendingReliableService { get; init; }
    public Action<int, int>? SendHeartbeat { get; init; }
}

public readonly struct HostGetConnectionResult(bool gotPacket, (uint, ushort) from)
{
    public bool GotPacket { get; } = gotPacket;
    public (uint, ushort) From { get; } = from;
}
