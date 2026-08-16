namespace HCDE.Net.Core;

public static class WorldDeltaChunkCodec
{
    public static int MinChunkSize(byte playerCount, byte sectorCount) =>
        LiveConstants.ServerWorldDeltaHeaderSize
        + playerCount * LiveConstants.ServerWorldDeltaPoseRecordV4Size
        + 1
        + sectorCount * LiveConstants.ServerWorldDeltaSectorRecordMaxSize;

    public static int Write(
        Span<byte> chunk,
        byte flags,
        uint gameTic,
        ReadOnlySpan<PlayerPoseWorldDelta> poses,
        ReadOnlySpan<SectorWorldDelta> sectors)
    {
        if (chunk.Length < MinChunkSize((byte)poses.Length, (byte)sectors.Length))
            return 0;

        var header = new ServerWorldDeltaHeader(flags, gameTic, (byte)poses.Length);
        if (ServerWorldDeltaHeader.Write(chunk, header) == 0)
            return 0;

        var cursor = LiveConstants.ServerWorldDeltaHeaderSize;
        foreach (var pose in poses)
        {
            if (WorldDeltaPoseCodec.Write(chunk, ref cursor, pose) == 0)
                return 0;
        }

        if (cursor >= chunk.Length)
            return 0;

        chunk[cursor++] = (byte)sectors.Length;
        foreach (var sector in sectors)
        {
            if (WorldDeltaPoseCodec.WriteSector(chunk, ref cursor, sector) == 0)
                return 0;
        }

        return cursor;
    }

    public static int WriteEmpty(Span<byte> chunk, uint gameTic = 0) =>
        Write(chunk, flags: 0, gameTic, ReadOnlySpan<PlayerPoseWorldDelta>.Empty, ReadOnlySpan<SectorWorldDelta>.Empty);

    public static bool TryRead(
        ReadOnlySpan<byte> chunk,
        out ServerWorldDeltaHeader header,
        out IReadOnlyList<PlayerPoseWorldDelta> poses,
        out IReadOnlyList<SectorWorldDelta> sectors,
        out int bytesConsumed,
        out string? rejectReason)
    {
        header = default;
        poses = Array.Empty<PlayerPoseWorldDelta>();
        sectors = Array.Empty<SectorWorldDelta>();
        bytesConsumed = 0;
        rejectReason = null;

        if (!ServerWorldDeltaHeader.TryRead(chunk, out header))
        {
            rejectReason = "missing-world-delta-header";
            return false;
        }

        var cursor = LiveConstants.ServerWorldDeltaHeaderSize;
        var parsedPoses = new List<PlayerPoseWorldDelta>(header.RecordCount);
        for (var i = 0; i < header.RecordCount; i++)
        {
            if (!WorldDeltaPoseCodec.TryRead(chunk, ref cursor, out var pose))
            {
                rejectReason = "world-delta-pose-truncated";
                return false;
            }

            parsedPoses.Add(pose);
        }

        if (chunk.Length - cursor < 1)
        {
            rejectReason = "world-delta-sector-count-truncated";
            return false;
        }

        var sectorCount = chunk[cursor++];
        var parsedSectors = new List<SectorWorldDelta>(sectorCount);
        for (var i = 0; i < sectorCount; i++)
        {
            if (!WorldDeltaPoseCodec.TryReadSector(chunk, ref cursor, out var sector))
            {
                rejectReason = "world-delta-sector-truncated";
                return false;
            }

            parsedSectors.Add(sector);
        }

        poses = parsedPoses;
        sectors = parsedSectors;
        bytesConsumed = cursor;
        return true;
    }
}
