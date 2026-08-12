namespace HCDE.Net.Core;

public static class ServerSnapshotTailCodec
{
    public const int MinimalTailSize =
        LiveConstants.ServerWorldDeltaHeaderSize
        + 1
        + LiveConstants.ActorDeltasHeaderSize
        + LiveConstants.PresentationEchoMinHeaderSize;

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
        var echoWritten = PresentationEchoCodec.WriteMinimal(tail[cursor..]);
        if (echoWritten == 0)
            return 0;

        cursor += echoWritten;
        if (checksumHashes is { Length: LiveConstants.SnapshotChecksumCategoryCount })
        {
            var checksumWritten = SnapshotChecksumCodec.Write(tail[cursor..], gameTic, checksumHashes);
            if (checksumWritten == 0)
                return 0;

            cursor += checksumWritten;
        }

        return cursor;
    }

    public static int WriteCoopShipping(
        Span<byte> tail,
        uint gameTic,
        ReadOnlySpan<PlayerPoseWorldDelta> poses,
        ReadOnlySpan<SectorWorldDelta> sectors,
        ReadOnlySpan<ActorDeltaRecord> actorDeltas,
        ReadOnlySpan<uint> coopDeadSpawnIndices,
        ReadOnlySpan<AuthorityEventRecord> authorityEvents = default,
        uint[]? checksumHashes = null)
    {
        var actorDeltaSize = LiveConstants.ActorDeltasHeaderSize;
        foreach (var record in actorDeltas)
            actorDeltaSize += ActorDeltaRecord.MinRecordSize(record.FieldMask);

        var deadSpawnSize = coopDeadSpawnIndices.Length == 0
            ? 0
            : LiveConstants.CoopDeadSpawnsHeaderSize + coopDeadSpawnIndices.Length * 4;

        var authorityEventSize = 0;
        if (!authorityEvents.IsEmpty)
        {
            authorityEventSize = LiveConstants.AuthorityEventsHeaderSize;
            foreach (var record in authorityEvents)
                authorityEventSize += AuthorityEventRecord.MinRecordSize(record.ClassName);
        }

        var required = WorldDeltaChunkCodec.MinChunkSize((byte)poses.Length, (byte)sectors.Length)
            + actorDeltaSize
            + deadSpawnSize
            + authorityEventSize
            + LiveConstants.PresentationEchoMinHeaderSize
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
        if (coopDeadSpawnIndices.Length > 0)
        {
            var deadWritten = CoopDeadSpawnsCodec.Write(tail[cursor..], coopDeadSpawnIndices);
            if (deadWritten == 0)
                return 0;

            cursor += deadWritten;
        }

        if (!authorityEvents.IsEmpty)
        {
            var authorityWritten = AuthorityEventsCodec.Write(tail[cursor..], authorityEvents);
            if (authorityWritten == 0)
                return 0;

            cursor += authorityWritten;
        }

        var echoWritten = PresentationEchoCodec.WriteMinimal(tail[cursor..]);
        if (echoWritten == 0)
            return 0;

        cursor += echoWritten;
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
        => WriteCoopShipping(tail, gameTic, poses, sectors, actorDeltas, ReadOnlySpan<uint>.Empty, default, checksumHashes);

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

        if (!ServerSnapshotTailWalker.TryWalk(tail, out var sections, out bytesConsumed, out rejectReason))
            return false;

        worldDeltaHeader = sections.WorldDelta;
        actorDeltaHeader = sections.ActorDelta;
        return sections.CoopDeadSpawns is null
            && sections.AuthorityEvents is null
            && sections.InvasionSnapshot is null
            && !sections.HasChecksum;
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
