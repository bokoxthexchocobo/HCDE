namespace HCDE.Net.Core;

/// <summary>Runtime connection packet handling (<c>i_net.cpp</c> 2920–3052).</summary>
public static class VmINetIncomingConnection
{
    public static void HandleIncomingConnectionNative(HandleIncomingConnectionContext ctx)
    {
        if (ctx.IsLocalHcdeServiceAuthority?.Invoke() != true)
        {
            ctx.HandleGuestRuntimeHcdeService?.Invoke();
            return;
        }

        if (ctx.RemoteClient < 0 || ctx.RemoteClient >= ctx.MaxClients)
            return;

        var remote = ctx.RemoteClient;
        if (ctx.NetBuffer.Length < 2 || ctx.NetBuffer[1] != ctx.PreHcdeService)
            goto driveSetup;

        if (remote <= 0 || ctx.IsHcdeConnect?.Invoke(remote) != true)
        {
            ctx.DebugMark?.Invoke($"ignored runtime HCDE service packet from unnegotiated client {remote}");
            return;
        }

        if (ctx.NetBufferLength < ctx.HcdeServiceHeaderSize)
        {
            ctx.DebugMark?.Invoke($"runtime HCDE service packet too short len={ctx.NetBufferLength}");
            return;
        }

        var service = ctx.NetBuffer[2];
        switch (service)
        {
        case 2: // HPS_CLIENT_USER_INFO
            if (ctx.GetStatus?.Invoke(remote) != ctx.CstatConnecting)
                break;
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host runtime service userinfo") != true)
                break;
            ctx.ReadUserInfo?.Invoke(remote, ctx.HcdeServiceHeaderSize);
            ctx.SetStatus?.Invoke(remote, ctx.CstatWaiting);
            ctx.NetClientConnected?.Invoke(remote, 16);
            break;
        case 3: // HPS_USER_INFO_ACK
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize + 1, "host runtime service userinfo ack") != true)
                break;
            var slot = ctx.NetBuffer[ctx.HcdeServiceHeaderSize];
            if (slot >= ctx.MaxClients)
                break;
            ctx.SetClientAck?.Invoke(remote, slot, true);
            break;
        case 5: // HPS_GAME_INFO_ACK
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host runtime service gameinfo ack") == true)
                ctx.SetHasGameInfo?.Invoke(remote, true);
            break;
        case 10: // HPS_MAP_LOAD_ACK
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host runtime service mapload ack") == true)
                ctx.SetHasMapLoadInfo?.Invoke(remote, true);
            break;
        case 12: // HPS_ROSTER_ACK
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host runtime service roster ack") == true)
                ctx.SetHasRosterInfo?.Invoke(remote, true);
            break;
        case 14: // HPS_BOOTSTRAP_ACK
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host runtime service bootstrap ack") == true)
            {
                if (ctx.HasBootstrapInfo?.Invoke(remote) != true)
                    ctx.BeginRuntimeBootstrap?.Invoke(remote, "hcde-service-bootstrap");
                ctx.SetHasBootstrapInfo?.Invoke(remote, true);
            }
            break;
        case 15: // HPS_RESYNC_REQUEST
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host runtime service resync request") == true)
            {
                ctx.RequestRuntimeResync?.Invoke(remote, "hcde-service-request");
                ctx.QueueBootstrapResync?.Invoke(remote);
            }
            break;
        case 17: // HPS_RESYNC_ACK
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host runtime service resync ack") == true)
                ctx.DebugMark?.Invoke($"runtime resync ack client={remote}");
            break;
        case 11: // HPS_START_GAME_ACK
            if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host runtime service start ack") == true)
                ctx.SetHasStartGameAck?.Invoke(remote, true);
            break;
        case 1: // HPS_HEARTBEAT
            ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host runtime service heartbeat");
            break;
        default:
            ctx.IncrementUnsupported?.Invoke();
            ctx.DebugMark?.Invoke($"ignored unsupported runtime HCDE service {service}");
            break;
        }

    driveSetup:
        VmINetRuntimeSetup.DriveRuntimeSetupStateForClientNative(
            remote,
            ctx.CountConnectedPlayers?.Invoke() ?? 0,
            ctx.MaxClients,
            ctx.CstatConnecting,
            ctx.CstatWaiting,
            ctx.CstatReady,
            ctx.CstatNone,
            ctx.GetStatus,
            ctx.ClientGotAck,
            ctx.BeginConnectAckResend,
            ctx.SendConnectAck,
            ctx.BeginConsolePlayerService,
            ctx.WriteConsolePlayerPayload,
            ctx.CommitConsolePlayerService,
            ctx.FlushReliableServices,
            ctx.BeginUserInfoAckService,
            ctx.CommitUserInfoAckService,
            ctx.HasMapLoadInfo,
            ctx.BeginMapLoadService,
            ctx.AppendMapLoadInfo,
            ctx.CommitMapLoadService,
            ctx.HasGameInfo,
            ctx.BeginGameInfoService,
            ctx.AppendGameInfo,
            ctx.CommitGameInfoService,
            ctx.HasRosterInfo,
            ctx.QueueRosterService,
            ctx.IsRuntimeJoin,
            ctx.HasBootstrapInfo,
            ctx.QueueBootstrapService,
            ctx.MarkClientReady,
            ctx.NetClientUpdated,
            ctx.BeginStartGameService,
            ctx.CommitStartGameService);
    }

    public static void HandleIncomingConnectionMaintenanceNative(HandleIncomingConnectionMaintenanceContext ctx)
    {
        if (ctx.IsLocalHcdeServiceAuthority?.Invoke() != true)
            return;

        var connectedPlayers = ctx.CountConnectedPlayers?.Invoke() ?? 0;
        for (var client = 1; client < ctx.MaxClients; client++)
        {
            if (ctx.GetStatus?.Invoke(client) == ctx.CstatNone || ctx.IsHcdeConnect?.Invoke(client) != true)
                continue;

            var runtimeSetupInProgress = ctx.GetStatus?.Invoke(client) != ctx.CstatReady
                || ctx.HasRosterInfo?.Invoke(client) != true
                || ctx.HasStartGameAck?.Invoke(client) != true
                || ctx.HasPendingReliableService?.Invoke(client) == true;
            if (!runtimeSetupInProgress)
                continue;

            var drop = ctx.DropClientForHcdeTimeout?.Invoke(client, connectedPlayers, "runtime setup maintenance");
            if (drop is { Dropped: true } dropped)
            {
                connectedPlayers = dropped.ConnectedPlayers;
                continue;
            }

            VmINetRuntimeSetup.DriveRuntimeSetupStateForClientNative(
                client,
                connectedPlayers,
                ctx.MaxClients,
                ctx.CstatConnecting,
                ctx.CstatWaiting,
                ctx.CstatReady,
                ctx.CstatNone,
                ctx.GetStatus,
                ctx.ClientGotAck,
                ctx.BeginConnectAckResend,
                ctx.SendConnectAck,
                ctx.BeginConsolePlayerService,
                ctx.WriteConsolePlayerPayload,
                ctx.CommitConsolePlayerService,
                ctx.FlushReliableServices,
                ctx.BeginUserInfoAckService,
                ctx.CommitUserInfoAckService,
                ctx.HasMapLoadInfo,
                ctx.BeginMapLoadService,
                ctx.AppendMapLoadInfo,
                ctx.CommitMapLoadService,
                ctx.HasGameInfo,
                ctx.BeginGameInfoService,
                ctx.AppendGameInfo,
                ctx.CommitGameInfoService,
                ctx.HasRosterInfo,
                ctx.QueueRosterService,
                ctx.IsRuntimeJoin,
                ctx.HasBootstrapInfo,
                ctx.QueueBootstrapService,
                ctx.MarkClientReady,
                ctx.NetClientUpdated,
                ctx.BeginStartGameService,
                ctx.CommitStartGameService);
        }
    }
}

public sealed class HandleIncomingConnectionContext
{
    public int RemoteClient { get; init; }
    public int MaxClients { get; init; }
    public int NetBufferLength { get; init; }
    public byte[] NetBuffer { get; init; } = [];
    public byte PreHcdeService { get; init; }
    public int HcdeServiceHeaderSize { get; init; }
    public byte CstatNone { get; init; }
    public byte CstatConnecting { get; init; }
    public byte CstatWaiting { get; init; }
    public byte CstatReady { get; init; }
    public Func<bool>? IsLocalHcdeServiceAuthority { get; init; }
    public Action? HandleGuestRuntimeHcdeService { get; init; }
    public Func<int, bool>? IsHcdeConnect { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
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
    public Action<int, bool>? SetHasStartGameAck { get; init; }
    public Action? IncrementUnsupported { get; init; }
    public Func<int>? CountConnectedPlayers { get; init; }
    public Func<int, bool>? ClientGotAck { get; init; }
    public Func<int, bool>? BeginConnectAckResend { get; init; }
    public Action<int, int, int, byte>? SendConnectAck { get; init; }
    public Func<int, bool>? BeginConsolePlayerService { get; init; }
    public Action<int, int, int, byte>? WriteConsolePlayerPayload { get; init; }
    public Func<int, bool>? CommitConsolePlayerService { get; init; }
    public Action<int>? FlushReliableServices { get; init; }
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
    public Action<int>? MarkClientReady { get; init; }
    public Action<int>? NetClientUpdated { get; init; }
    public Func<int, bool>? BeginStartGameService { get; init; }
    public Func<int, bool>? CommitStartGameService { get; init; }
}

public sealed class HandleIncomingConnectionMaintenanceContext
{
    public int MaxClients { get; init; }
    public byte CstatNone { get; init; }
    public byte CstatReady { get; init; }
    public byte CstatConnecting { get; init; }
    public byte CstatWaiting { get; init; }
    public Func<bool>? IsLocalHcdeServiceAuthority { get; init; }
    public Func<int>? CountConnectedPlayers { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Func<int, bool>? IsHcdeConnect { get; init; }
    public Func<int, bool>? HasRosterInfo { get; init; }
    public Func<int, bool>? HasStartGameAck { get; init; }
    public Func<int, bool>? HasPendingReliableService { get; init; }
    public Func<int, int, string, (bool Dropped, int ConnectedPlayers)>? DropClientForHcdeTimeout { get; init; }
    public Func<int, bool>? ClientGotAck { get; init; }
    public Func<int, bool>? BeginConnectAckResend { get; init; }
    public Action<int, int, int, byte>? SendConnectAck { get; init; }
    public Func<int, bool>? BeginConsolePlayerService { get; init; }
    public Action<int, int, int, byte>? WriteConsolePlayerPayload { get; init; }
    public Func<int, bool>? CommitConsolePlayerService { get; init; }
    public Action<int>? FlushReliableServices { get; init; }
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
    public Func<int, bool>? QueueRosterService { get; init; }
    public Func<int, bool>? IsRuntimeJoin { get; init; }
    public Func<int, bool>? HasBootstrapInfo { get; init; }
    public Func<int, bool>? QueueBootstrapService { get; init; }
    public Action<int>? MarkClientReady { get; init; }
    public Action<int>? NetClientUpdated { get; init; }
    public Func<int, bool>? BeginStartGameService { get; init; }
    public Func<int, bool>? CommitStartGameService { get; init; }
}
