namespace HCDE.Net.Core;

public readonly struct LiveCapabilityNegotiation
{
    public LiveCapabilityNegotiation(ulong remote, ulong local, ulong negotiated, ulong unsupported)
    {
        Remote = remote;
        Local = local;
        Negotiated = negotiated;
        Unsupported = unsupported;
    }

    public ulong Remote { get; }
    public ulong Local { get; }
    public ulong Negotiated { get; }
    public ulong Unsupported { get; }

    public static LiveCapabilityNegotiation Negotiate(ulong remoteCapabilities, ulong localCapabilities = LiveConstants.DefaultLocalCapabilities)
    {
        var unsupported = remoteCapabilities & ~LiveConstants.KnownCapabilityMask;
        var negotiated = remoteCapabilities & localCapabilities;
        return new LiveCapabilityNegotiation(remoteCapabilities, localCapabilities, negotiated, unsupported);
    }
}

public enum LiveCapabilityParseResult
{
    Legacy,
    Parsed,
    Rejected,
}

public static class LiveControlCapabilitiesParser
{
    public static LiveCapabilityParseResult Apply(
        ReadOnlySpan<byte> controlPayload,
        ulong localCapabilities,
        out LiveCapabilityNegotiation negotiation)
    {
        negotiation = LiveCapabilityNegotiation.Negotiate(0, localCapabilities);
        if (controlPayload.Length < LiveConstants.ControlMinPayloadSize)
            return LiveCapabilityParseResult.Legacy;

        if (!LiveControlCapabilities.TryRead(controlPayload[LiveConstants.ControlBasePayloadSize..], out var capabilities))
            return LiveCapabilityParseResult.Legacy;

        negotiation = LiveCapabilityNegotiation.Negotiate(capabilities.CapabilityMask, localCapabilities);
        return LiveCapabilityParseResult.Parsed;
    }
}
