namespace HCDE.Net.Core;

/// <summary>Reliable HCDE pregame service queue (<c>i_net.cpp</c> 1976–2125).</summary>
public static class VmINetReliableQueue
{
    public static bool CommitReliableHcdePregameServiceNative(
        HcdeReliableServiceSlot[] slots,
        byte service,
        byte key,
        ReadOnlySpan<byte> netBuffer,
        int netBufferLength,
        int sequenceOffset,
        Func<HcdeReliableServiceSlot[]?, HcdeReliableServiceSlot?>? findFree,
        Action? flushServices)
    {
        var pending = findFree?.Invoke(slots);
        if (pending is null)
            return false;
        pending.Active = true;
        pending.Service = service;
        pending.Key = key;
        pending.Sequence = VmINetPregameService.ReadBe32Native(netBuffer.Slice(sequenceOffset, 4));
        pending.FirstSendTime = 0;
        pending.LastSendTime = 0;
        pending.SendCount = 0;
        pending.Packet = netBuffer.Slice(0, netBufferLength).ToArray();
        flushServices?.Invoke();
        return true;
    }

    public static ushort ReadBe16Native(ReadOnlySpan<byte> data) =>
        (ushort)((data[0] << 8) | data[1]);

    public static void WriteBe16Native(Span<byte> data, ushort value)
    {
        data[0] = (byte)(value >> 8);
        data[1] = (byte)value;
    }

    public static bool QueueHcdeRosterServiceNative(
        int maxClients,
        int targetClient,
        int maxMsgLen,
        Span<byte> netBuffer,
        ref int netBufferLength,
        int serviceHeaderSize,
        byte cstatNone,
        byte cstatWaiting,
        Func<int, byte>? getStatus,
        Func<int, int, int>? appendRosterEntry,
        Func<bool>? beginRosterService,
        Func<bool>? commitRosterService)
    {
        if (beginRosterService?.Invoke() != true)
            return false;
        var rosterCountOffset = netBufferLength++;
        byte rosterCount = 0;
        for (var i = 0; i < maxClients; i++)
        {
            var status = getStatus?.Invoke(i) ?? cstatNone;
            if (i == targetClient || status == cstatNone || status < cstatWaiting)
                continue;
            if (appendRosterEntry?.Invoke(i, netBufferLength) < 0)
                return false;
            rosterCount++;
        }
        netBuffer[rosterCountOffset] = rosterCount;
        return commitRosterService?.Invoke() ?? false;
    }

    public static bool QueueHcdeBootstrapControlServiceNative(
        byte service,
        byte roomId,
        int gameTic,
        int clientTic,
        int serviceHeaderSize,
        Span<byte> netBuffer,
        ref int netBufferLength,
        Func<bool>? beginService,
        Func<bool>? commitService)
    {
        if (beginService?.Invoke() != true)
            return false;
        netBuffer[serviceHeaderSize] = roomId;
        VmINetPregameService.WriteBe32Native(netBuffer.Slice(serviceHeaderSize + 1, 4), (uint)Math.Max(gameTic, 0));
        VmINetPregameService.WriteBe32Native(netBuffer.Slice(serviceHeaderSize + 5, 4), (uint)Math.Max(clientTic, 0));
        VmINetPregameService.WriteBe32Native(netBuffer.Slice(serviceHeaderSize + 9, 4), 0);
        netBufferLength = serviceHeaderSize + 13;
        return commitService?.Invoke() ?? false;
    }

    public static void AckHcdeControlServiceNative(
        byte service,
        int serviceHeaderSize,
        Func<bool>? beginService,
        Func<bool>? commitService,
        Action? flushForced)
    {
        if (beginService?.Invoke() != true)
            return;
        commitService?.Invoke();
        flushForced?.Invoke();
    }

    public static void HandleGuestRuntimeHcdeServiceNative(
        bool localAuthority,
        int remoteClient,
        int netBufferLength,
        int serviceHeaderSize,
        byte ncmdSetup,
        byte preHcdeService,
        ReadOnlySpan<byte> netBuffer,
        bool hcdeConnectPresent,
        Func<ulong, int, string, bool>? checkPregameService,
        Action<string>? netMessage,
        Action<byte>? ackResync)
    {
        if (localAuthority || remoteClient != 0 || netBufferLength < serviceHeaderSize
            || netBuffer[0] != ncmdSetup || netBuffer[1] != preHcdeService || !hcdeConnectPresent)
            return;
        var service = netBuffer[2];
        switch (service)
        {
            case 1: // HPS_RESYNC_BEGIN
                if (checkPregameService?.Invoke(0, serviceHeaderSize + 13, "guest runtime service resync begin") == true)
                {
                    netMessage?.Invoke("Receiving HCDE resync");
                    ackResync?.Invoke(2);
                }
                break;
            case 3: // HPS_HEARTBEAT
                checkPregameService?.Invoke(0, serviceHeaderSize, "guest runtime service heartbeat");
                break;
        }
    }

    public static int GetPacketRecvNative(
        Func<int>? recvFrom,
        Func<uint, ushort, int>? findClient,
        Func<int, int, int>? handleConnReset,
        Func<int, byte[], int, bool>? tryHandleServerQuery)
    {
        var msgSize = recvFrom?.Invoke() ?? -1;
        if (msgSize < 0)
            return -1;
        if (msgSize > 0 && tryHandleServerQuery?.Invoke(msgSize, [], msgSize) == true)
            return 0;
        return msgSize;
    }
}
