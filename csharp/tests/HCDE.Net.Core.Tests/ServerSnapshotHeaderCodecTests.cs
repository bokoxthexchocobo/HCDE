using System.Buffers.Binary;

namespace HCDE.Net.Core.Tests;

public class ServerSnapshotHeaderCodecTests
{
    [Fact]
    public void ServerSnapshotHeader_RoundTrip_MatchesCppLayout()
    {
        var header = new ServerSnapshotHeader(
            controlFlags: 0,
            routingByte: 2,
            playerCount: 2,
            sequenceAck: 50,
            consistencyAck: 60,
            quitterBytes: 2,
            baseSequence: 70,
            baseConsistency: 80,
            commandTics: 3,
            consistencyTics: 3,
            stabilityBuffer: 17,
            bodyBytes: 6);

        Span<byte> buffer = stackalloc byte[LiveConstants.ServerSnapshotHeaderSize + 2];
        Assert.Equal(LiveConstants.ServerSnapshotHeaderSize, ServerSnapshotHeader.Write(buffer, header));
        Assert.True(ServerSnapshotHeader.LooksLikeHeader(buffer));
        Assert.True(ServerSnapshotHeader.TryRead(buffer, out var parsed));
        Assert.Equal(header.QuitterBytes, parsed.QuitterBytes);
        Assert.Equal(header.BodyBytes, parsed.BodyBytes);
        Assert.Equal(50u, BinaryPrimitives.ReadUInt32BigEndian(buffer[8..]));
    }

    [Fact]
    public void ServerSnapshotHeader_Validate_RejectsLengthMismatch()
    {
        var header = new ServerSnapshotHeader(
            0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 17, bodyBytes: 10);
        Assert.False(ServerSnapshotHeader.ValidateHeader(header, LiveConstants.ServerSnapshotHeaderSize + 5, out var reason));
        Assert.Equal("server-snapshot-body-length-mismatch", reason);
    }

    [Fact]
    public void ServerSnapshotRecordsHeader_RoundTrip()
    {
        var records = new ServerSnapshotRecordsHeader(playerCount: 1);
        Span<byte> buffer = stackalloc byte[LiveConstants.ServerSnapshotRecordsHeaderSize];
        ServerSnapshotRecordsHeader.Write(buffer, records);
        Assert.True(ServerSnapshotRecordsHeader.TryRead(buffer, out var parsed));
        Assert.Equal((byte)1, parsed.PlayerCount);
        Assert.True(buffer[..4].SequenceEqual(LiveConstants.ServerSnapshotRecordsMagic));
    }
}
