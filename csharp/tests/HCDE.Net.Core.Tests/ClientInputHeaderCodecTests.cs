using System.Buffers.Binary;

namespace HCDE.Net.Core.Tests;

public class ClientInputHeaderCodecTests
{
    [Fact]
    public void ClientInputHeader_RoundTrip_MatchesCppLayout()
    {
        var header = new ClientInputHeader(
            controlFlags: 0,
            routingByte: 1,
            playerCount: 1,
            sequenceAck: 100,
            consistencyAck: 200,
            baseSequence: 300,
            baseConsistency: 400,
            commandTics: 2,
            consistencyTics: 2,
            stabilityBuffer: 17,
            bodyBytes: 6);

        Span<byte> buffer = stackalloc byte[LiveConstants.ClientInputHeaderSize];
        Assert.Equal(LiveConstants.ClientInputHeaderSize, ClientInputHeader.Write(buffer, header));
        Assert.True(ClientInputHeader.LooksLikeHeader(buffer));
        Assert.True(ClientInputHeader.TryRead(buffer, out var parsed));
        Assert.Equal(header.PlayerCount, parsed.PlayerCount);
        Assert.Equal(header.BodyBytes, parsed.BodyBytes);
        Assert.Equal(100u, BinaryPrimitives.ReadUInt32BigEndian(buffer[8..]));
    }

    [Fact]
    public void ClientInputHeader_Validate_RejectsDisallowedFlags()
    {
        var header = new ClientInputHeader(
            controlFlags: (byte)HCDE.Net.Transport.NetCommandFlags.Setup,
            routingByte: 0,
            playerCount: 1,
            sequenceAck: 0,
            consistencyAck: 0,
            baseSequence: 0,
            baseConsistency: 0,
            commandTics: 0,
            consistencyTics: 0,
            stabilityBuffer: 0,
            bodyBytes: 6);

        Assert.False(ClientInputHeader.ValidateHeader(header, LiveConstants.ClientInputHeaderSize + 6, out var reason));
        Assert.Equal("client-input-disallowed-control-flags", reason);
    }

    [Fact]
    public void ClientInputRecordsHeader_MustMatchHeaderPlayerCount()
    {
        var records = new ClientInputRecordsHeader(playerCount: 2);
        Assert.False(ClientInputRecordsHeader.Validate(records, headerPlayerCount: 1, out var reason));
        Assert.Equal("client-input-record-player-count-mismatch", reason);
    }
}
