namespace HCDE.Net.Core;

public interface IInvasionSnapshotApplySink
{
    InvasionMirrorState MirrorState { get; }

    bool ApplyMirror(
        InvasionSnapshotHeader header,
        uint waveSpawned,
        uint waveCleared);
}

public readonly struct InvasionSnapshotApplyResult
{
    public InvasionSnapshotApplyResult(
        bool mirrorApplied,
        int authorityRecords,
        int authorityApplied,
        int actorRecords,
        int actorApplied)
    {
        MirrorApplied = mirrorApplied;
        AuthorityRecords = authorityRecords;
        AuthorityApplied = authorityApplied;
        ActorRecords = actorRecords;
        ActorApplied = actorApplied;
    }

    public bool MirrorApplied { get; }
    public int AuthorityRecords { get; }
    public int AuthorityApplied { get; }
    public int ActorRecords { get; }
    public int ActorApplied { get; }
}

public static class InvasionSnapshotApplySession
{
    public static bool TryApply(
        InvasionSnapshotHeader header,
        AuthorityEventRecord[]? embeddedAuthorityRecords,
        ActorDeltasHeader embeddedActorHeader,
        IReadOnlyList<ActorDeltaRecord>? embeddedActorRecords,
        ulong negotiatedCapabilities,
        int recipientClientSlot,
        bool isLocalAuthority,
        IInvasionSnapshotApplySink? invasionSink,
        IAuthorityEventSink? authoritySink,
        IActorDeltaApplySink? actorSink,
        out InvasionSnapshotApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;

        if ((negotiatedCapabilities & LiveConstants.CapInvasionSnapshotV2) == 0)
        {
            rejectReason = "invasion-snapshot-capability-missing";
            return false;
        }

        if (!TryValidateHeader(header, out rejectReason))
            return false;

        var mirrorApplied = false;
        if (invasionSink != null)
        {
            var previous = invasionSink.MirrorState;
            var (waveSpawned, waveCleared) = InvasionSnapshotWavePolicy.ResolveWaveCounts(
                previous,
                header,
                isLocalAuthority);
            mirrorApplied = invasionSink.ApplyMirror(header, waveSpawned, waveCleared);
        }

        var authorityRecords = embeddedAuthorityRecords?.Length ?? 0;
        var authorityApplied = 0;
        if (authorityRecords > 0)
        {
            if ((negotiatedCapabilities & LiveConstants.CapAuthorityEventsV1) == 0)
            {
                rejectReason = "invasion-embedded-authority-capability-missing";
                return false;
            }

            if (authoritySink == null)
            {
                rejectReason = "invasion-embedded-authority-sink-missing";
                return false;
            }

            if (!AuthorityEventsApplySession.TryApply(
                    embeddedAuthorityRecords!,
                    authoritySink,
                    out var authorityResult,
                    out rejectReason))
            {
                return false;
            }

            authorityApplied = authorityResult.Applied;
        }

        var actorRecords = embeddedActorRecords?.Count ?? 0;
        var actorApplied = 0;
        if (actorRecords > 0)
        {
            if ((negotiatedCapabilities & LiveConstants.CapActorDeltaV2) == 0
                || (negotiatedCapabilities & LiveConstants.CapActorRegistryV1) == 0)
            {
                rejectReason = "invasion-embedded-actor-capability-missing";
                return false;
            }

            if (!ActorDeltasApplySession.TryApply(
                    embeddedActorHeader,
                    embeddedActorRecords!,
                    recipientClientSlot,
                    actorSink,
                    out var actorResult,
                    out rejectReason))
            {
                return false;
            }

            actorApplied = actorResult.Applied;
        }

        result = new InvasionSnapshotApplyResult(
            mirrorApplied,
            authorityRecords,
            authorityApplied,
            actorRecords,
            actorApplied);
        return true;
    }

    private static bool TryValidateHeader(InvasionSnapshotHeader header, out string? rejectReason)
    {
        rejectReason = null;
        if (header.ProtocolVersion != 1 && header.ProtocolVersion != LiveConstants.InvasionSnapshotProtocolVersion)
        {
            rejectReason = "invasion-snapshot-version-mismatch";
            return false;
        }

        if ((header.Flags & ~LiveConstants.InvasionSnapshotFlagBossWave) != 0)
        {
            rejectReason = "invasion-snapshot-invalid-flags";
            return false;
        }

        if (header.State > LiveConstants.InvasionStateFailure)
        {
            rejectReason = "invasion-snapshot-invalid-state";
            return false;
        }

        if (header.ProtocolVersion >= 2)
        {
            if ((header.SpawnFlags & ~LiveConstants.InvasionSnapshotSpawnFlagUsingFallback) != 0)
            {
                rejectReason = "invasion-snapshot-invalid-spawn-flags";
                return false;
            }

            if (header.SpawnFallbackSource > LiveConstants.InvasionSpawnSourcePlayerStart)
            {
                rejectReason = "invasion-snapshot-invalid-spawn-source";
                return false;
            }
        }

        return true;
    }
}
