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
    }

    public bool Compared { get; }
    public int MismatchCount { get; }
    public bool LocalBucketMissing { get; }
    public bool HasActorCategoryMismatch { get; }
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
}
