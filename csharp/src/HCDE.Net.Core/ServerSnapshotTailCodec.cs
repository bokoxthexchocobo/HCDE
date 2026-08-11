namespace HCDE.Net.Core;

public static class ServerSnapshotTailCodec
{
    public const int MinimalTailSize = LiveConstants.ServerWorldDeltaHeaderSize + 1 + LiveConstants.ActorDeltasHeaderSize;

    public static int WriteMinimal(Span<byte> tail, uint gameTic = 0)
    {
        if (tail.Length < MinimalTailSize)
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
        return cursor;
    }

    public static int Write(
        Span<byte> tail,
        uint gameTic,
        ReadOnlySpan<PlayerPoseWorldDelta> poses,
        ReadOnlySpan<SectorWorldDelta> sectors)
    {
        var required = WorldDeltaChunkCodec.MinChunkSize((byte)poses.Length, (byte)sectors.Length) + LiveConstants.ActorDeltasHeaderSize;
        if (tail.Length < required)
            return 0;

        var cursor = 0;
        var worldDeltaWritten = WorldDeltaChunkCodec.Write(tail[cursor..], flags: 0, gameTic, poses, sectors);
        if (worldDeltaWritten == 0)
            return 0;

        cursor += worldDeltaWritten;
        var actorDeltaWritten = ActorDeltasCodec.WriteEmpty(tail[cursor..]);
        if (actorDeltaWritten == 0)
            return 0;

        cursor += actorDeltaWritten;
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
        if (!ActorDeltasHeader.TryRead(actorTail, out actorDeltaHeader))
        {
            rejectReason = "missing-actor-delta-header";
            return false;
        }

        if (actorDeltaHeader.RecordCount != 0)
        {
            rejectReason = "actor-delta-tail-not-empty";
            return false;
        }

        bytesConsumed = worldDeltaBytes + LiveConstants.ActorDeltasHeaderSize;
        return true;
    }
}
