namespace HCDE.Net.Core;

public interface IAuthorityEventSink
{
    bool TryApplyInvasionSpawn(AuthorityEventRecord record);
    bool TryApplyPickupSpawn(AuthorityEventRecord record);
    bool TryApplyInvasionDespawn(AuthorityEventRecord record);
    bool TryApplyPickupRetire(AuthorityEventRecord record);
    bool TryApplyInvasionDamage(AuthorityEventRecord record);
    bool TryApplyCoopProjectileSpawn(AuthorityEventRecord record);
    bool TryApplyCoopProjectileRetire(AuthorityEventRecord record);
    bool TryApplyCoopCosmeticSpawn(AuthorityEventRecord record);
}

public readonly struct AuthorityEventsApplyResult
{
    public AuthorityEventsApplyResult(int recordCount, int applied, int missing)
    {
        RecordCount = recordCount;
        Applied = applied;
        Missing = missing;
    }

    public int RecordCount { get; }
    public int Applied { get; }
    public int Missing { get; }
}

public static class AuthorityEventsApplySession
{
    public static bool TryApply(
        ReadOnlySpan<byte> chunk,
        IAuthorityEventSink sink,
        out AuthorityEventsApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;
        if (sink == null)
        {
            rejectReason = "authority-event-sink-missing";
            return false;
        }

        if (!AuthorityEventsCodec.TryRead(chunk, out _, out var records, out _, out rejectReason))
            return false;

        var applied = 0;
        var missing = 0;
        foreach (var record in records)
        {
            if (!record.IsValid(out rejectReason))
                return false;

            if (TryRoute(record, sink))
                applied++;
            else
                missing++;
        }

        result = new AuthorityEventsApplyResult(records.Length, applied, missing);
        return true;
    }

    public static bool TryApply(
        ReadOnlySpan<AuthorityEventRecord> records,
        IAuthorityEventSink sink,
        out AuthorityEventsApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;
        if (sink == null)
        {
            rejectReason = "authority-event-sink-missing";
            return false;
        }

        var applied = 0;
        var missing = 0;
        foreach (var record in records)
        {
            if (!record.IsValid(out rejectReason))
                return false;

            if (TryRoute(record, sink))
                applied++;
            else
                missing++;
        }

        result = new AuthorityEventsApplyResult(records.Length, applied, missing);
        return true;
    }

    private static bool TryRoute(AuthorityEventRecord record, IAuthorityEventSink sink)
    {
        return record.EventType switch
        {
            AuthorityEventType.Spawn when record.Source == ReplicatedActorSource.Invasion
                => sink.TryApplyInvasionSpawn(record),
            AuthorityEventType.Spawn when record.Category == ReplicatedActorCategory.Pickup
                => sink.TryApplyPickupSpawn(record),
            AuthorityEventType.Despawn when record.Source == ReplicatedActorSource.Invasion
                => sink.TryApplyInvasionDespawn(record),
            AuthorityEventType.Despawn when record.Category == ReplicatedActorCategory.Pickup
                => sink.TryApplyPickupRetire(record),
            AuthorityEventType.Damage when record.Source == ReplicatedActorSource.Invasion
                => sink.TryApplyInvasionDamage(record),
            AuthorityEventType.Spawn
                when record.Source == ReplicatedActorSource.Coop && record.Category == ReplicatedActorCategory.Projectile
                => sink.TryApplyCoopProjectileSpawn(record),
            AuthorityEventType.Despawn
                when record.Source == ReplicatedActorSource.Coop && record.Category == ReplicatedActorCategory.Projectile
                => sink.TryApplyCoopProjectileRetire(record),
            AuthorityEventType.CosmeticSpawn
                when record.Source == ReplicatedActorSource.Coop && record.Category == ReplicatedActorCategory.Visual
                => sink.TryApplyCoopCosmeticSpawn(record),
            _ => false,
        };
    }
}
