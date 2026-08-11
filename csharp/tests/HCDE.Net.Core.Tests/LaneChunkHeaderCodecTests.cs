namespace HCDE.Net.Core.Tests;

public class LaneChunkHeaderCodecTests
{
    [Fact]
    public void WorldDeltaHeader_RoundTrip()
    {
        var header = new ServerWorldDeltaHeader(flags: 1, gameTic: 99, recordCount: 3);
        Span<byte> buffer = stackalloc byte[LiveConstants.ServerWorldDeltaHeaderSize];
        ServerWorldDeltaHeader.Write(buffer, header);
        Assert.True(ServerWorldDeltaHeader.TryRead(buffer, out var parsed));
        Assert.Equal(header.GameTic, parsed.GameTic);
        Assert.Equal(header.RecordCount, parsed.RecordCount);
    }

    [Fact]
    public void AuthorityEventsHeader_RoundTrip()
    {
        var header = new AuthorityEventsHeader(flags: 0, eventCount: 2);
        Span<byte> buffer = stackalloc byte[LiveConstants.AuthorityEventsHeaderSize];
        AuthorityEventsHeader.Write(buffer, header);
        Assert.True(AuthorityEventsHeader.TryRead(buffer, out var parsed));
        Assert.Equal((byte)2, parsed.EventCount);
    }
}
