using HCDE.Net.Core;

namespace HCDE.Net.Core.Tests;

public class ServerSnapshotChecksumTailTests
{
    [Fact]
    public void BuildServerSnapshot_WithChecksumTail_RoundTripsThroughWalker()
    {
        var checksumHashes = new uint[] { 1, 2, 3, 4, 5, 6 };
        Span<byte> payload = stackalloc byte[512];
        var written = GameplayPayloadBuilders.BuildServerSnapshotSinglePlayer(
            payload,
            playerNum: 0,
            command: default,
            includeMinimalTail: true,
            gameTic: 12,
            checksumHashes: checksumHashes);
        Assert.True(written > LiveConstants.ServerSnapshotHeaderSize);

        Assert.True(ServerSnapshotHeader.TryRead(payload[..written], out var header));
        Assert.True(ServerSnapshotBodyCodec.TryReadPlayerRecords(
            payload[(LiveConstants.ServerSnapshotHeaderSize + header.QuitterBytes)..written],
            header.ConsistencyTics,
            header.CommandTics,
            out _,
            out var hcsrBytes,
            out _));

        var tail = payload[(LiveConstants.ServerSnapshotHeaderSize + header.QuitterBytes + hcsrBytes)..written];
        Assert.True(ServerSnapshotTailWalker.TryWalk(tail, out var sections, out _, out _));
        Assert.True(sections.HasChecksum);
        Assert.Equal(12u, sections.ChecksumGameTic);
        Assert.Equal(checksumHashes, sections.ChecksumHashes);
    }
}
