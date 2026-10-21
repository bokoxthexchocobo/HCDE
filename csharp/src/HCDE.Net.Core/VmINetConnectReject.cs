namespace HCDE.Net.Core;

/// <summary>Setup rejection and client add (<c>i_net.cpp</c> 2525–2676).</summary>
public static class VmINetConnectReject
{
    public static void RejectConnectionNative(
        byte ncmdSetup,
        byte reason,
        Span<byte> netBuffer,
        ref int netBufferLength,
        Action? sendPacket)
    {
        netBuffer[0] = ncmdSetup;
        netBuffer[1] = reason;
        netBufferLength = 2;
        sendPacket?.Invoke();
    }

    public static void SendVerificationErrorNative(
        byte ncmdSetup,
        byte preVerificationError,
        byte veEngine,
        VerificationErrorPayload error,
        Span<byte> netBuffer,
        ref int netBufferLength,
        int maxMsgLen,
        Action? sendPacket,
        Action<string>? debugMark)
    {
        netBuffer[0] = ncmdSetup;
        netBuffer[1] = preVerificationError;
        netBuffer[2] = error.ErrorCode;
        if (error.ErrorCode == veEngine)
        {
            netBuffer[3] = error.Major;
            netBuffer[4] = error.Minor;
            netBuffer[5] = error.Revision;
            netBuffer[6] = error.NetMajor;
            netBuffer[7] = error.NetMinor;
            netBuffer[8] = error.NetRevision;
            netBufferLength = 9;
        }
        else
        {
            var files = error.FileList;
            if (files is null || files.Count == 0)
            {
                debugMark?.Invoke($"verification error payload type {error.ErrorCode} has no list");
                netBuffer[3] = netBuffer[4] = netBuffer[5] = netBuffer[6] = 0;
                netBufferLength = 7;
                sendPacket?.Invoke();
                return;
            }
            var offset = 7;
            var count = 0u;
            foreach (var file in files)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(file);
                if (offset + bytes.Length + 1 > maxMsgLen)
                    break;
                bytes.CopyTo(netBuffer.Slice(offset));
                offset += bytes.Length;
                netBuffer[offset++] = 0;
                count++;
            }
            netBuffer[3] = (byte)(count >> 24);
            netBuffer[4] = (byte)(count >> 16);
            netBuffer[5] = (byte)(count >> 8);
            netBuffer[6] = (byte)count;
            netBufferLength = offset;
        }
        sendPacket?.Invoke();
    }

    public static void AddClientConnectionNative(
        int client,
        bool runtimeJoin,
        HcdeConnectInfoPacket connectInfo,
        Action<int>? resetClientState,
        Action<int>? clearConnectedSlot,
        Action<int, byte>? setStatusConnecting,
        Action<int, uint>? setSessionToken,
        Action<int, bool, byte, byte>? setHcdeConnectFields,
        Action<int, bool>? setRuntimeJoinFlag,
        Action<int>? addNetworkClient,
        Action<int, bool>? resetRuntimePlayerState,
        Action<int>? netClientUpdated,
        Action<int, int>? demoteReadyClientsToWaiting)
    {
        resetClientState?.Invoke(client);
        clearConnectedSlot?.Invoke(client);
        setStatusConnecting?.Invoke(client, 1);
        setSessionToken?.Invoke(client, 0);
        if (connectInfo.Present)
            setHcdeConnectFields?.Invoke(client, true, connectInfo.Version, connectInfo.Flags);
        setRuntimeJoinFlag?.Invoke(client, runtimeJoin);
        addNetworkClient?.Invoke(client);
        if (runtimeJoin)
            resetRuntimePlayerState?.Invoke(client, true);
        if (connectInfo.Present)
            netClientUpdated?.Invoke(client);
        else if (!runtimeJoin)
            demoteReadyClientsToWaiting?.Invoke(1, client);
    }
}

public readonly struct VerificationErrorPayload(
    byte errorCode,
    byte major,
    byte minor,
    byte revision,
    byte netMajor,
    byte netMinor,
    byte netRevision,
    IReadOnlyList<string>? fileList)
{
    public byte ErrorCode { get; } = errorCode;
    public byte Major { get; } = major;
    public byte Minor { get; } = minor;
    public byte Revision { get; } = revision;
    public byte NetMajor { get; } = netMajor;
    public byte NetMinor { get; } = netMinor;
    public byte NetRevision { get; } = netRevision;
    public IReadOnlyList<string>? FileList { get; } = fileList;
}
