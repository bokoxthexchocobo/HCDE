namespace HCDE.Net.Core;

public enum SnapshotTailMode : byte
{
    Coop = 0,
    Invasion = 1,
}

public readonly struct ServerSnapshotTailSections
{
    public ServerSnapshotTailSections(
        ServerWorldDeltaHeader worldDelta,
        ActorDeltasHeader actorDelta,
        CoopDeadSpawnsHeader? coopDeadSpawns,
        AuthorityEventsHeader? authorityEvents,
        InvasionSnapshotHeader? invasionSnapshot,
        PresentationEchoHeader presentationEcho,
        bool hasChecksum,
        uint checksumGameTic,
        uint[]? checksumHashes,
        PresentationEchoBlock? echoBlock,
        AuthorityEventRecord[]? authorityEventRecords = null)
    {
        WorldDelta = worldDelta;
        ActorDelta = actorDelta;
        CoopDeadSpawns = coopDeadSpawns;
        AuthorityEvents = authorityEvents;
        InvasionSnapshot = invasionSnapshot;
        PresentationEcho = presentationEcho;
        HasChecksum = hasChecksum;
        ChecksumGameTic = checksumGameTic;
        ChecksumHashes = checksumHashes;
        EchoBlock = echoBlock;
        AuthorityEventRecords = authorityEventRecords;
    }

    public ServerWorldDeltaHeader WorldDelta { get; }
    public ActorDeltasHeader ActorDelta { get; }
    public CoopDeadSpawnsHeader? CoopDeadSpawns { get; }
    public AuthorityEventsHeader? AuthorityEvents { get; }
    public InvasionSnapshotHeader? InvasionSnapshot { get; }
    public PresentationEchoHeader PresentationEcho { get; }
    public bool HasChecksum { get; }
    public uint ChecksumGameTic { get; }
    public uint[]? ChecksumHashes { get; }
    public PresentationEchoBlock? EchoBlock { get; }
    public AuthorityEventRecord[]? AuthorityEventRecords { get; }
}

public static class ServerSnapshotTailWalker
{
    public static bool TryWalk(
        ReadOnlySpan<byte> tail,
        out ServerSnapshotTailSections sections,
        out int bytesConsumed,
        out string? rejectReason)
    {
        sections = default;
        bytesConsumed = 0;
        rejectReason = null;
        var cursor = 0;

        if (!WorldDeltaChunkCodec.TryRead(tail[cursor..], out var worldDelta, out _, out _, out var worldDeltaBytes, out rejectReason))
            return false;

        cursor += worldDeltaBytes;

        CoopDeadSpawnsHeader? coopDeadSpawns = null;
        AuthorityEventsHeader? authorityEvents = null;
        AuthorityEventRecord[]? authorityEventRecords = null;
        InvasionSnapshotHeader? invasionSnapshot = null;
        ActorDeltasHeader actorDelta;

        if (cursor < tail.Length && InvasionSnapshotHeader.TryRead(tail[cursor..], out var invasionHeader))
        {
            if (!InvasionSnapshotCodec.TryReadBlock(tail[cursor..], out invasionHeader, out var invasionBytes, out rejectReason))
                return false;

            invasionSnapshot = invasionHeader;
            cursor += invasionBytes;
            actorDelta = default;
        }
        else
        {
            if (!ActorDeltasCodec.TryRead(tail[cursor..], out actorDelta, out _, out var actorDeltaBytes, out rejectReason))
                return false;

            cursor += actorDeltaBytes;

            if (cursor < tail.Length && CoopDeadSpawnsHeader.TryRead(tail[cursor..], out var deadSpawnsHeader))
            {
                if (!CoopDeadSpawnsCodec.TryRead(tail[cursor..], out deadSpawnsHeader, out _, out var deadBytes, out rejectReason))
                    return false;

                coopDeadSpawns = deadSpawnsHeader;
                cursor += deadBytes;
            }

            if (cursor < tail.Length && AuthorityEventsCodec.TryPeek(tail[cursor..]))
            {
                if (!AuthorityEventsCodec.TryRead(tail[cursor..], out var authorityHeader, out var authorityRecords, out var authorityBytes, out rejectReason))
                    return false;

                authorityEvents = authorityHeader;
                authorityEventRecords = authorityRecords;
                cursor += authorityBytes;
            }
        }

        PresentationEchoBlock echoBlock;
        if (cursor >= tail.Length || !tail[cursor..].StartsWith(LiveConstants.PresentationEchoMagic))
        {
            rejectReason = "missing-presentation-echo";
            return false;
        }

        if (!PresentationEchoCodec.TryRead(tail[cursor..], out echoBlock, out var echoBytes, out rejectReason))
            return false;

        var echoHeader = new PresentationEchoHeader(
            (byte)echoBlock.Players.Length,
            echoBlock.InventoryPlayerSlot ?? LiveConstants.PresentationEchoInvalidInventorySlot);
        cursor += echoBytes;

        var hasChecksum = false;
        uint checksumGameTic = 0;
        uint[]? checksumHashes = null;
        if (cursor < tail.Length && tail[cursor..].StartsWith(LiveConstants.SnapshotChecksumMagic))
        {
            if (!SnapshotChecksumCodec.TryRead(tail[cursor..], out checksumGameTic, out checksumHashes!, out var checksumBytes, out rejectReason))
                return false;

            hasChecksum = true;
            cursor += checksumBytes;
        }

        if (cursor != tail.Length)
        {
            rejectReason = "snapshot-tail-trailing-bytes";
            return false;
        }

        sections = new ServerSnapshotTailSections(
            worldDelta,
            actorDelta,
            coopDeadSpawns,
            authorityEvents,
            invasionSnapshot,
            echoHeader,
            hasChecksum,
            checksumGameTic,
            checksumHashes,
            echoBlock,
            authorityEventRecords);
        bytesConsumed = cursor;
        return true;
    }
}
