namespace HCDE.Net.Core;

public interface IActorDeltaApplySink
{
    bool TryApply(int recipientClientSlot, ActorDeltaRecord record);
}

public readonly struct ActorDeltasApplyResult
{
    public ActorDeltasApplyResult(int recordCount, int applied, int missing)
    {
        RecordCount = recordCount;
        Applied = applied;
        Missing = missing;
    }

    public int RecordCount { get; }
    public int Applied { get; }
    public int Missing { get; }
}

public static class ActorDeltasApplySession
{
    public static bool TryApply(
        ActorDeltasHeader header,
        IReadOnlyList<ActorDeltaRecord> records,
        int recipientClientSlot,
        IActorDeltaApplySink? sink,
        out ActorDeltasApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;

        if (header.RecordCount != records.Count)
        {
            rejectReason = "actor-delta-count-mismatch";
            return false;
        }

        if (header.ProtocolVersion != LiveConstants.ActorDeltasProtocolVersion)
        {
            rejectReason = "actor-delta-version-mismatch";
            return false;
        }

        if ((header.Flags & ~LiveConstants.ActorDeltasFlagComplete) != 0)
        {
            rejectReason = "actor-delta-invalid-flags";
            return false;
        }

        var applied = 0;
        var missing = 0;
        foreach (var record in records)
        {
            if (record.ActorId == 0 || record.FieldMask == 0)
            {
                rejectReason = "actor-delta-invalid-record";
                return false;
            }

            if ((record.FieldMask & ~LiveConstants.ActorDeltaFieldAll) != 0)
            {
                rejectReason = "actor-delta-invalid-field-mask";
                return false;
            }

            if (record.Category > (byte)ReplicatedActorCategory.Visual)
            {
                rejectReason = "actor-delta-invalid-category";
                return false;
            }

            if ((record.Flags & ~LiveConstants.ActorDeltaFlagLive) != 0)
            {
                rejectReason = "actor-delta-invalid-actor-flags";
                return false;
            }

            if (sink != null && sink.TryApply(recipientClientSlot, record))
                applied++;
            else
                missing++;
        }

        result = new ActorDeltasApplyResult(records.Count, applied, missing);
        return true;
    }
}
