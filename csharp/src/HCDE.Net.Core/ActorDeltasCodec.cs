namespace HCDE.Net.Core;

public readonly struct ActorDeltasHeader
{
    public ActorDeltasHeader(byte flags, byte recordCount, byte protocolVersion = LiveConstants.ActorDeltasProtocolVersion)
    {
        Flags = flags;
        RecordCount = recordCount;
        ProtocolVersion = protocolVersion;
    }

    public byte Flags { get; }
    public byte RecordCount { get; }
    public byte ProtocolVersion { get; }

    public static bool TryRead(ReadOnlySpan<byte> chunk, out ActorDeltasHeader header)
    {
        header = default;
        if (chunk.Length < LiveConstants.ActorDeltasHeaderSize)
            return false;
        if (!chunk[..4].SequenceEqual(LiveConstants.ActorDeltasMagic))
            return false;

        header = new ActorDeltasHeader(chunk[5], chunk[6], chunk[4]);
        return true;
    }

    public static int Write(Span<byte> chunk, ActorDeltasHeader header)
    {
        if (chunk.Length < LiveConstants.ActorDeltasHeaderSize)
            return 0;

        LiveConstants.ActorDeltasMagic.CopyTo(chunk);
        chunk[4] = header.ProtocolVersion;
        chunk[5] = header.Flags;
        chunk[6] = header.RecordCount;
        chunk[7] = 0;
        return LiveConstants.ActorDeltasHeaderSize;
    }
}

public static class ActorDeltasCodec
{
    public static int WriteEmpty(Span<byte> chunk) =>
        ActorDeltasHeader.Write(chunk, new ActorDeltasHeader(LiveConstants.ActorDeltasFlagComplete, recordCount: 0));

    public static int Write(Span<byte> chunk, ReadOnlySpan<ActorDeltaRecord> records, bool markComplete = true)
    {
        if (chunk.Length < LiveConstants.ActorDeltasHeaderSize)
            return 0;

        if (records.Length == 0)
            return WriteEmpty(chunk);

        if (ActorDeltasHeader.Write(chunk, new ActorDeltasHeader(flags: 0, recordCount: 0)) == 0)
            return 0;

        var cursor = LiveConstants.ActorDeltasHeaderSize;
        byte count = 0;
        foreach (var record in records)
        {
            if (count == byte.MaxValue)
                return 0;

            if (ActorDeltaRecordCodec.Write(chunk, ref cursor, record) == 0)
                return 0;

            count++;
        }

        chunk[5] = markComplete ? LiveConstants.ActorDeltasFlagComplete : (byte)0;
        chunk[6] = count;
        return cursor;
    }

    public static bool TryRead(
        ReadOnlySpan<byte> chunk,
        out ActorDeltasHeader header,
        out IReadOnlyList<ActorDeltaRecord> records,
        out int bytesConsumed,
        out string? rejectReason)
    {
        header = default;
        records = Array.Empty<ActorDeltaRecord>();
        bytesConsumed = 0;
        rejectReason = null;

        if (!ActorDeltasHeader.TryRead(chunk, out header))
        {
            rejectReason = "missing-actor-delta-header";
            return false;
        }

        var cursor = LiveConstants.ActorDeltasHeaderSize;
        var parsed = new List<ActorDeltaRecord>(header.RecordCount);
        for (var i = 0; i < header.RecordCount; i++)
        {
            if (!ActorDeltaRecordCodec.TryRead(chunk, ref cursor, out var record))
            {
                rejectReason = "actor-delta-record-truncated";
                return false;
            }

            parsed.Add(record);
        }

        records = parsed;
        bytesConsumed = cursor;
        return true;
    }
}
