using System.Buffers.Binary;

namespace HCDE.Net.Core;

public readonly struct InvasionSnapshotHeader
{
    public InvasionSnapshotHeader(
        byte flags,
        byte state,
        uint stateTics,
        uint wave,
        uint maxWaves,
        uint waveBudget,
        uint waveSpawned,
        uint waveCleared,
        uint activeMonsters,
        ushort spawnSpotCount = 0,
        ushort activeSpawnSpotCount = 0,
        uint spawnPlanBudget = 0,
        uint spawnActiveTag = 0,
        byte spawnFlags = 0,
        byte spawnFallbackSource = 0,
        byte protocolVersion = LiveConstants.InvasionSnapshotProtocolVersion)
    {
        Flags = flags;
        State = state;
        StateTics = stateTics;
        Wave = wave;
        MaxWaves = maxWaves;
        WaveBudget = waveBudget;
        WaveSpawned = waveSpawned;
        WaveCleared = waveCleared;
        ActiveMonsters = activeMonsters;
        SpawnSpotCount = spawnSpotCount;
        ActiveSpawnSpotCount = activeSpawnSpotCount;
        SpawnPlanBudget = spawnPlanBudget;
        SpawnActiveTag = spawnActiveTag;
        SpawnFlags = spawnFlags;
        SpawnFallbackSource = spawnFallbackSource;
        ProtocolVersion = protocolVersion;
    }

    public byte Flags { get; }
    public byte State { get; }
    public uint StateTics { get; }
    public uint Wave { get; }
    public uint MaxWaves { get; }
    public uint WaveBudget { get; }
    public uint WaveSpawned { get; }
    public uint WaveCleared { get; }
    public uint ActiveMonsters { get; }
    public ushort SpawnSpotCount { get; }
    public ushort ActiveSpawnSpotCount { get; }
    public uint SpawnPlanBudget { get; }
    public uint SpawnActiveTag { get; }
    public byte SpawnFlags { get; }
    public byte SpawnFallbackSource { get; }
    public byte ProtocolVersion { get; }

    public int HeaderSize => ProtocolVersion >= 2
        ? LiveConstants.InvasionSnapshotHeaderV2Size
        : LiveConstants.InvasionSnapshotHeaderV1Size;

    public static bool TryRead(ReadOnlySpan<byte> chunk, out InvasionSnapshotHeader header)
    {
        header = default;
        if (chunk.Length < LiveConstants.InvasionSnapshotHeaderV1Size)
            return false;
        if (!chunk[..4].SequenceEqual(LiveConstants.InvasionSnapshotMagic))
            return false;

        var version = chunk[4];
        if (version != 1 && version != LiveConstants.InvasionSnapshotProtocolVersion)
            return false;

        var headerSize = version >= 2 ? LiveConstants.InvasionSnapshotHeaderV2Size : LiveConstants.InvasionSnapshotHeaderV1Size;
        if (chunk.Length < headerSize)
            return false;

        var cursor = 8;
        var stateTics = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;
        var wave = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;
        var maxWaves = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;
        var waveBudget = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;
        var waveSpawned = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;
        var waveCleared = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;
        var activeMonsters = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
        cursor += 4;

        ushort spawnSpotCount = 0;
        ushort activeSpawnSpotCount = 0;
        uint spawnPlanBudget = 0;
        uint spawnActiveTag = 0;
        byte spawnFlags = 0;
        byte spawnFallbackSource = 0;
        if (version >= 2)
        {
            spawnSpotCount = BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]);
            cursor += 2;
            activeSpawnSpotCount = BinaryPrimitives.ReadUInt16BigEndian(chunk[cursor..]);
            cursor += 2;
            spawnPlanBudget = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
            cursor += 4;
            spawnActiveTag = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
            cursor += 4;
            spawnFlags = chunk[cursor++];
            spawnFallbackSource = chunk[cursor++];
            cursor += 2;
        }

        header = new InvasionSnapshotHeader(
            chunk[5],
            chunk[6],
            stateTics,
            wave,
            maxWaves,
            waveBudget,
            waveSpawned,
            waveCleared,
            activeMonsters,
            spawnSpotCount,
            activeSpawnSpotCount,
            spawnPlanBudget,
            spawnActiveTag,
            spawnFlags,
            spawnFallbackSource,
            version);
        return true;
    }

    public static int Write(Span<byte> chunk, InvasionSnapshotHeader header)
    {
        var headerSize = header.HeaderSize;
        if (chunk.Length < headerSize)
            return 0;

        LiveConstants.InvasionSnapshotMagic.CopyTo(chunk);
        chunk[4] = header.ProtocolVersion;
        chunk[5] = header.Flags;
        chunk[6] = header.State;
        chunk[7] = 0;
        BinaryPrimitives.WriteUInt32BigEndian(chunk[8..], header.StateTics);
        BinaryPrimitives.WriteUInt32BigEndian(chunk[12..], header.Wave);
        BinaryPrimitives.WriteUInt32BigEndian(chunk[16..], header.MaxWaves);
        BinaryPrimitives.WriteUInt32BigEndian(chunk[20..], header.WaveBudget);
        BinaryPrimitives.WriteUInt32BigEndian(chunk[24..], header.WaveSpawned);
        BinaryPrimitives.WriteUInt32BigEndian(chunk[28..], header.WaveCleared);
        BinaryPrimitives.WriteUInt32BigEndian(chunk[32..], header.ActiveMonsters);

        if (header.ProtocolVersion >= 2)
        {
            BinaryPrimitives.WriteUInt16BigEndian(chunk[36..], header.SpawnSpotCount);
            BinaryPrimitives.WriteUInt16BigEndian(chunk[38..], header.ActiveSpawnSpotCount);
            BinaryPrimitives.WriteUInt32BigEndian(chunk[40..], header.SpawnPlanBudget);
            BinaryPrimitives.WriteUInt32BigEndian(chunk[44..], header.SpawnActiveTag);
            chunk[48] = header.SpawnFlags;
            chunk[49] = header.SpawnFallbackSource;
            chunk[50] = 0;
            chunk[51] = 0;
        }

        return headerSize;
    }
}

public static class InvasionSnapshotCodec
{
    public static int WriteEmptyV2(Span<byte> chunk, InvasionSnapshotHeader header)
    {
        var written = InvasionSnapshotHeader.Write(chunk, header);
        return written == 0 ? 0 : written;
    }

    public static bool TryReadBlock(
        ReadOnlySpan<byte> chunk,
        out InvasionSnapshotHeader header,
        out int bytesConsumed,
        out string? rejectReason)
    {
        header = default;
        bytesConsumed = 0;
        rejectReason = null;

        if (!InvasionSnapshotHeader.TryRead(chunk, out header))
        {
            rejectReason = "missing-invasion-snapshot-header";
            return false;
        }

        var cursor = header.HeaderSize;
        var payloadEnd = Math.Min(chunk.Length, cursor + LiveConstants.InvasionSnapshotPayloadBudgetBytes);

        if (cursor < payloadEnd && AuthorityEventsCodec.TryPeek(chunk[cursor..]))
        {
            if (!AuthorityEventsCodec.TryReadAndSkip(chunk, ref cursor, out rejectReason))
                return false;
        }

        if (cursor < payloadEnd && chunk[cursor..].StartsWith(LiveConstants.ActorDeltasMagic))
        {
            if (!ActorDeltasCodec.TryRead(chunk[cursor..], out _, out _, out var actorBytes, out rejectReason))
                return false;

            cursor += actorBytes;
        }

        bytesConsumed = cursor;
        return true;
    }
}
