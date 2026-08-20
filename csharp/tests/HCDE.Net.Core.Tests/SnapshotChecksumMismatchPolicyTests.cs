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
    public void ShouldTriggerNetGapResyncOnCoopLineSpecActorMismatch_RequiresAppliedCoopLineSpec()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopLineSpecActorMismatch(
            result,
            appliedCoopLineSpec: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopLineSpecActorMismatch(
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
    public void ShouldTriggerNetGapResyncOnInvasionLineSpecActorMismatch_RequiresAppliedInvasionLineSpec()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionLineSpecActorMismatch(
            result,
            appliedInvasionLineSpec: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionLineSpecActorMismatch(
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

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionActorDeltaLineSpecMismatch_RequiresAppliedInvasionActorDeltas()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionActorDeltaLineSpecMismatch(
            result,
            appliedInvasionActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionActorDeltaLineSpecMismatch(
            result,
            appliedInvasionActorDeltas: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopActorDeltaMismatch_RequiresAppliedCoopActorDeltas()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopActorDeltaMismatch(
            result,
            appliedCoopActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopActorDeltaMismatch(
            result,
            appliedCoopActorDeltas: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopActorDeltaLineSpecMismatch_RequiresAppliedCoopActorDeltas()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopActorDeltaLineSpecMismatch(
            result,
            appliedCoopActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopActorDeltaLineSpecMismatch(
            result,
            appliedCoopActorDeltas: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventMismatch_RequiresAppliedInvasionAuthorityEvents()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventMismatch(
            result,
            appliedInvasionAuthorityEvents: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventLineSpecMismatch_RequiresAppliedInvasionAuthorityEvents()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventMismatch_RequiresAppliedCoopAuthorityEvents()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventMismatch(
            result,
            appliedCoopAuthorityEvents: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventLineSpecMismatch_RequiresAppliedCoopAuthorityEvents()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventPresentationEchoLineSpecMismatch_RequiresAppliedInvasionAuthorityEventsAndPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: false,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventPresentationEchoLineSpecMismatch_RequiresAppliedCoopAuthorityEventsAndPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventPresentationEchoLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventPresentationEchoLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventPresentationEchoLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: false,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionPresentationEchoMismatch_RequiresAppliedInvasionPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionPresentationEchoMismatch(
            result,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionPresentationEchoMismatch(
            result,
            appliedInvasionPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionPresentationEchoLineSpecMismatch_RequiresAppliedInvasionPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopPresentationEchoMismatch_RequiresAppliedCoopPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopPresentationEchoMismatch(
            result,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopPresentationEchoMismatch(
            result,
            appliedCoopPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopPresentationEchoLineSpecMismatch_RequiresAppliedCoopPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopPresentationEchoLineSpecMismatch(
            result,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopPresentationEchoLineSpecMismatch(
            result,
            appliedCoopPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }
}
