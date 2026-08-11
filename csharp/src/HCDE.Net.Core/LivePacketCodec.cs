namespace HCDE.Net.Core;

public readonly struct LivePacket
{
    public LivePacket(LiveHeader header, ReadOnlyMemory<byte> payload)
    {
        Header = header;
        Payload = payload;
    }

    public LiveHeader Header { get; }
    public ReadOnlyMemory<byte> Payload { get; }

    public static bool TryRead(ReadOnlySpan<byte> netBuffer, out LivePacket packet)
    {
        packet = default;
        if (!LiveHeader.TryRead(netBuffer, out var header))
            return false;

        var payload = netBuffer[LiveConstants.HeaderSize..].ToArray();
        packet = new LivePacket(header, payload);
        return true;
    }

    public static int Write(Span<byte> netBuffer, LivePacket packet)
    {
        var headerSize = LiveHeader.Write(netBuffer, packet.Header);
        if (headerSize == 0 || netBuffer.Length < headerSize + packet.Payload.Length)
            return 0;

        packet.Payload.Span.CopyTo(netBuffer[headerSize..]);
        return headerSize + packet.Payload.Length;
    }
}

public static class LiveControlPacketBuilder
{
    public static LivePacket BuildControl(
        uint txSequence,
        uint acknowledgement,
        LiveControlBasePayload basePayload,
        LiveControlCapabilities? capabilities = null)
    {
        var payloadSize = capabilities is null
            ? LiveConstants.ControlBasePayloadSize
            : LiveConstants.ControlBasePayloadSize + capabilities.Value.WireSize;

        var payload = new byte[payloadSize];
        LiveControlPayloadCodec.Write(payload, basePayload, capabilities);

        var header = new LiveHeader(LiveMessageType.Control, txSequence, acknowledgement);
        return new LivePacket(header, payload);
    }

    public static bool TryParseControl(
        LivePacket packet,
        out LiveControlBasePayload basePayload,
        out LiveControlCapabilities? capabilities,
        out LiveCapabilityNegotiation? negotiation,
        ulong localCapabilities = LiveConstants.DefaultLocalCapabilities)
    {
        basePayload = default;
        capabilities = null;
        negotiation = null;

        if (packet.Header.MessageType != LiveMessageType.Control)
            return false;
        if (!LiveControlPayloadCodec.TryRead(packet.Payload.Span, out basePayload, out capabilities))
            return false;

        if (capabilities is not null)
        {
            var result = LiveControlCapabilitiesParser.Apply(packet.Payload.Span, localCapabilities, out var parsed);
            if (result == LiveCapabilityParseResult.Parsed)
                negotiation = parsed;
        }

        return true;
    }
}

public sealed class LiveControlScheduler
{
    private ulong _lastControlSentMs;

    public bool ShouldSendControl(ulong nowMs, ulong intervalMs = LiveConstants.ControlIntervalMs)
    {
        if (_lastControlSentMs != 0 && nowMs - _lastControlSentMs < intervalMs)
            return false;

        _lastControlSentMs = nowMs;
        return true;
    }
}
