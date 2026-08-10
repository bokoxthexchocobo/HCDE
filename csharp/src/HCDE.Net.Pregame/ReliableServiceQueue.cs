using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public sealed class PendingReliableService
{
    public bool Active { get; set; }
    public PregameServiceType Service { get; set; }
    public byte Key { get; set; }
    public uint Sequence { get; set; }
    public ulong FirstSendTime { get; set; }
    public ulong LastSendTime { get; set; }
    public uint SendCount { get; set; }
    public byte[] Packet { get; set; } = Array.Empty<byte>();

    public void Clear()
    {
        Active = false;
        Service = PregameServiceType.Heartbeat;
        Key = 0;
        Sequence = 0;
        FirstSendTime = 0;
        LastSendTime = 0;
        SendCount = 0;
        Packet = Array.Empty<byte>();
    }
}

/// <summary>
/// Ports <c>FHCDEPendingService</c> queue and retransmit logic from <c>i_net.cpp</c>.
/// </summary>
public sealed class ReliableServiceQueue
{
    private readonly PendingReliableService[] _pending = new PendingReliableService[PregameConstants.MaxReliableServices];

    public ReliableServiceQueue()
    {
        for (var i = 0; i < _pending.Length; i++)
            _pending[i] = new PendingReliableService();
    }

    public IReadOnlyList<PendingReliableService> Pending => _pending;

    public bool HasPending() => FindOldest() is not null;

    public bool TryBegin(PregameServiceType service, PregameConnectionState connection, byte key)
    {
        if (Find(service, key) is not null)
            return false;
        return FindFree() is not null;
    }

    public bool TryCommit(ReadOnlySpan<byte> netBuffer, PregameServiceType service, byte key)
    {
        var slot = FindFree();
        if (slot is null)
            return false;

        slot.Active = true;
        slot.Service = service;
        slot.Key = key;
        slot.Sequence = BinaryPrimitives.ReadUInt32BigEndian(netBuffer[PregameConstants.ServiceSequenceOffset..]);
        slot.FirstSendTime = 0;
        slot.LastSendTime = 0;
        slot.SendCount = 0;
        slot.Packet = netBuffer.ToArray();
        return true;
    }

    public void ClearAcked(PregameConnectionState connection)
    {
        foreach (var pending in _pending)
        {
            if (pending.Active && pending.Sequence <= connection.ServicePeerAck)
                pending.Clear();
        }
    }

    public bool TryFlush(
        PregameConnectionState connection,
        ulong nowMilliseconds,
        Span<byte> netBuffer,
        out int netLength,
        bool force = false)
    {
        netLength = 0;
        ClearAcked(connection);

        var pending = FindOldest();
        if (pending is null)
            return false;

        if (!force
            && pending.SendCount > 0
            && nowMilliseconds - pending.LastSendTime < PregameConstants.ServiceResendMilliseconds)
            return true;

        if (pending.Packet.Length < PregameConstants.ServiceHeaderSize)
        {
            pending.Clear();
            return false;
        }

        pending.Packet.CopyTo(netBuffer);
        BinaryPrimitives.WriteUInt32BigEndian(
            netBuffer[PregameConstants.ServiceAckOffset..],
            connection.ServiceRxSeq);
        BinaryPrimitives.WriteUInt32BigEndian(
            pending.Packet.AsSpan(PregameConstants.ServiceAckOffset, 4),
            connection.ServiceRxSeq);
        netLength = pending.Packet.Length;

        if (pending.SendCount == 0)
            pending.FirstSendTime = nowMilliseconds;
        pending.LastSendTime = nowMilliseconds;
        pending.SendCount++;
        return true;
    }

    private PendingReliableService? Find(PregameServiceType service, byte key)
    {
        foreach (var pending in _pending)
        {
            if (pending.Active && pending.Service == service && pending.Key == key)
                return pending;
        }

        return null;
    }

    private PendingReliableService? FindFree()
    {
        foreach (var pending in _pending)
        {
            if (!pending.Active)
                return pending;
        }

        return null;
    }

    private PendingReliableService? FindOldest()
    {
        PendingReliableService? oldest = null;
        foreach (var pending in _pending)
        {
            if (!pending.Active)
                continue;
            if (oldest is null || pending.Sequence < oldest.Sequence)
                oldest = pending;
        }

        return oldest;
    }
}
