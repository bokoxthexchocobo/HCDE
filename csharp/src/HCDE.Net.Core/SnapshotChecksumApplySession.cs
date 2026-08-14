namespace HCDE.Net.Core;

public interface ISnapshotChecksumMismatchSink
{
    void ReportMismatch(SnapshotChecksumMismatch mismatch, uint remoteTic);
}

public readonly struct SnapshotChecksumApplyResult
{
    public SnapshotChecksumApplyResult(bool compared, int mismatchCount, bool localBucketMissing)
    {
        Compared = compared;
        MismatchCount = mismatchCount;
        LocalBucketMissing = localBucketMissing;
    }

    public bool Compared { get; }
    public int MismatchCount { get; }
    public bool LocalBucketMissing { get; }
}

public static class SnapshotChecksumApplySession
{
    public static bool TryApply(
        uint remoteTic,
        uint[] remoteHashes,
        SnapshotChecksumRing ring,
        bool checksumEnabled,
        byte enabledCategoryMask,
        ISnapshotChecksumMismatchSink? mismatchSink,
        out SnapshotChecksumApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;

        if (!checksumEnabled)
            return true;

        if (remoteHashes.Length != LiveConstants.SnapshotChecksumCategoryCount)
        {
            rejectReason = "checksum-hash-count-mismatch";
            return false;
        }

        if (!ring.TryFind((int)remoteTic, out var localHashes))
        {
            result = new SnapshotChecksumApplyResult(compared: false, mismatchCount: 0, localBucketMissing: true);
            return true;
        }

        var mismatchList = new List<SnapshotChecksumMismatch>();
        for (var i = 0; i < LiveConstants.SnapshotChecksumCategoryCount; i++)
        {
            if ((enabledCategoryMask & (1 << i)) == 0)
                continue;

            if (remoteHashes[i] == localHashes[i])
                continue;

            var mismatch = new SnapshotChecksumMismatch(
                (SnapshotChecksumCategory)i,
                remoteHashes[i],
                localHashes[i]);
            mismatchList.Add(mismatch);
            mismatchSink?.ReportMismatch(mismatch, remoteTic);
        }

        result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: mismatchList.Count,
            localBucketMissing: false);
        return true;
    }
}
