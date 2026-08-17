namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumMismatchPolicyTests
{
    [Fact]
    public void ShouldTreatAsValid_ReportsMismatchWhenCompared()
    {
        var result = new SnapshotChecksumApplyResult(compared: true, mismatchCount: 2, localBucketMissing: false);
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTreatAsValid(
            result,
            SnapshotChecksumMismatchPolicyKind.ReportAllCompared));
    }

    [Fact]
    public void ShouldTreatAsValid_IgnoresWhenLocalBucketMissing()
    {
        var result = new SnapshotChecksumApplyResult(compared: false, mismatchCount: 0, localBucketMissing: true);
        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTreatAsValid(
            result,
            SnapshotChecksumMismatchPolicyKind.IgnoreWhenLocalBucketMissing));
    }

    [Fact]
    public void ShouldResyncNetState_ReturnsTrueWhenComparedMismatch()
    {
        var result = new SnapshotChecksumApplyResult(compared: true, mismatchCount: 1, localBucketMissing: false);
        Assert.True(SnapshotChecksumMismatchPolicy.ShouldResyncNetState(
            result,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }
}
