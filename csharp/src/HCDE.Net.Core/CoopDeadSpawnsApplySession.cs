namespace HCDE.Net.Core;

public interface ICoopDeadSpawnsApplySink
{
    bool TryRetireSpawnIndex(uint spawnIndex);
}

public readonly struct CoopDeadSpawnsApplyResult
{
    public CoopDeadSpawnsApplyResult(int recordCount, int applied, int missing)
    {
        RecordCount = recordCount;
        Applied = applied;
        Missing = missing;
    }

    public int RecordCount { get; }
    public int Applied { get; }
    public int Missing { get; }
}

public static class CoopDeadSpawnsApplySession
{
    public static bool TryApply(
        CoopDeadSpawnsHeader header,
        ReadOnlySpan<uint> spawnIndices,
        ICoopDeadSpawnsApplySink? sink,
        out CoopDeadSpawnsApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;

        if (header.RecordCount != spawnIndices.Length)
        {
            rejectReason = "coop-dead-spawns-count-mismatch";
            return false;
        }

        if (header.ProtocolVersion != LiveConstants.CoopDeadSpawnsProtocolVersion)
        {
            rejectReason = "coop-dead-spawns-version-mismatch";
            return false;
        }

        var applied = 0;
        var missing = 0;
        foreach (var index in spawnIndices)
        {
            if (sink != null && sink.TryRetireSpawnIndex(index))
                applied++;
            else
                missing++;
        }

        result = new CoopDeadSpawnsApplyResult(spawnIndices.Length, applied, missing);
        return true;
    }
}
