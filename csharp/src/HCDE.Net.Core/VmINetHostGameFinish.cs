namespace HCDE.Net.Core;

/// <summary>Host session completion and verification decode (<c>i_net.cpp</c> 3545–3717).</summary>
public static class VmINetHostGameFinish
{
    public static bool ClientSlotConnected(byte status, byte cstatNone) => status != cstatNone;

    public static bool SinglePlayerHostImmediateStart(int connectedPlayers) => connectedPlayers == 1;

    public static bool VerificationStreamTooShort(int length, int minimum) => length < minimum;

    public static int ReadVerificationPayloadSize(ReadOnlySpan<byte> stream) =>
        (stream[1] << 24) | (stream[2] << 16) | (stream[3] << 8) | stream[4];

    public static bool DisconnectPrePacketValid(int bufferLength, int minimum) => bufferLength >= minimum;

    public static bool ClientSlotInRange(int slot, int maxClients) => slot >= 0 && slot < maxClients;

    public static bool SkipNonHostRemoteClient(int remoteClient) => remoteClient != 0;

    public static bool HostGameFinishSessionNative(HostGameFinishContext ctx)
    {
        if (ctx.NetLoopWaitForConnections?.Invoke(ctx.ConnectedPlayers) != true)
        {
            ctx.SendAbort?.Invoke();
            ctx.ShutdownMasters?.Invoke();
            ctx.DebugMark?.Invoke("host session aborted");
            ctx.ExitHost?.Invoke(0);
            return false;
        }
        ctx.SetConnectFlowSyncing?.Invoke();
        ctx.SetNetworkDetails?.Invoke(ctx.MaxClients, ctx.DedicatedServerMode ? "server-syncing" : "syncing");
        ctx.NetMessage?.Invoke(ctx.DedicatedServerMode ? "Starting dedicated game" : "Starting game");
        if (SinglePlayerHostImmediateStart(ctx.ConnectedPlayers))
        {
            ctx.NetDone?.Invoke();
            ctx.CloseNetwork?.Invoke();
            ctx.SetMaxClients?.Invoke(1);
            ctx.SetTicDup?.Invoke(1);
            return true;
        }
        ctx.NetLog?.Invoke("Go");
        for (var client = 1; client < ctx.MaxClients; client++)
        {
            if (!ClientSlotConnected(ctx.GetStatus?.Invoke(client) ?? ctx.CstatNone, ctx.CstatNone))
                continue;
            ctx.QueueStartGameService?.Invoke(client);
        }
        if (ctx.NetLoopWaitForStartAcks?.Invoke(ctx.ConnectedPlayers) != true)
        {
            ctx.SendAbort?.Invoke();
            ctx.ShutdownMasters?.Invoke();
            ctx.DebugMark?.Invoke("host start acknowledgement wait aborted");
            ctx.ExitHost?.Invoke(0);
            return false;
        }
        ctx.NetDone?.Invoke();
        ctx.NetLogTotalPlayers?.Invoke(ctx.ConnectedPlayers);
        return true;
    }

    public static ushort IGetGamePortNative(ushort gamePort) => gamePort;

    public static string ReadVerificationErrorNative(ReadVerificationErrorContext ctx)
    {
        var stream = ctx.Stream;
        if (VerificationStreamTooShort(stream.Length, 5))
            return "Unknown error";
        if (stream[0] == ctx.VeEngine)
        {
            if (VerificationStreamTooShort(stream.Length, 7))
                return "Unknown error";
            return $"Engine mismatch: host expected {stream[1]}.{stream[2]}.{stream[3]}, got {stream[4]}.{stream[5]}.{stream[6]}";
        }
        var size = ReadVerificationPayloadSize(stream);
        if (stream[0] == ctx.VeFileUnknown)
            return ctx.FormatUnknownFiles?.Invoke(size, stream) ?? "Unknown error";
        if (stream[0] == ctx.VeFileOrder)
            return ctx.FormatExpectedOrder?.Invoke(size, stream) ?? "Unknown error";
        if (stream[0] == ctx.VeFileMissing)
            return ctx.FormatMissingFiles?.Invoke(size, stream) ?? "Unknown error";
        return "Unknown error";
    }

    public static bool GuestContactHostDisconnectNative(GuestContactHostDisconnectContext ctx)
    {
        while (ctx.GetConnection?.Invoke() is { GotPacket: true } conn)
        {
            if (SkipNonHostRemoteClient(ctx.RemoteClient))
                continue;
            if (ctx.NetBuffer[0] == ctx.NcmdExit)
            {
                ctx.FatalNetError?.Invoke("The host cancelled the game");
                return false;
            }
            if (ctx.NetBuffer[0] != ctx.NcmdSetup)
                continue;
            if (ctx.NetBuffer[1] == ctx.PreDisconnect)
            {
                if (!DisconnectPrePacketValid(ctx.NetBufferLength, 7)
                    || ctx.CheckSessionToken?.Invoke(ctx.ReadSessionToken?.Invoke(3) ?? 0) != true)
                    continue;
                var slot = ctx.NetBuffer[2];
                if (!ClientSlotInRange(slot, ctx.MaxClients))
                    continue;
                ctx.ClearClient?.Invoke(slot);
                ctx.SubtractNetworkClient?.Invoke(slot);
                ctx.SetClientAck?.Invoke(ctx.ConsolePlayer, slot, false);
                ctx.NetClientDisconnected?.Invoke(slot, "host reported disconnect");
            }
            else
                break;
        }
        return true;
    }
}

public sealed class HostGameFinishContext
{
    public int ConnectedPlayers { get; set; }
    public int MaxClients { get; set; }
    public bool DedicatedServerMode { get; init; }
    public byte CstatNone { get; init; }
    public Func<int, bool>? NetLoopWaitForConnections { get; init; }
    public Func<int, bool>? NetLoopWaitForStartAcks { get; init; }
    public Action? SendAbort { get; init; }
    public Action? ShutdownMasters { get; init; }
    public Action<string>? DebugMark { get; init; }
    public Action<int>? ExitHost { get; init; }
    public Action? SetConnectFlowSyncing { get; init; }
    public Action<int, string>? SetNetworkDetails { get; init; }
    public Action<string>? NetMessage { get; init; }
    public Action? NetDone { get; init; }
    public Action? CloseNetwork { get; init; }
    public Action<int>? SetMaxClients { get; init; }
    public Action<uint>? SetTicDup { get; init; }
    public Action<string>? NetLog { get; init; }
    public Func<int, byte>? GetStatus { get; init; }
    public Action<int>? QueueStartGameService { get; init; }
    public Action<int>? NetLogTotalPlayers { get; init; }
}

public sealed class ReadVerificationErrorContext
{
    public byte[] Stream { get; init; } = [];
    public byte VeEngine { get; init; }
    public byte VeFileUnknown { get; init; }
    public byte VeFileOrder { get; init; }
    public byte VeFileMissing { get; init; }
    public Func<int, byte[], string>? FormatUnknownFiles { get; init; }
    public Func<int, byte[], string>? FormatExpectedOrder { get; init; }
    public Func<int, byte[], string>? FormatMissingFiles { get; init; }
}

public sealed class GuestContactHostDisconnectContext
{
    public int RemoteClient { get; init; }
    public int ConsolePlayer { get; init; }
    public int MaxClients { get; init; }
    public int NetBufferLength { get; init; }
    public byte[] NetBuffer { get; init; } = [];
    public byte NcmdExit { get; init; }
    public byte NcmdSetup { get; init; }
    public byte PreDisconnect { get; init; }
    public Func<HostGetConnectionResult>? GetConnection { get; init; }
    public Action<string>? FatalNetError { get; init; }
    public Func<uint, bool>? CheckSessionToken { get; init; }
    public Func<int, uint>? ReadSessionToken { get; init; }
    public Action<int>? ClearClient { get; init; }
    public Action<int>? SubtractNetworkClient { get; init; }
    public Action<int, int, bool>? SetClientAck { get; init; }
    public Action<int, string>? NetClientDisconnected { get; init; }
}
