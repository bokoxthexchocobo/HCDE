using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public readonly struct ServerSnapshotHeader
{
    public ServerSnapshotHeader(
        byte controlFlags,
        byte routingByte,
        byte playerCount,
        uint sequenceAck,
        uint consistencyAck,
        ushort quitterBytes,
        uint baseSequence,
        uint baseConsistency,
        byte commandTics,
        byte consistencyTics,
        byte stabilityBuffer,
        ushort bodyBytes,
        byte protocolVersion = LiveConstants.ServerSnapshotProtocolVersion)
    {
        ControlFlags = controlFlags;
        RoutingByte = routingByte;
        PlayerCount = playerCount;
        SequenceAck = sequenceAck;
        ConsistencyAck = consistencyAck;
        QuitterBytes = quitterBytes;
        BaseSequence = baseSequence;
        BaseConsistency = baseConsistency;
        CommandTics = commandTics;
        ConsistencyTics = consistencyTics;
        StabilityBuffer = stabilityBuffer;
        BodyBytes = bodyBytes;
        ProtocolVersion = protocolVersion;
    }

    public byte ControlFlags { get; }
    public byte RoutingByte { get; }
    public byte PlayerCount { get; }
    public uint SequenceAck { get; }
    public uint ConsistencyAck { get; }
    public ushort QuitterBytes { get; }
    public uint BaseSequence { get; }
    public uint BaseConsistency { get; }
    public byte CommandTics { get; }
    public byte ConsistencyTics { get; }
    public byte StabilityBuffer { get; }
    public ushort BodyBytes { get; }
    public byte ProtocolVersion { get; }

    public static bool LooksLikeHeader(ReadOnlySpan<byte> payload) =>
        payload.Length >= LiveConstants.ServerSnapshotHeaderSize
        && payload[..4].SequenceEqual(LiveConstants.ServerSnapshotMagic);

    public static bool TryRead(ReadOnlySpan<byte> payload, out ServerSnapshotHeader header)
    {
        header = default;
        if (!LooksLikeHeader(payload))
            return false;

        header = new ServerSnapshotHeader(
            payload[5],
            payload[6],
            payload[7],
            BinaryPrimitives.ReadUInt32BigEndian(payload[8..]),
            BinaryPrimitives.ReadUInt32BigEndian(payload[12..]),
            BinaryPrimitives.ReadUInt16BigEndian(payload[16..]),
            BinaryPrimitives.ReadUInt32BigEndian(payload[18..]),
            BinaryPrimitives.ReadUInt32BigEndian(payload[22..]),
            payload[26],
            payload[27],
            payload[28],
            BinaryPrimitives.ReadUInt16BigEndian(payload[29..]),
            payload[4]);
        return true;
    }

    public static int Write(Span<byte> payload, ServerSnapshotHeader header)
    {
        if (payload.Length < LiveConstants.ServerSnapshotHeaderSize)
            return 0;

        LiveConstants.ServerSnapshotMagic.CopyTo(payload);
        payload[4] = header.ProtocolVersion;
        payload[5] = header.ControlFlags;
        payload[6] = header.RoutingByte;
        payload[7] = header.PlayerCount;
        BinaryPrimitives.WriteUInt32BigEndian(payload[8..], header.SequenceAck);
        BinaryPrimitives.WriteUInt32BigEndian(payload[12..], header.ConsistencyAck);
        BinaryPrimitives.WriteUInt16BigEndian(payload[16..], header.QuitterBytes);
        BinaryPrimitives.WriteUInt32BigEndian(payload[18..], header.BaseSequence);
        BinaryPrimitives.WriteUInt32BigEndian(payload[22..], header.BaseConsistency);
        payload[26] = header.CommandTics;
        payload[27] = header.ConsistencyTics;
        payload[28] = header.StabilityBuffer;
        BinaryPrimitives.WriteUInt16BigEndian(payload[29..], header.BodyBytes);
        return LiveConstants.ServerSnapshotHeaderSize;
    }

    public static bool ValidateHeader(
        ServerSnapshotHeader header,
        int payloadSize,
        out string? rejectReason)
    {
        rejectReason = null;
        if (header.ProtocolVersion != LiveConstants.ServerSnapshotProtocolVersion)
        {
            rejectReason = "server-snapshot-version-mismatch";
            return false;
        }

        if (header.PlayerCount > NetConstants.MaxPlayers)
        {
            rejectReason = "server-snapshot-player-count-overflow";
            return false;
        }

        if (header.CommandTics > NetConstants.MaxSendTics)
        {
            rejectReason = "server-snapshot-command-tics-overflow";
            return false;
        }

        if (header.ConsistencyTics > NetConstants.MaxSendTics)
        {
            rejectReason = "server-snapshot-consistency-tics-overflow";
            return false;
        }

        if (LiveConstants.ServerSnapshotHeaderSize + header.QuitterBytes + header.BodyBytes != payloadSize)
        {
            rejectReason = "server-snapshot-body-length-mismatch";
            return false;
        }

        return true;
    }
}

public readonly struct ServerSnapshotRecordsHeader
{
    public ServerSnapshotRecordsHeader(byte playerCount, byte protocolVersion = LiveConstants.ServerSnapshotRecordsProtocolVersion)
    {
        PlayerCount = playerCount;
        ProtocolVersion = protocolVersion;
    }

    public byte PlayerCount { get; }
    public byte ProtocolVersion { get; }

    public static bool TryRead(ReadOnlySpan<byte> body, out ServerSnapshotRecordsHeader header)
    {
        header = default;
        if (body.Length < LiveConstants.ServerSnapshotRecordsHeaderSize)
            return false;
        if (!body[..4].SequenceEqual(LiveConstants.ServerSnapshotRecordsMagic))
            return false;

        header = new ServerSnapshotRecordsHeader(body[5], body[4]);
        return true;
    }

    public static int Write(Span<byte> body, ServerSnapshotRecordsHeader header)
    {
        if (body.Length < LiveConstants.ServerSnapshotRecordsHeaderSize)
            return 0;

        LiveConstants.ServerSnapshotRecordsMagic.CopyTo(body);
        body[4] = header.ProtocolVersion;
        body[5] = header.PlayerCount;
        return LiveConstants.ServerSnapshotRecordsHeaderSize;
    }

    public static bool Validate(ServerSnapshotRecordsHeader records, byte headerPlayerCount, out string? rejectReason)
    {
        rejectReason = null;
        if (records.ProtocolVersion != LiveConstants.ServerSnapshotRecordsProtocolVersion)
        {
            rejectReason = "server-snapshot-record-version-mismatch";
            return false;
        }

        if (records.PlayerCount != headerPlayerCount)
        {
            rejectReason = "server-snapshot-record-player-count-mismatch";
            return false;
        }

        return true;
    }
}
