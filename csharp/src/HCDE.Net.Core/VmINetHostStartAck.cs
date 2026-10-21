namespace HCDE.Net.Core;

/// <summary>Start-game ack wait loop and abort send (<c>i_net.cpp</c> 3366–3460).</summary>
public static class VmINetHostStartAck
{
    public static bool HostCheckStartGameAcksNative(HostCheckStartGameAcksContext ctx)
    {
        var connectedPlayers = ctx.ConnectedPlayers;
        HostGetConnectionResult conn;
        while ((conn = ctx.GetConnection?.Invoke() ?? default).GotPacket)
        {
            if (ctx.NetBuffer[0] == ctx.NcmdExit)
            {
                if (ctx.RemoteClient > 0)
                {
                    ctx.RemoveClientConnection?.Invoke(ctx.RemoteClient, "exit packet while waiting for start acknowledgement");
                    connectedPlayers--;
                    ctx.UpdatePlayers?.Invoke(connectedPlayers, ctx.MaxClients);
                }
                continue;
            }
            if (ctx.NetBuffer[0] != ctx.NcmdSetup || ctx.NetBuffer[1] != ctx.PreHcdeService)
                continue;
            var remote = ctx.RemoteClient;
            if (remote <= 0 || ctx.IsHcdeConnect?.Invoke(remote) != true)
            {
                ctx.DebugMark?.Invoke($"ignored HCDE start-ack packet from unnegotiated client {remote}");
                continue;
            }
            var service = ctx.NetBuffer[2];
            switch (service)
            {
            case 11:
                if (ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service start ack") == true)
                {
                    if (ctx.HasStartGameAck?.Invoke(remote) != true)
                        ctx.NetLog?.Invoke(remote, "acknowledged HCDE service start");
                    ctx.SetHasStartGameAck?.Invoke(remote, true);
                }
                break;
            case 1:
                ctx.CheckHcdePregameService?.Invoke(remote, ctx.HcdeServiceHeaderSize, "host service start heartbeat");
                break;
            default:
                ctx.DebugMark?.Invoke($"ignored HCDE service {service} while waiting for start ack");
                break;
            }
        }

        var allAcked = true;
        var acknowledged = 1;
        for (var client = 1; client < ctx.MaxClients; client++)
        {
            if (ctx.GetStatus?.Invoke(client) == ctx.CstatNone)
                continue;
            var drop = ctx.DropClientForHcdeTimeout?.Invoke(client, connectedPlayers, "start acknowledgement");
            if (drop is { Dropped: true } dropped)
            {
                connectedPlayers = dropped.ConnectedPlayers;
                continue;
            }
            ctx.FlushReliableServices?.Invoke(client);
            if (ctx.HasStartGameAck?.Invoke(client) != true)
            {
                allAcked = false;
                continue;
            }
            acknowledged++;
        }
        ctx.UpdatePlayers?.Invoke(acknowledged, connectedPlayers);
        ctx.ConnectedPlayers = connectedPlayers;
        return allAcked;
    }

    public static void SendAbortNative(SendAbortContext ctx)
    {
        ctx.NetBuffer[0] = ctx.NcmdExit;
        if (ctx.ConsolePlayer == 0)
        {
            ctx.NetBuffer[1] = 0;
            ctx.NetBufferLength = 2;
            for (var client = 1; client < ctx.MaxClients; client++)
            {
                if (ctx.GetStatus?.Invoke(client) != ctx.CstatNone)
                    ctx.SendPacket?.Invoke(client);
            }
        }
        else
        {
            ctx.NetBufferLength = 1;
            ctx.SendPacket?.Invoke(0);
        }
    }

    public static bool HostGameThroughNetMessageNative(HostGameContext ctx)
    {
        ctx.DebugMark?.Invoke($"host request arg={ctx.Arg}");
        ctx.SetDedicatedServerStartRequested?.Invoke(false);
        if (ctx.DedicatedServerAbortRequested)
        {
            ctx.DebugMark?.Invoke("host request cancelled before network start");
            ctx.ExitHost?.Invoke(0);
            return false;
        }
        var requestedClients = 0;
        if (ctx.Arg < (ctx.NumArgs?.Invoke() ?? 0))
        {
            var raw = ctx.GetArg?.Invoke(ctx.Arg);
            var parsed = ctx.TryParseClientCount?.Invoke(raw);
            if (parsed is { Ok: false })
            {
                ctx.DebugWarning?.Invoke($"invalid host client count '{raw ?? "<null>"}', using default");
                requestedClients = 0;
            }
            else
                requestedClients = parsed?.Value ?? 0;
        }
        var maxClients = ctx.MaxClients;
        if (ctx.DedicatedServerMode)
        {
            var playableSlots = requestedClients > 0 ? requestedClients : 0;
            if (ctx.SvMaxPlayers > 0)
                playableSlots = Math.Max(playableSlots, ctx.SvMaxPlayers);
            if (playableSlots <= 0)
                playableSlots = 1;
            if (playableSlots >= ctx.MaxPlayers)
                ctx.FatalError?.Invoke($"Cannot host a dedicated game with {playableSlots} client slots.");
            maxClients = playableSlots + 1;
        }
        else if (requestedClients == 0)
            maxClients = 2;
        else
            maxClients = requestedClients;
        if (maxClients > ctx.MaxPlayers)
            ctx.FatalError?.Invoke($"Cannot host a game with {maxClients} players.");
        ctx.SetMaxClients?.Invoke(maxClients);
        ctx.DebugMark?.Invoke($"host slots resolved max={maxClients} consoleplayer={ctx.ConsolePlayer}");
        ctx.SetNetworkDetails?.Invoke(maxClients, ctx.DedicatedServerMode ? "server-init" : "host-init");
        ctx.GenerateGameId?.Invoke();
        ctx.DebugMark?.Invoke("host gameid generated");
        ctx.AddNetworkClient?.Invoke(0);
        ctx.SetHostReady?.Invoke(ctx.ConsolePlayer);
        ctx.SetupUserInfo?.Invoke();
        ctx.DebugMark?.Invoke("host userinfo ready");
        if (maxClients == 1)
        {
            ctx.SetTicDup?.Invoke(1);
            ctx.SetMultiplayer?.Invoke(true);
            return true;
        }
        ctx.DebugMark?.Invoke($"host starting network port={ctx.GamePort}");
        ctx.StartNetwork?.Invoke(false);
        ctx.DebugMark?.Invoke($"host network ready port={ctx.GamePort}");
        ctx.InitMasters?.Invoke();
        ctx.DebugMark?.Invoke("host masters initialized");
        ctx.SetMasterState?.Invoke();
        ctx.SetNetworkDetails?.Invoke(maxClients, ctx.DedicatedServerMode ? "server-waiting" : "host-waiting");
        ctx.PrintDiagnostics?.Invoke();
        ctx.SetConnectFlow?.Invoke();
        ctx.NetInit?.Invoke(ctx.DedicatedServerMode ? "Starting dedicated server..." : "Hosting game...");
        ctx.UpdatePlayers?.Invoke(1, maxClients);
        ctx.NetClientConnected?.Invoke(0, 16);
        ctx.NetMessage?.Invoke(ctx.DedicatedServerMode ? "Dedicated server accepting clients" : "Waiting for players");
        return true;
    }
}

public sealed class HostCheckStartGameAcksContext
{
    public int ConnectedPlayers { get; set; }
    public int MaxClients { get; init; }
    public int RemoteClient { get; init; }
    public byte[] NetBuffer { get; init; } = [];
    public byte NcmdExit { get; init; }
    public byte NcmdSetup { get; init; }
    public byte PreHcdeService { get; init; }
    public byte CstatNone { get; init; }
    public int HcdeServiceHeaderSize { get; init; }
    public Func<HostGetConnectionResult>? GetConnection { get; init; }
    public Action<int, string>? RemoveClientConnection { get; init; }
    public Action<int, int>? UpdatePlayers { get; init; }
    public Func<int, bool>? IsHcdeConnect { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Func<int, int, string, bool>? CheckHcdePregameService { get; init; }
    public Func<int, bool>? HasStartGameAck { get; init; }
    public Action<int, string>? NetLog { get; init; }
    public Action<int, bool>? SetHasStartGameAck { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Func<int, int, string, (bool Dropped, int ConnectedPlayers)>? DropClientForHcdeTimeout { get; init; }
    public Action<int>? FlushReliableServices { get; init; }
}

public sealed class HostGameContext
{
    public int Arg { get; init; }
    public int ConsolePlayer { get; init; }
    public int MaxPlayers { get; init; }
    public int MaxClients { get; set; }
    public bool DedicatedServerMode { get; init; }
    public bool DedicatedServerAbortRequested { get; init; }
    public int SvMaxPlayers { get; init; }
    public uint GamePort { get; init; }
    public Func<int>? NumArgs { get; init; }
    public Func<int, string?>? GetArg { get; init; }
    public Func<string?, (bool Ok, int Value)>? TryParseClientCount { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Action<string>? DebugWarning { get; init; }
    public Action<bool>? SetDedicatedServerStartRequested { get; init; }
    public Action<int>? ExitHost { get; init; }
    public Action<int>? SetMaxClients { get; init; }
    public Action<string>? FatalError { get; init; }
    public Action<int, string>? SetNetworkDetails { get; init; }
    public Action? GenerateGameId { get; init; }
    public Action<int>? AddNetworkClient { get; init; }
    public Action<int>? SetHostReady { get; init; }
    public Action? SetupUserInfo { get; init; }
    public Action<uint>? SetTicDup { get; init; }
    public Action<bool>? SetMultiplayer { get; init; }
    public Action<bool>? StartNetwork { get; init; }
    public Action? InitMasters { get; init; }
    public Action? SetMasterState { get; init; }
    public Action? PrintDiagnostics { get; init; }
    public Action? SetConnectFlow { get; init; }
    public Action<string>? NetInit { get; init; }
    public Action<int, int>? UpdatePlayers { get; init; }
    public Action<uint, uint>? NetClientConnected { get; init; }
    public Action<string>? NetMessage { get; init; }
}

public sealed class SendAbortContext
{
    public byte[] NetBuffer { get; init; } = [];
    public int NetBufferLength { get; set; }
    public int ConsolePlayer { get; init; }
    public int MaxClients { get; init; }
    public byte NcmdExit { get; init; }
    public byte CstatNone { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Action<int>? SendPacket { get; init; }
}
