namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumRingTests
{
    [Fact]
    public void StoreAndFind_ReturnsMatchingBucket()
    {
        var ring = new SnapshotChecksumRing();
        var hashes = new uint[] { 1, 2, 3, 4, 5, 6 };
        ring.Store(gameTic: 42, hashes);

        Assert.True(ring.TryFind(42, out var found));
        Assert.Equal(hashes, found);
    }

    [Fact]
    public void TryReadAndCompare_ReportsMismatchForEnabledCategory()
    {
        var ring = new SnapshotChecksumRing();
        var localHashes = new uint[] { 1, 2, 3, 4, 5, 6 };
        var remoteHashes = new uint[] { 1, 99, 3, 4, 5, 6 };
        ring.Store(gameTic: 7, localHashes);

        Span<byte> tail = stackalloc byte[ServerSnapshotTailCodec.MinimalTailWithChecksumSize];
        Assert.Equal(ServerSnapshotTailCodec.MinimalTailWithChecksumSize, ServerSnapshotTailCodec.WriteMinimal(tail, gameTic: 7, remoteHashes));

        var cursor = ServerSnapshotTailCodec.MinimalTailSize;
        Assert.True(ring.TryReadAndCompare(
            tail,
            ref cursor,
            serverTic: 7,
            checksumEnabled: true,
            SnapshotChecksumRing.DefaultEnabledCategoryMask,
            out var mismatches,
            out _));

        Assert.Equal(tail.Length, cursor);
        Assert.Single(mismatches);
        Assert.Equal(SnapshotChecksumCategory.Sectors, mismatches[0].Category);
        Assert.Equal(99u, mismatches[0].ServerHash);
        Assert.Equal(2u, mismatches[0].LocalHash);
    }

    [Fact]
    public void TryReadAndCompare_SkipsWhenLocalBucketMissing()
    {
        var ring = new SnapshotChecksumRing();
        Span<byte> tail = stackalloc byte[ServerSnapshotTailCodec.MinimalTailWithChecksumSize];
        var remoteHashes = new uint[] { 1, 2, 3, 4, 5, 6 };
        Assert.Equal(ServerSnapshotTailCodec.MinimalTailWithChecksumSize, ServerSnapshotTailCodec.WriteMinimal(tail, gameTic: 8, remoteHashes));

        var cursor = ServerSnapshotTailCodec.MinimalTailSize;
        Assert.True(ring.TryReadAndCompare(
            tail,
            ref cursor,
            serverTic: 8,
            checksumEnabled: true,
            SnapshotChecksumRing.DefaultEnabledCategoryMask,
            out var mismatches,
            out _));

        Assert.Empty(mismatches);
    }
}
