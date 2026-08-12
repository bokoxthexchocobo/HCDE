using System.Buffers.Binary;

namespace HCDE.Net.Core;

public readonly struct CoopDeadSpawnsHeader
{
    public CoopDeadSpawnsHeader(byte flags, byte recordCount, byte protocolVersion = LiveConstants.CoopDeadSpawnsProtocolVersion)
    {
        Flags = flags;
        RecordCount = recordCount;
        ProtocolVersion = protocolVersion;
    }

    public byte Flags { get; }
    public byte RecordCount { get; }
    public byte ProtocolVersion { get; }

    public static bool TryRead(ReadOnlySpan<byte> chunk, out CoopDeadSpawnsHeader header)
    {
        header = default;
        if (chunk.Length < LiveConstants.CoopDeadSpawnsHeaderSize)
            return false;
        if (!chunk[..4].SequenceEqual(LiveConstants.CoopDeadSpawnsMagic))
            return false;

        header = new CoopDeadSpawnsHeader(chunk[5], chunk[6], chunk[4]);
        return true;
    }

    public static int Write(Span<byte> chunk, CoopDeadSpawnsHeader header)
    {
        if (chunk.Length < LiveConstants.CoopDeadSpawnsHeaderSize)
            return 0;

        LiveConstants.CoopDeadSpawnsMagic.CopyTo(chunk);
        chunk[4] = header.ProtocolVersion;
        chunk[5] = header.Flags;
        chunk[6] = header.RecordCount;
        chunk[7] = 0;
        return LiveConstants.CoopDeadSpawnsHeaderSize;
    }
}

public static class CoopDeadSpawnsCodec
{
    public static int Write(Span<byte> chunk, ReadOnlySpan<uint> spawnIndices)
    {
        if (spawnIndices.Length == 0)
            return 0;

        var required = LiveConstants.CoopDeadSpawnsHeaderSize + spawnIndices.Length * 4;
        if (chunk.Length < required || spawnIndices.Length > byte.MaxValue)
            return 0;

        if (CoopDeadSpawnsHeader.Write(chunk, new CoopDeadSpawnsHeader(flags: 0, (byte)spawnIndices.Length)) == 0)
            return 0;

        var cursor = LiveConstants.CoopDeadSpawnsHeaderSize;
        foreach (var index in spawnIndices)
        {
            BinaryPrimitives.WriteUInt32BigEndian(chunk[cursor..], index);
            cursor += 4;
        }

        return cursor;
    }

    public static bool TryRead(
        ReadOnlySpan<byte> chunk,
        out CoopDeadSpawnsHeader header,
        out uint[] spawnIndices,
        out int bytesConsumed,
        out string? rejectReason)
    {
        header = default;
        spawnIndices = Array.Empty<uint>();
        bytesConsumed = 0;
        rejectReason = null;

        if (!CoopDeadSpawnsHeader.TryRead(chunk, out header))
        {
            rejectReason = "missing-coop-dead-spawns-header";
            return false;
        }

        if (header.ProtocolVersion != LiveConstants.CoopDeadSpawnsProtocolVersion)
        {
            rejectReason = "coop-dead-spawns-version-mismatch";
            return false;
        }

        var cursor = LiveConstants.CoopDeadSpawnsHeaderSize;
        if (chunk.Length - cursor < header.RecordCount * 4)
        {
            rejectReason = "coop-dead-spawns-truncated";
            return false;
        }

        spawnIndices = new uint[header.RecordCount];
        for (var i = 0; i < header.RecordCount; i++)
        {
            spawnIndices[i] = BinaryPrimitives.ReadUInt32BigEndian(chunk[cursor..]);
            cursor += 4;
        }

        bytesConsumed = cursor;
        return true;
    }
}
