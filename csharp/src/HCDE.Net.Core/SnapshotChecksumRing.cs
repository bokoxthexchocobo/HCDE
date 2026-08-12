namespace HCDE.Net.Core;

public readonly struct SnapshotChecksumMismatch
{
    public SnapshotChecksumMismatch(SnapshotChecksumCategory category, uint serverHash, uint localHash)
    {
        Category = category;
        ServerHash = serverHash;
        LocalHash = localHash;
    }

    public SnapshotChecksumCategory Category { get; }
    public uint ServerHash { get; }
    public uint LocalHash { get; }
}

public enum SnapshotChecksumCategory : byte
{
    Players = 0,
    Sectors = 1,
    Movers = 2,
    Actors = 3,
    Rng = 4,
    LineSpec = 5,
}

public readonly struct SnapshotChecksumBucket
{
    public SnapshotChecksumBucket(int gameTic, uint[] categoryHashes)
    {
        GameTic = gameTic;
        CategoryHashes = categoryHashes;
    }

    public int GameTic { get; }
    public uint[] CategoryHashes { get; }
}

public sealed class SnapshotChecksumRing
{
    public const int HistoryDepth = 64;
    public const byte DefaultEnabledCategoryMask = 0x3F;

    private readonly SnapshotChecksumBucket?[] _history = new SnapshotChecksumBucket?[HistoryDepth];
    private int _cursor;

    public void Reset()
    {
        Array.Clear(_history);
        _cursor = 0;
    }

    public void Store(int gameTic, ReadOnlySpan<uint> categoryHashes)
    {
        if (categoryHashes.Length != LiveConstants.SnapshotChecksumCategoryCount)
            throw new ArgumentException("category hash count mismatch", nameof(categoryHashes));

        _history[_cursor] = new SnapshotChecksumBucket(gameTic, categoryHashes.ToArray());
        _cursor = (_cursor + 1) % HistoryDepth;
    }

    public bool TryFind(int gameTic, out uint[] categoryHashes)
    {
        categoryHashes = Array.Empty<uint>();
        foreach (var bucket in _history)
        {
            if (bucket is { GameTic: var tic } && tic == gameTic)
            {
                categoryHashes = bucket.Value.CategoryHashes;
                return true;
            }
        }

        return false;
    }

    public bool TryReadAndCompare(
        ReadOnlySpan<byte> chunk,
        ref int cursor,
        uint serverTic,
        bool checksumEnabled,
        byte enabledCategoryMask,
        out SnapshotChecksumMismatch[] mismatches,
        out string? rejectReason)
    {
        mismatches = Array.Empty<SnapshotChecksumMismatch>();
        rejectReason = null;

        if (!checksumEnabled)
            return true;

        if (cursor >= chunk.Length)
            return true;

        if (!chunk[cursor..].StartsWith(LiveConstants.SnapshotChecksumMagic))
            return true;

        if (!SnapshotChecksumCodec.TryRead(chunk[cursor..], out var remoteTic, out var remoteHashes, out var bytesConsumed, out rejectReason))
            return false;

        cursor += bytesConsumed;
        if (!TryFind((int)remoteTic, out var localHashes))
            return true;

        var mismatchList = new List<SnapshotChecksumMismatch>();
        for (var i = 0; i < LiveConstants.SnapshotChecksumCategoryCount; i++)
        {
            if ((enabledCategoryMask & (1 << i)) == 0)
                continue;

            if (remoteHashes[i] == localHashes[i])
                continue;

            mismatchList.Add(new SnapshotChecksumMismatch((SnapshotChecksumCategory)i, remoteHashes[i], localHashes[i]));
        }

        mismatches = mismatchList.ToArray();
        return true;
    }
}
