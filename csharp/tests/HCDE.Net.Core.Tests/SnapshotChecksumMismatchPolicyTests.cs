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
    public void ShouldTriggerNetGapResyncOnInvasionActorDeltaPresentationEchoLineSpecMismatch_RequiresAppliedInvasionActorDeltasAndPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionActorDeltas: false,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: false,
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
    public void ShouldTriggerNetGapResyncOnCoopActorDeltaPresentationEchoLineSpecMismatch_RequiresAppliedCoopActorDeltasAndPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedCoopActorDeltas: false,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: false,
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
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoMismatch_RequiresAppliedInvasionAuthorityEventsActorDeltasAndPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: false,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoMismatch(
            result,
            appliedInvasionAuthorityEvents: false,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
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
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaLineSpecMismatch_RequiresAppliedInvasionAuthorityEventsAndActorDeltas()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: false,
            appliedInvasionActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaMismatch_RequiresAppliedInvasionAuthorityEventsAndActorDeltas()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaMismatch(
            result,
            appliedInvasionAuthorityEvents: false,
            appliedInvasionActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoLineSpecMismatch_RequiresAppliedInvasionAuthorityEventsActorDeltasAndPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: false,
            appliedInvasionPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: false,
            appliedInvasionActorDeltas: true,
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
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaLineSpecMismatch_RequiresAppliedCoopAuthorityEventsAndActorDeltas()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: false,
            appliedCoopActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaMismatch_RequiresAppliedCoopAuthorityEventsAndActorDeltas()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaMismatch(
            result,
            appliedCoopAuthorityEvents: false,
            appliedCoopActorDeltas: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoLineSpecMismatch_RequiresAppliedCoopAuthorityEventsActorDeltasAndPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: false,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: false,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoMismatch_RequiresAppliedCoopAuthorityEventsActorDeltasAndPresentationEcho()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: false,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoMismatch(
            result,
            appliedCoopAuthorityEvents: false,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaCoopDeadSpawnMismatch_RequiresAppliedCoopAuthorityEventsActorDeltasAndCoopDeadSpawns()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaCoopDeadSpawnMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaCoopDeadSpawnMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaCoopDeadSpawnMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: false,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaCoopDeadSpawnMismatch(
            result,
            appliedCoopAuthorityEvents: false,
            appliedCoopActorDeltas: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch_RequiresAppliedCoopAuthorityEventsActorDeltasPresentationEchoAndCoopDeadSpawns()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: false,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: false,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaCoopDeadSpawnMismatch_RequiresAppliedInvasionAuthorityEventsActorDeltasAndCoopDeadSpawns()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaCoopDeadSpawnMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaCoopDeadSpawnMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaCoopDeadSpawnMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: false,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaCoopDeadSpawnMismatch(
            result,
            appliedInvasionAuthorityEvents: false,
            appliedInvasionActorDeltas: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch_RequiresAppliedInvasionAuthorityEventsActorDeltasPresentationEchoAndCoopDeadSpawns()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasActorCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: false,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: false,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch_RequiresAppliedInvasionAuthorityEventsActorDeltasPresentationEchoAndCoopDeadSpawns()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: false,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: false,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch_RequiresAppliedCoopAuthorityEventsActorDeltasPresentationEchoAndCoopDeadSpawns()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 1,
            localBucketMissing: false,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: false,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: false,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch_RequiresActorAndLineSpecCategoryMismatchesWithAllTailsApplied()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 2,
            localBucketMissing: false,
            hasActorCategoryMismatch: true,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch(
            new SnapshotChecksumApplyResult(
                compared: true,
                mismatchCount: 1,
                localBucketMissing: false,
                hasActorCategoryMismatch: true,
                hasLineSpecCategoryMismatch: false),
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch(
            new SnapshotChecksumApplyResult(
                compared: true,
                mismatchCount: 1,
                localBucketMissing: false,
                hasActorCategoryMismatch: false,
                hasLineSpecCategoryMismatch: true),
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch_RequiresActorAndLineSpecCategoryMismatchesWithAllTailsApplied()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 2,
            localBucketMissing: false,
            hasActorCategoryMismatch: true,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch(
            new SnapshotChecksumApplyResult(
                compared: true,
                mismatchCount: 1,
                localBucketMissing: false,
                hasActorCategoryMismatch: true,
                hasLineSpecCategoryMismatch: false),
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMismatch(
            new SnapshotChecksumApplyResult(
                compared: true,
                mismatchCount: 1,
                localBucketMissing: false,
                hasActorCategoryMismatch: false,
                hasLineSpecCategoryMismatch: true),
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatch_RequiresMultipleMismatchBucketsWithAllTailsApplied()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 2,
            localBucketMissing: false,
            hasActorCategoryMismatch: true,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatch(
            new SnapshotChecksumApplyResult(
                compared: true,
                mismatchCount: 1,
                localBucketMissing: false,
                hasActorCategoryMismatch: true,
                hasLineSpecCategoryMismatch: true),
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatch(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatchFollowUp_RequiresThreeMismatchBucketsWithAllTailsApplied()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 3,
            localBucketMissing: false,
            hasActorCategoryMismatch: true,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatchFollowUp(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatchFollowUp(
            new SnapshotChecksumApplyResult(
                compared: true,
                mismatchCount: 2,
                localBucketMissing: false,
                hasActorCategoryMismatch: true,
                hasLineSpecCategoryMismatch: true),
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatchFollowUp(
            result,
            appliedInvasionAuthorityEvents: true,
            appliedInvasionActorDeltas: true,
            appliedInvasionPresentationEcho: true,
            appliedInvasionCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatch_RequiresMultipleMismatchBucketsWithAllTailsApplied()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 2,
            localBucketMissing: false,
            hasActorCategoryMismatch: true,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatch(
            new SnapshotChecksumApplyResult(
                compared: true,
                mismatchCount: 1,
                localBucketMissing: false,
                hasActorCategoryMismatch: true,
                hasLineSpecCategoryMismatch: true),
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatch(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: false,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
    }

    [Fact]
    public void ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatchFollowUp_RequiresThreeMismatchBucketsWithAllTailsApplied()
    {
        var result = new SnapshotChecksumApplyResult(
            compared: true,
            mismatchCount: 3,
            localBucketMissing: false,
            hasActorCategoryMismatch: true,
            hasLineSpecCategoryMismatch: true);

        Assert.True(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatchFollowUp(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatchFollowUp(
            new SnapshotChecksumApplyResult(
                compared: true,
                mismatchCount: 2,
                localBucketMissing: false,
                hasActorCategoryMismatch: true,
                hasLineSpecCategoryMismatch: true),
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: true,
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch));
        Assert.False(SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnActorLineSpecMultiBucketMismatchFollowUp(
            result,
            appliedCoopAuthorityEvents: true,
            appliedCoopActorDeltas: true,
            appliedCoopPresentationEcho: true,
            appliedCoopDeadSpawns: false,
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
