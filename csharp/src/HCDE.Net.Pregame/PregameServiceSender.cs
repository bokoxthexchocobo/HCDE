using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

/// <summary>
/// Builds reliable HCDE pregame service packets and manages the send queue.
/// </summary>
public sealed class PregameServiceSender
{
    private readonly ReliableServiceQueue _queue = new();
    private readonly byte[] _netBuffer = new byte[NetConstants.MaxMessageLength];

    public ReliableServiceQueue Queue => _queue;

    public bool TryQueueReliable(
        PregameServiceType service,
        PregameConnectionState connection,
        byte key,
        ReadOnlySpan<byte> payload)
    {
        if (!_queue.TryBegin(service, connection, key))
            return false;

        var sequence = connection.AllocateServiceSequence();
        var length = HcdeServicePacket.Write(
            _netBuffer,
            service,
            connection.SessionToken,
            sequence,
            connection.ServiceRxSeq,
            payload);

        if (length == 0 || !_queue.TryCommit(_netBuffer.AsSpan(0, length), service, key))
            return false;

        return true;
    }

    public bool TryFlush(
        PregameConnectionState connection,
        ulong nowMilliseconds,
        Span<byte> netBuffer,
        out int netLength,
        bool force = false) =>
        _queue.TryFlush(connection, nowMilliseconds, netBuffer, out netLength, force);
}
