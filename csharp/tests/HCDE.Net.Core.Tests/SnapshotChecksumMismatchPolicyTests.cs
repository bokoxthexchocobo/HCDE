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

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopLineSpecMismatch_RequiresAppliedCoopLineSpec()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopLineSpecMismatch(
            result,
            appliedCoopLineSpec: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopLineSpecMismatch(
            result,
            appliedCoopLineSpec: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionLineSpecMismatch_RequiresAppliedInvasionLineSpec()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionLineSpecMismatch(
            result,
            appliedInvasionLineSpec: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionLineSpecMismatch(
            result,
            appliedInvasionLineSpec: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionActorDeltaMismatch_RequiresAppliedInvasionActorDeltas()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionActorDeltaMismatch(
            result,
            appliedInvasionActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionActorDeltaMismatch(
            result,
            appliedInvasionActorDeltas: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }
}
