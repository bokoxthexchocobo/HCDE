namespace HCDE.Net.Core;

public static class ServerSnapshotTailCodec
{
    public const int MinimalTailSize = LiveConstants.ServerWorldDeltaHeaderSize + 1 + LiveConstants.ActorDeltasHeaderSize;

    public static int MinimalTailWithChecksumSize => MinimalTailSize + LiveConstants.SnapshotChecksumBlockSize;

    public static int WriteMinimal(Span<byte> tail, uint gameTic = 0, uint[]? checksumHashes = null)
    {
        var required = checksumHashes is { Length: LiveConstants.SnapshotChecksumCategoryCount }
            ? MinimalTailWithChecksumSize
            : MinimalTailSize;
        if (tail.Length < required)
            return 0;

        var cursor = 0;
        var worldDeltaWritten = WorldDeltaChunkCodec.WriteEmpty(tail[cursor..], gameTic);
        if (worldDeltaWritten == 0)
            return 0;

        cursor += worldDeltaWritten;
        var actorDeltaWritten = ActorDeltasCodec.WriteEmpty(tail[cursor..]);
        if (actorDeltaWritten == 0)
            return 0;

        cursor += actorDeltaWritten;
        if (checksumHashes is { Length: LiveConstants.SnapshotChecksumCategoryCount })
        {
            var checksumWritten = SnapshotChecksumCodec.Write(tail[cursor..], gameTic, checksumHashes);
            if (checksumWritten == 0)
                return 0;

            cursor += checksumWritten;
        }

        return cursor;
    }

    public static int Write(
        Span<byte> tail,
        uint gameTic,
        ReadOnlySpan<PlayerPoseWorldDelta> poses,
        ReadOnlySpan<SectorWorldDelta> sectors,
        ReadOnlySpan<ActorDeltaRecord> actorDeltas,
        uint[]? checksumHashes = null)
    {
        var actorDeltaSize = LiveConstants.ActorDeltasHeaderSize;
        foreach (var record in actorDeltas)
            actorDeltaSize += ActorDeltaRecord.MinRecordSize(record.FieldMask);
        var required = WorldDeltaChunkCodec.MinChunkSize((byte)poses.Length, (byte)sectors.Length)
            + actorDeltaSize
            + (checksumHashes is { Length: LiveConstants.SnapshotChecksumCategoryCount }
                ? LiveConstants.SnapshotChecksumBlockSize
                : 0);
        if (tail.Length < required)
            return 0;

        var cursor = 0;
        var worldDeltaWritten = WorldDeltaChunkCodec.Write(tail[cursor..], flags: 0, gameTic, poses, sectors);
        if (worldDeltaWritten == 0)
            return 0;

        cursor += worldDeltaWritten;
        var actorDeltaWritten = actorDeltas.Length == 0
            ? ActorDeltasCodec.WriteEmpty(tail[cursor..])
            : ActorDeltasCodec.Write(tail[cursor..], actorDeltas);
        if (actorDeltaWritten == 0)
            return 0;

        cursor += actorDeltaWritten;
        if (checksumHashes is { Length: LiveConstants.SnapshotChecksumCategoryCount })
        {
            var checksumWritten = SnapshotChecksumCodec.Write(tail[cursor..], gameTic, checksumHashes);
            if (checksumWritten == 0)
                return 0;

            cursor += checksumWritten;
        }

        return cursor;
    }

    public static bool TryReadMinimal(
        ReadOnlySpan<byte> tail,
        out ServerWorldDeltaHeader worldDeltaHeader,
        out ActorDeltasHeader actorDeltaHeader,
        out int bytesConsumed,
        out string? rejectReason)
    {
        worldDeltaHeader = default;
        actorDeltaHeader = default;
        bytesConsumed = 0;
        rejectReason = null;

        if (!WorldDeltaChunkCodec.TryRead(tail, out worldDeltaHeader, out _, out _, out var worldDeltaBytes, out rejectReason))
            return false;

        var actorTail = tail[worldDeltaBytes..];
        if (!ActorDeltasCodec.TryRead(actorTail, out actorDeltaHeader, out var records, out var actorDeltaBytes, out rejectReason))
            return false;

        if (records.Count != 0)
        {
            rejectReason = "actor-delta-tail-not-empty";
            return false;
        }

        bytesConsumed = worldDeltaBytes + actorDeltaBytes;
        return true;
    }

    public static bool TryReadChecksum(ReadOnlySpan<byte> tail, out uint gameTic, out uint[] categoryHashes, out int bytesConsumed)
    {
        gameTic = 0;
        categoryHashes = Array.Empty<uint>();
        bytesConsumed = 0;
        if (tail.Length < LiveConstants.SnapshotChecksumBlockSize
            || !tail[..4].SequenceEqual(LiveConstants.SnapshotChecksumMagic))
            return false;

        return SnapshotChecksumCodec.TryRead(tail, out gameTic, out categoryHashes, out bytesConsumed, out _);
    }
}
