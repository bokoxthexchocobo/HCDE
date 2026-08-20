namespace HCDE.Net.Core;

public enum SnapshotChecksumMismatchPolicyKind
{
    ReportAllCompared,
    IgnoreWhenLocalBucketMissing,
    ResyncNetStateOnMismatch,
}

public readonly struct GuestChecksumApplyState
{
    public GuestChecksumApplyState(SnapshotChecksumApplyResult result)
    {
        Compared = result.Compared;
        MismatchCount = result.MismatchCount;
        LocalBucketMissing = result.LocalBucketMissing;
        HasActorCategoryMismatch = result.HasActorCategoryMismatch;
        HasLineSpecCategoryMismatch = result.HasLineSpecCategoryMismatch;
    }

    public bool Compared { get; }
    public int MismatchCount { get; }
    public bool LocalBucketMissing { get; }
    public bool HasActorCategoryMismatch { get; }
    public bool HasLineSpecCategoryMismatch { get; }
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
            SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch
                => result.MismatchCount == 0,
            _ => result.MismatchCount == 0,
        };
    }

    public static bool ShouldResyncNetState(
        SnapshotChecksumApplyResult result,
        SnapshotChecksumMismatchPolicyKind policy) =>
        policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch
        && result.Compared
        && result.MismatchCount > 0
        && !result.LocalBucketMissing;

    public static bool ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(
        SnapshotChecksumApplyResult result,
        SnapshotChecksumMismatchPolicyKind policy) =>
        policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch
        && result.Compared
        && result.HasActorCategoryMismatch
        && !result.LocalBucketMissing;

    public static bool ShouldTriggerNetGapResyncOnLineSpecMismatch(
        SnapshotChecksumApplyResult result,
        SnapshotChecksumMismatchPolicyKind policy) =>
        policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch
        && result.Compared
        && result.HasLineSpecCategoryMismatch
        && !result.LocalBucketMissing;

    public static bool ShouldTriggerNetGapResyncOnInvasionCoopDeadSpawnApply(
        bool appliedInvasionCoopDeadSpawns,
        bool hasChecksum,
        SnapshotChecksumMismatchPolicyKind policy) =>
        appliedInvasionCoopDeadSpawns
        && !hasChecksum
        && policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;

    public static bool ShouldTriggerNetGapResyncOnInvasionAuthorityEventApply(
        bool appliedInvasionAuthorityEvents,
        bool hasChecksum,
        SnapshotChecksumMismatchPolicyKind policy) =>
        appliedInvasionAuthorityEvents
        && !hasChecksum
        && policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;

    public static bool ShouldTriggerNetGapResyncOnInvasionActorDeltaApply(
        bool appliedInvasionActorDeltas,
        bool hasChecksum,
        SnapshotChecksumMismatchPolicyKind policy) =>
        appliedInvasionActorDeltas
        && !hasChecksum
        && policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;

    public static bool ShouldTriggerNetGapResyncOnInvasionPresentationEchoApply(
        bool appliedInvasionPresentationEcho,
        bool hasChecksum,
        SnapshotChecksumMismatchPolicyKind policy) =>
        appliedInvasionPresentationEcho
        && !hasChecksum
        && policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;

    public static bool ShouldTriggerNetGapResyncOnPresentationEchoApply(
        bool appliedPresentationEcho,
        bool hasChecksum,
        SnapshotChecksumMismatchPolicyKind policy) =>
        appliedPresentationEcho
        && !hasChecksum
        && policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;

    public static bool ShouldTriggerNetGapResyncOnInvasionLineSpecApply(
        bool appliedInvasionLineSpec,
        bool hasChecksum,
        SnapshotChecksumMismatchPolicyKind policy) =>
        appliedInvasionLineSpec
        && !hasChecksum
        && policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;

    public static bool ShouldTriggerNetGapResyncOnCoopLineSpecApply(
        bool appliedCoopLineSpec,
        bool hasChecksum,
        SnapshotChecksumMismatchPolicyKind policy) =>
        appliedCoopLineSpec
        && !hasChecksum
        && policy == SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;

    public static bool ShouldTriggerNetGapResyncOnInvasionLineSpecMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedInvasionLineSpec,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnLineSpecMismatch(result, policy)
        && appliedInvasionLineSpec;

    public static bool ShouldTriggerNetGapResyncOnInvasionLineSpecActorMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedInvasionLineSpec,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(result, policy)
        && appliedInvasionLineSpec;

    public static bool ShouldTriggerNetGapResyncOnCoopLineSpecMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedCoopLineSpec,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnLineSpecMismatch(result, policy)
        && appliedCoopLineSpec;

    public static bool ShouldTriggerNetGapResyncOnCoopLineSpecActorMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedCoopLineSpec,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(result, policy)
        && appliedCoopLineSpec;

    public static bool ShouldTriggerNetGapResyncOnInvasionActorDeltaMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedInvasionActorDeltas,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(result, policy)
        && appliedInvasionActorDeltas;

    public static bool ShouldTriggerNetGapResyncOnCoopActorDeltaMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedCoopActorDeltas,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(result, policy)
        && appliedCoopActorDeltas;

    public static bool ShouldTriggerNetGapResyncOnInvasionAuthorityEventMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedInvasionAuthorityEvents,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(result, policy)
        && appliedInvasionAuthorityEvents;

    public static bool ShouldTriggerNetGapResyncOnInvasionAuthorityEventLineSpecMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedInvasionAuthorityEvents,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnLineSpecMismatch(result, policy)
        && appliedInvasionAuthorityEvents;

    public static bool ShouldTriggerNetGapResyncOnCoopAuthorityEventMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedCoopAuthorityEvents,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(result, policy)
        && appliedCoopAuthorityEvents;

    public static bool ShouldTriggerNetGapResyncOnCoopAuthorityEventLineSpecMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedCoopAuthorityEvents,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnLineSpecMismatch(result, policy)
        && appliedCoopAuthorityEvents;

    public static bool ShouldTriggerNetGapResyncOnInvasionPresentationEchoMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedInvasionPresentationEcho,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(result, policy)
        && appliedInvasionPresentationEcho;

    public static bool ShouldTriggerNetGapResyncOnInvasionPresentationEchoLineSpecMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedInvasionPresentationEcho,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnLineSpecMismatch(result, policy)
        && appliedInvasionPresentationEcho;

    public static bool ShouldTriggerNetGapResyncOnCoopPresentationEchoMismatch(
        SnapshotChecksumApplyResult result,
        bool appliedCoopPresentationEcho,
        SnapshotChecksumMismatchPolicyKind policy) =>
        ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(result, policy)
        && appliedCoopPresentationEcho;
}
