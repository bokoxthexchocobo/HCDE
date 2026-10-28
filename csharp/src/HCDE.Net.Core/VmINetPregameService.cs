namespace HCDE.Net.Core;

/// <summary>HCDE pregame reliable-service helpers (<c>i_net.cpp</c> 448–696).</summary>
public static class VmINetPregameService
{
    public const int HcdeServiceHeaderSize = 15;
    public const int HcdeServiceSequenceOffset = 7;
    public const int HcdeServiceAckOffset = 11;
    public const ulong HcdeServiceTimeoutMs = 15000;

    public static uint ReadBe32Native(ReadOnlySpan<byte> data) =>
        (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);

    public static void WriteBe32Native(Span<byte> data, uint value)
    {
        data[0] = (byte)(value >> 24);
        data[1] = (byte)(value >> 16);
        data[2] = (byte)(value >> 8);
        data[3] = (byte)value;
    }

    public static void BeginSetupPacketNative(Span<byte> buffer, byte type, uint token, int tokenOffset)
    {
        buffer[0] = 0;
        buffer[1] = type;
        WriteBe32Native(buffer.Slice(tokenOffset, 4), token);
    }

    public static void BeginHcdePregameServiceNative(
        Span<byte> buffer,
        byte service,
        ref uint txSeq,
        uint rxSeq,
        ref int bufferLength)
    {
        buffer[2] = service;
        txSeq++;
        WriteBe32Native(buffer.Slice(HcdeServiceSequenceOffset, 4), txSeq);
        WriteBe32Native(buffer.Slice(HcdeServiceAckOffset, 4), rxSeq);
        bufferLength = HcdeServiceHeaderSize;
    }

    public static int FindReliableServiceIndexNative(HcdePendingServiceSlot[] slots, byte service, byte key)
    {
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i].Active && slots[i].Service == service && slots[i].Key == key)
                return i;
        }
        return -1;
    }

    public static int FindFreeReliableServiceIndexNative(HcdePendingServiceSlot[] slots)
    {
        for (var i = 0; i < slots.Length; i++)
        {
            if (!slots[i].Active)
                return i;
        }
        return -1;
    }

    public static int FindOldestReliableServiceIndexNative(HcdePendingServiceSlot[] slots)
    {
        var oldest = -1;
        uint oldestSeq = uint.MaxValue;
        for (var i = 0; i < slots.Length; i++)
        {
            if (!slots[i].Active)
                continue;
            if (slots[i].Sequence < oldestSeq)
            {
                oldestSeq = slots[i].Sequence;
                oldest = i;
            }
        }
        return oldest;
    }

    public static void ClearAckedReliableServicesNative(HcdePendingServiceSlot[] slots, uint peerAck)
    {
        foreach (ref var slot in slots.AsSpan())
        {
            if (slot.Active && slot.Sequence <= peerAck)
                slot.Clear();
        }
    }

    public static int FindTimedOutReliableServiceIndexNative(
        HcdePendingServiceSlot[] slots,
        uint peerAck,
        ulong now,
        ulong timeoutMs)
    {
        ClearAckedReliableServicesNative(slots, peerAck);
        for (var i = 0; i < slots.Length; i++)
        {
            ref var slot = ref slots[i];
            if (slot.Active && slot.SendCount > 0 && slot.FirstSendTime > 0
                && now - slot.FirstSendTime >= timeoutMs)
                return i;
        }
        return -1;
    }

    public static uint MakeSessionTokenNative(uint crc, uint addrHash, uint portHash, uint clientHash, uint timeHash)
    {
        var token = crc ^ addrHash ^ portHash ^ clientHash ^ timeHash;
        return token == 0 ? 1u : token;
    }

    public static void NoteMalformedServiceTrafficNative(
        ref uint connectionStrikes,
        uint strikeLimit,
        ref ulong malformedUntil,
        ulong now,
        ulong quarantineMs)
    {
        connectionStrikes++;
        if (connectionStrikes < strikeLimit)
            return;
        connectionStrikes = 0;
        malformedUntil = now + quarantineMs;
    }

    public static bool ServiceQuarantineActiveNative(ulong malformedUntil, ulong now) => malformedUntil > now;

    public static void ClearServiceQuarantineNative(ref uint strikes, ref ulong malformedUntil)
    {
        strikes = 0;
        malformedUntil = 0;
    }

    public static bool CheckSessionTokenNative(uint expected, uint actual, ref uint strikes, ref ulong malformedUntil)
    {
        if (expected != actual)
        {
            return false;
        }
        ClearServiceQuarantineNative(ref strikes, ref malformedUntil);
        return true;
    }

    public static bool CheckHcdePregameServiceNative(
        ref HcdeConnectionServiceState connection,
        ulong netBufferLength,
        ulong minimumSize,
        ReadOnlySpan<byte> netBuffer,
        ulong now,
        uint malformedStrikeLimit,
        ulong quarantineMs)
    {
        if (ServiceQuarantineActiveNative(connection.MalformedUntil, now))
            return false;

        if (netBufferLength < minimumSize)
        {
            NoteMalformedServiceTrafficNative(
                ref connection.MalformedStrikes, malformedStrikeLimit, ref connection.MalformedUntil, now, quarantineMs);
            return false;
        }

        var token = ReadBe32Native(netBuffer.Slice(3, 4));
        if (!CheckSessionTokenNative(connection.SessionToken, token, ref connection.MalformedStrikes, ref connection.MalformedUntil))
        {
            NoteMalformedServiceTrafficNative(
                ref connection.MalformedStrikes, malformedStrikeLimit, ref connection.MalformedUntil, now, quarantineMs);
            return false;
        }

        connection.LastValidRxTime = now;
        var seq = ReadBe32Native(netBuffer.Slice(HcdeServiceSequenceOffset, 4));
        var ack = ReadBe32Native(netBuffer.Slice(HcdeServiceAckOffset, 4));
        if (seq == 0)
        {
            NoteMalformedServiceTrafficNative(
                ref connection.MalformedStrikes, malformedStrikeLimit, ref connection.MalformedUntil, now, quarantineMs);
            return false;
        }

        if (ack > connection.TxSeq)
        {
            // Native logs ack-out-of-range and bumps profile (i_net.cpp 653–656); no hosted trace here.
        }
        else if (ack > connection.PeerAck)
        {
            connection.PeerAck = ack;
            ClearAckedReliableServicesNative(connection.PendingServices, ack);
        }

        if (seq <= connection.RxSeq)
            return false;

        connection.RxSeq = seq;
        ClearServiceQuarantineNative(ref connection.MalformedStrikes, ref connection.MalformedUntil);
        return true;
    }

    public static bool FindStringEndNative(ReadOnlySpan<byte> buffer, int start, int limit, out int end)
    {
        for (var i = start; i < limit; i++)
        {
            if (buffer[i] == 0)
            {
                end = i + 1;
                return true;
            }
        }
        end = start;
        return false;
    }
}

public struct HcdePendingServiceSlot
{
    public bool Active;
    public byte Service;
    public byte Key;
    public uint Sequence;
    public ulong FirstSendTime;
    public uint SendCount;

    public void Clear()
    {
        Active = false;
        Service = 0;
        Key = 0;
        Sequence = 0;
        FirstSendTime = 0;
        SendCount = 0;
    }
}

public struct HcdeConnectionServiceState
{
    public uint SessionToken;
    public uint TxSeq;
    public uint RxSeq;
    public uint PeerAck;
    public uint MalformedStrikes;
    public ulong MalformedUntil;
    public ulong LastValidRxTime;
    public HcdePendingServiceSlot[] PendingServices;
}
