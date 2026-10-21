namespace HCDE.Net.Core;

/// <summary>Inbound `GetPacket` decode path (<c>i_net.cpp</c> 2126–2320).</summary>
public static class VmINetGetPacket
{
    public const byte NcmdCompressed = 0x20;
    public const byte NcmdSetup = 0x80;
    public const byte NcmdExit = 0x04;

    public static int FinishGetPacketNative(
        int client,
        int msgSize,
        int maxMsgLen,
        ref int remoteClient,
        ref int netBufferLength,
        Action<string>? logOversized)
    {
        remoteClient = client;
        netBufferLength = Math.Max(msgSize, 0);
        if (netBufferLength > maxMsgLen)
        {
            logOversized?.Invoke("clamped oversized packet");
            netBufferLength = maxMsgLen;
        }
        return netBufferLength;
    }

    public static bool TryDecodeInboundCrcNative(
        ReadOnlySpan<byte> transmitBuffer,
        int msgSize,
        byte[] gameId,
        Func<byte[], int, uint>? calcCrc32,
        Func<uint, byte[], uint>? addCrc32,
        out int payloadSize,
        out uint netBufferHeader)
    {
        payloadSize = 0;
        netBufferHeader = 0;
        if (msgSize < 5)
            return false;
        var dataStart = 4;
        payloadSize = msgSize - 4;
        var setup = (transmitBuffer[dataStart] & NcmdSetup) != 0;
        var payload = transmitBuffer.Slice(dataStart, payloadSize).ToArray();
        var check = setup
            ? calcCrc32?.Invoke(payload, payloadSize) ?? 0
            : addCrc32?.Invoke(calcCrc32?.Invoke(payload, payloadSize) ?? 0, gameId) ?? 0;
        var crc = (uint)(transmitBuffer[0] << 24 | transmitBuffer[1] << 16 | transmitBuffer[2] << 8 | transmitBuffer[3]);
        if (check != crc)
            return false;
        netBufferHeader = (byte)(transmitBuffer[dataStart] & ~NcmdCompressed);
        return true;
    }

    public static bool TryUncompressPayloadNative(
        ReadOnlySpan<byte> transmitBuffer,
        int msgSize,
        Span<byte> netBuffer,
        ref int outMsgSize,
        Func<byte[], int, byte[], int, int>? uncompress)
    {
        var dataStart = 4;
        if ((transmitBuffer[dataStart] & NcmdCompressed) == 0)
        {
            var copySize = msgSize - 5;
            if (copySize > 0)
                transmitBuffer.Slice(dataStart + 1, copySize).CopyTo(netBuffer.Slice(1));
            outMsgSize = msgSize - 4;
            return true;
        }
        if (msgSize <= 5)
            return false;
        var size = uncompress?.Invoke(
            transmitBuffer.Slice(dataStart + 1, msgSize - 5).ToArray(), msgSize - 5,
            netBuffer.Slice(1).ToArray(), netBuffer.Length - 1) ?? -1;
        if (size < 0)
            return false;
        outMsgSize = size + 1;
        return true;
    }

    public static bool TryLateJoinAdmissionNative(
        int client,
        bool gameStarted,
        bool localAuthority,
        bool dedicatedServerMode,
        bool lateJoinEnabled,
        int msgSize,
        ref int netBufferLength,
        Func<bool>? tryProcessSetupConnect,
        Func<uint, ushort, int>? findClient,
        Action? sendPreInProgress,
        out int resolvedClient)
    {
        resolvedClient = client;
        if (client != -1 || !gameStarted)
            return false;
        var admissionAllowed = localAuthority && (dedicatedServerMode || lateJoinEnabled);
        netBufferLength = Math.Max(msgSize, 0);
        if (admissionAllowed && tryProcessSetupConnect?.Invoke() == true)
            resolvedClient = findClient?.Invoke(0, 0) ?? -1;
        if (resolvedClient == -1 && !admissionAllowed)
        {
            sendPreInProgress?.Invoke();
            return true;
        }
        return resolvedClient != -1;
    }
}
