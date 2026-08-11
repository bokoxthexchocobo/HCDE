namespace HCDE.Net.Core.Tests;

public class LiveControlPacketTests
{
    [Fact]
    public void BuildAndParseControlPacket_NegotiatesCapabilities()
    {
        var packet = LiveControlPacketBuilder.BuildControl(
            txSequence: 1,
            acknowledgement: 0,
            new LiveControlBasePayload(gameTic: 42, consolePlayer: 0, maxClients: 2),
            new LiveControlCapabilities(LiveConstants.DefaultLocalCapabilities, sessionId: 7));

        Span<byte> netBuffer = stackalloc byte[LiveConstants.HeaderSize + LiveConstants.ControlFullPayloadSize];
        Assert.Equal(LiveConstants.HeaderSize + LiveConstants.ControlFullPayloadSize, LivePacket.Write(netBuffer, packet));
        Assert.True(LivePacket.TryRead(netBuffer, out var parsed));

        Assert.True(LiveControlPacketBuilder.TryParseControl(parsed, out var basePayload, out var caps, out var negotiation));
        Assert.Equal(42u, basePayload.GameTic);
        Assert.NotNull(caps);
        Assert.NotNull(negotiation);
        Assert.Equal(LiveConstants.DefaultLocalCapabilities, negotiation!.Value.Negotiated);
    }

    [Fact]
    public void LiveControlScheduler_RespectsOneSecondCadence()
    {
        var scheduler = new LiveControlScheduler();
        Assert.True(scheduler.ShouldSendControl(1000));
        Assert.False(scheduler.ShouldSendControl(1500));
        Assert.True(scheduler.ShouldSendControl(2000));
    }

    [Fact]
    public void LivePacket_ControlRoundTripPreservesHeader()
    {
        var built = LiveControlPacketBuilder.BuildControl(5, 3, new LiveControlBasePayload(10, 0, 4));
        Span<byte> buffer = stackalloc byte[512];
        var length = LivePacket.Write(buffer, built);
        Assert.True(LivePacket.TryRead(buffer[..length], out var read));
        Assert.Equal(LiveMessageType.Control, read.Header.MessageType);
        Assert.Equal(5u, read.Header.TxSequence);
        Assert.Equal(3u, read.Header.Acknowledgement);
    }
}
