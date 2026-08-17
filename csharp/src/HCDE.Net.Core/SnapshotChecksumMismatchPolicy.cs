namespace HCDE.Net.Core;

public enum SnapshotChecksumMismatchPolicyKind
{
    ReportAllCompared,
    IgnoreWhenLocalBucketMissing,
}

public readonly struct GuestChecksumApplyState
{
    public GuestChecksumApplyState(SnapshotChecksumApplyResult result)
    {
        Compared = result.Compared;
        MismatchCount = result.MismatchCount;
        LocalBucketMissing = result.LocalBucketMissing;
    }

    public bool Compared { get; }
    public int MismatchCount { get; }
    public bool LocalBucketMissing { get; }
}

public static class SnapshotChecksumMismatchPolicy
{
    public static bool ShouldTreatAsValid(
        SnapshotChecksumApplyResult result,
        SnapshotChecksumMismatchPolicyKind policy)
    {
        return policy switch
        {
            SnapshotChecksumMismatchPolicyKind.IgnoreWhenLocalBucketMissing
                => result.MismatchCount == 0 || result.LocalBucketMissing,
            _ => result.MismatchCount == 0,
        };
    }
}
