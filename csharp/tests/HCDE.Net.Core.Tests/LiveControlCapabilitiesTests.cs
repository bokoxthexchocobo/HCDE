using System.Buffers.Binary;

namespace HCDE.Net.Core.Tests;

public class LiveControlCapabilitiesTests
{
    [Fact]
    public void Capabilities_MinSizeBlock_RoundTrip()
    {
        var capabilities = new LiveControlCapabilities(LiveConstants.DefaultLocalCapabilities);
        Span<byte> buffer = stackalloc byte[LiveConstants.ControlCapabilitiesMinSize];
        Assert.Equal(LiveConstants.ControlCapabilitiesMinSize, LiveControlCapabilities.Write(buffer, capabilities));
        Assert.True(LiveControlCapabilities.TryRead(buffer, out var parsed));
        Assert.Equal(capabilities.CapabilityMask, parsed.CapabilityMask);
        Assert.Null(parsed.SessionId);
        Assert.True(buffer[..4].SequenceEqual("HCAP"u8));
    }

    [Fact]
    public void Capabilities_FullBlock_IncludesSessionId()
    {
        var capabilities = new LiveControlCapabilities(LiveConstants.CapClientInputV5, sessionId: 0xAABBCCDD);
        Span<byte> buffer = stackalloc byte[LiveConstants.ControlCapabilitiesFullSize];
        Assert.Equal(LiveConstants.ControlCapabilitiesFullSize, LiveControlCapabilities.Write(buffer, capabilities));
        Assert.True(LiveControlCapabilities.TryRead(buffer, out var parsed));
        Assert.Equal(0xAABBCCDDu, parsed.SessionId);
        Assert.Equal((byte)LiveControlCapabilityFlags.SessionId, buffer[5]);
    }

    [Fact]
    public void ControlPayload_MatchesSendHCDELiveControlLayout()
    {
        var basePayload = new LiveControlBasePayload(gameTic: 1234, consolePlayer: 0, maxClients: 4);
        var capabilities = new LiveControlCapabilities(LiveConstants.DefaultLocalCapabilities, sessionId: 99);
        Span<byte> payload = stackalloc byte[LiveConstants.ControlFullPayloadSize];
        Assert.Equal(LiveConstants.ControlFullPayloadSize, LiveControlPayloadCodec.Write(payload, basePayload, capabilities));

        Assert.True(LiveControlPayloadCodec.TryRead(payload, out var parsedBase, out var parsedCaps));
        Assert.Equal(1234u, parsedBase.GameTic);
        Assert.Equal((byte)0, parsedBase.ConsolePlayer);
        Assert.Equal((byte)4, parsedBase.MaxClients);
        Assert.NotNull(parsedCaps);
        Assert.Equal(LiveConstants.DefaultLocalCapabilities, parsedCaps!.Value.CapabilityMask);
        Assert.Equal(99u, parsedCaps.Value.SessionId);
        Assert.Equal(1234u, BinaryPrimitives.ReadUInt32BigEndian(payload));
    }

    [Fact]
    public void CapabilityParser_LegacyPayloadWhenCapabilitiesMissing()
    {
        Span<byte> payload = stackalloc byte[LiveConstants.ControlBasePayloadSize];
        LiveControlBasePayload.Write(payload, new LiveControlBasePayload(1, 0, 2));
        var result = LiveControlCapabilitiesParser.Apply(payload, LiveConstants.DefaultLocalCapabilities, out var negotiation);
        Assert.Equal(LiveCapabilityParseResult.Legacy, result);
        Assert.Equal(0UL, negotiation.Remote);
    }

    [Fact]
    public void CapabilityParser_NegotiatesIntersection()
    {
        Span<byte> payload = stackalloc byte[LiveConstants.ControlMinPayloadSize];
        LiveControlBasePayload.Write(payload, new LiveControlBasePayload(1, 0, 2));
        LiveControlCapabilities.Write(
            payload[LiveConstants.ControlBasePayloadSize..],
            new LiveControlCapabilities(LiveConstants.CapControlV1 | LiveConstants.CapClientInputV5 | (1UL << 63)));

        var result = LiveControlCapabilitiesParser.Apply(payload, LiveConstants.DefaultLocalCapabilities, out var negotiation);
        Assert.Equal(LiveCapabilityParseResult.Parsed, result);
        Assert.Equal(LiveConstants.CapControlV1 | LiveConstants.CapClientInputV5 | (1UL << 63), negotiation.Remote);
        Assert.Equal(LiveConstants.CapControlV1 | LiveConstants.CapClientInputV5, negotiation.Negotiated);
        Assert.Equal(1UL << 63, negotiation.Unsupported);
    }
}
