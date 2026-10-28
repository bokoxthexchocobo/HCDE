namespace HCDE.Net.Core;

/// <summary>Outbound packet path and reliable-service flush (<c>i_net.cpp</c> 1812–1974).</summary>
public static class VmINetPacketSend
{
    public const byte NcmdCompressed = 0x20;
    public const byte NcmdSetup = 0x80;

    public static void IClearClientNative(int client, Action<int>? clearConnected) =>
        clearConnected?.Invoke(client);

    public static int FindClientNative(
        uint sinAddr,
        ushort sinPort,
        int maxClients,
        Func<int, byte>? getStatus,
        Func<int, uint>? getPeerAddr,
        Func<int, ushort>? getPeerPort,
        byte cstatNone)
    {
        for (var i = 0; i < maxClients; i++)
        {
            if (getStatus?.Invoke(i) == cstatNone)
                continue;
            if (getPeerAddr?.Invoke(i) == sinAddr && getPeerPort?.Invoke(i) == sinPort)
                return i;
        }
        return -1;
    }

    public static void SendPacketNative(
        int netBufferLength,
        int maxMsgLen,
        int maxTransmitSize,
        int minCompressionSize,
        byte[] netBuffer,
        byte[] transmitBuffer,
        byte[] gameId,
        Func<byte[], int, uint>? calcCrc32,
        Func<uint, byte[], uint>? addCrc32,
        Func<byte[], int, byte[], int, int>? compress2,
        Func<int, byte[], int, byte, int>? sendTo,
        Action<string>? fatalOverflow,
        Action<string>? errorCompress,
        Action<string>? warningSendFailed,
        Action<string>? markSetupSendOk)
    {
        if (netBufferLength > maxMsgLen)
        {
            fatalOverflow?.Invoke($"Netbuffer overflow: Tried to send {netBufferLength} bytes of data");
            return;
        }

        var dataStart = 4;
        int size;
        if (netBufferLength >= minCompressionSize)
        {
            transmitBuffer[dataStart] = (byte)(netBuffer[0] | NcmdCompressed);
            var compressedLen = compress2?.Invoke(
                netBuffer, netBufferLength - 1,
                transmitBuffer, transmitBuffer.Length) ?? -1;
            if (compressedLen < 0)
            {
                errorCompress?.Invoke("Net compression failed");
                return;
            }
            size = compressedLen + 1;
        }
        else
        {
            Array.Copy(netBuffer, 0, transmitBuffer, dataStart, netBufferLength);
            size = netBufferLength;
        }

        if (size + 4 > maxTransmitSize)
        {
            errorCompress?.Invoke("Failed to compress data down to acceptable transmission size");
            return;
        }

        var payload = transmitBuffer.AsSpan(dataStart, size).ToArray();
        var crc = (netBuffer[0] & NcmdSetup) != 0
            ? calcCrc32?.Invoke(payload, size) ?? 0
            : addCrc32?.Invoke(calcCrc32?.Invoke(payload, size) ?? 0, gameId) ?? 0;
        transmitBuffer[0] = (byte)(crc >> 24);
        transmitBuffer[1] = (byte)(crc >> 16);
        transmitBuffer[2] = (byte)(crc >> 8);
        transmitBuffer[3] = (byte)crc;

        var sendResult = sendTo?.Invoke(size + 4, transmitBuffer, netBuffer[0], netBuffer[1]) ?? 0;
        if (sendResult < 0)
            warningSendFailed?.Invoke("sendto FAILED");
        else if ((netBuffer[0] & NcmdSetup) != 0)
            markSetupSendOk?.Invoke("sendto ok");
    }

    public static bool FlushHcdeReliableServicesNative(
        HcdeReliableServiceSlot[] slots,
        uint peerAck,
        ulong now,
        ulong resendMs,
        int serviceHeaderSize,
        int serviceAckOffset,
        Span<byte> netBuffer,
        ref int netBufferLength,
        Action? clearAcked,
        Func<HcdeReliableServiceSlot[]?, HcdeReliableServiceSlot?>? findOldest,
        Action? sendPacket,
        Action? dropMalformed)
    {
        clearAcked?.Invoke();
        var pending = findOldest?.Invoke(slots);
        if (pending is null)
            return false;
        if (!pending.Active)
            return false;
        if (pending.SendCount > 0 && now - pending.LastSendTime < resendMs)
            return true;
        if (pending.Packet.Length < serviceHeaderSize)
        {
            dropMalformed?.Invoke();
            pending.Clear();
            return false;
        }
        netBufferLength = pending.Packet.Length;
        pending.Packet.CopyTo(netBuffer);
        VmINetPregameService.WriteBe32Native(netBuffer.Slice(serviceAckOffset, 4), peerAck);
        pending.Packet.AsSpan(serviceAckOffset, 4).Clear();
        VmINetPregameService.WriteBe32Native(pending.Packet.AsSpan(serviceAckOffset), peerAck);
        sendPacket?.Invoke();
        if (pending.SendCount == 0)
            pending.FirstSendTime = now;
        pending.LastSendTime = now;
        pending.SendCount++;
        return true;
    }

    public static bool BeginReliableHcdePregameServiceNative(
        byte service,
        byte key,
        HcdeReliableServiceSlot[] slots,
        Func<byte, byte, HcdeReliableServiceSlot?>? findExisting,
        Func<HcdeReliableServiceSlot[]?, HcdeReliableServiceSlot?>? findOldestWhenFull,
        Func<HcdeReliableServiceSlot[]?, HcdeReliableServiceSlot?>? findFree,
        Action<byte>? beginServiceHeader)
    {
        if (findExisting?.Invoke(service, key) is not null)
            return false;
        if (findFree?.Invoke(slots) is null)
        {
            findOldestWhenFull?.Invoke(slots);
            return false;
        }
        beginServiceHeader?.Invoke(service);
        return true;
    }
}

public sealed class HcdeReliableServiceSlot
{
    public bool Active;
    public byte Service;
    public byte Key;
    public uint Sequence;
    public ulong FirstSendTime;
    public ulong LastSendTime;
    public uint SendCount;
    public byte[] Packet = [];

    public void Clear()
    {
        Active = false;
        Service = 0;
        Key = 0;
        Sequence = 0;
        FirstSendTime = 0;
        LastSendTime = 0;
        SendCount = 0;
        Packet = [];
    }
}
