namespace HCDE.Net.Core;

/// <summary>Setup rejection and client add (<c>i_net.cpp</c> 2525–2676).</summary>
public static class VmINetConnectReject
{
    public const byte VeEngine = 0;
    public const byte VeFileUnknown = 1;
    public const byte VeFileMissing = 2;
    public const byte VeFileOrder = 3;

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

    public static IReadOnlyList<string>? SelectVerificationFileListNative(in VerificationErrorPayload error)
    {
        if (error.ErrorCode == VeFileUnknown)
            return error.UnknownFiles;
        if (error.ErrorCode == VeFileOrder)
            return error.ExpectedOrder;
        if (error.ErrorCode == VeFileMissing)
            return error.MissingFiles;
        return null;
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
        Action<string>? debugMark,
        Action<string>? printTruncated)
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
            sendPacket?.Invoke();
            return;
        }

        var files = SelectVerificationFileListNative(in error);
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
            var len = bytes.Length + 1;
            if (offset + len > maxMsgLen)
            {
                debugMark?.Invoke($"verification error truncated after {count} entries");
                printTruncated?.Invoke("Verification error reply truncated to fit packet size\n");
                break;
            }

            bytes.CopyTo(netBuffer.Slice(offset));
            netBuffer[offset + bytes.Length] = 0;
            offset += len;
            count++;
        }

        netBuffer[3] = (byte)(count >> 24);
        netBuffer[4] = (byte)(count >> 16);
        netBuffer[5] = (byte)(count >> 8);
        netBuffer[6] = (byte)count;
        netBufferLength = offset;
        sendPacket?.Invoke();
    }

    public static void AddClientConnectionNative(
        int client,
        bool runtimeJoin,
        HcdeConnectInfoPacket connectInfo,
        int maxClients,
        uint peerAddress,
        ushort peerPort,
        byte cstatReady,
        byte cstatWaiting,
        Func<int, byte>? getClientStatus,
        Func<uint, ushort, int, ulong>? makeSessionToken,
        Func<int, bool>? isServerReservedSlot,
        Func<int, bool>? playerHasLivePawnOrCamera,
        Action<int>? resetClientState,
        Action<int>? clearConnectedSlot,
        Action<int, byte>? setClientStatus,
        Action<int, uint, ushort>? setConnectedAddress,
        Action<int, ulong>? setSessionToken,
        Action<int, bool, byte, byte>? setHcdeConnectFields,
        Action<int, bool>? setRuntimeJoinFlag,
        Action<int>? addNetworkClient,
        Action<int, bool>? setPlayeringame,
        Action<int, bool>? setPlayerWaiting,
        Action<int, bool>? setPlayerInconsistant,
        Action<int, bool>? setSettingsController,
        Action<int, byte, string>? setPlayerState,
        byte playerStateGone,
        byte playerStateDead,
        Action<int>? netClientUpdated,
        Action<string>? netLog,
        Action<string>? admissionPolicyBugLog)
    {
        resetClientState?.Invoke(client);
        clearConnectedSlot?.Invoke(client);
        setClientStatus?.Invoke(client, 1);
        setConnectedAddress?.Invoke(client, peerAddress, peerPort);
        var token = makeSessionToken?.Invoke(peerAddress, peerPort, client) ?? 0;
        setSessionToken?.Invoke(client, token);
        setHcdeConnectFields?.Invoke(client, connectInfo.Present, connectInfo.Version, connectInfo.Flags);
        setRuntimeJoinFlag?.Invoke(client, runtimeJoin);
        addNetworkClient?.Invoke(client);

        if (runtimeJoin && isServerReservedSlot?.Invoke(client) != true)
        {
            setPlayeringame?.Invoke(client, false);
            setPlayerWaiting?.Invoke(client, false);
            setPlayerInconsistant?.Invoke(client, false);
            setSettingsController?.Invoke(client, false);
            if (playerHasLivePawnOrCamera?.Invoke(client) == true)
                setPlayerState?.Invoke(client, playerStateGone, "runtime_connect_transport_reset");
            else
                setPlayerState?.Invoke(client, playerStateDead, "runtime_connect_transport_reset");
        }

        if (connectInfo.Present)
        {
            var runtimeSuffix = runtimeJoin ? " (runtime join)" : "";
            netLog?.Invoke(
                $"Client {client} connected to server with HCDE service connect v{connectInfo.Version} flags=0x{connectInfo.Flags:X2}{runtimeSuffix} dest={peerAddress}:{peerPort}");
        }
        else
        {
            admissionPolicyBugLog?.Invoke(
                $"Client {client} admitted without HCDE service info (admission policy bug){(runtimeJoin ? " (runtime join)" : "")}");
        }

        netClientUpdated?.Invoke(client);

        if (!runtimeJoin)
        {
            for (var i = 1; i < maxClients; i++)
            {
                if (getClientStatus?.Invoke(i) != cstatReady)
                    continue;
                setClientStatus?.Invoke(i, cstatWaiting);
                netClientUpdated?.Invoke(i);
            }
        }
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
    IReadOnlyList<string>? unknownFiles,
    IReadOnlyList<string>? expectedOrder,
    IReadOnlyList<string>? missingFiles)
{
    public byte ErrorCode { get; } = errorCode;
    public byte Major { get; } = major;
    public byte Minor { get; } = minor;
    public byte Revision { get; } = revision;
    public byte NetMajor { get; } = netMajor;
    public byte NetMinor { get; } = netMinor;
    public byte NetRevision { get; } = netRevision;
    public IReadOnlyList<string>? UnknownFiles { get; } = unknownFiles;
    public IReadOnlyList<string>? ExpectedOrder { get; } = expectedOrder;
    public IReadOnlyList<string>? MissingFiles { get; } = missingFiles;
}
