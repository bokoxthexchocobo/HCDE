using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public readonly struct ClientInputHeader
{
    public ClientInputHeader(
        byte controlFlags,
        byte routingByte,
        byte playerCount,
        uint sequenceAck,
        uint consistencyAck,
        uint baseSequence,
        uint baseConsistency,
        byte commandTics,
        byte consistencyTics,
        byte stabilityBuffer,
        ushort bodyBytes,
        byte protocolVersion = LiveConstants.ClientInputProtocolVersion)
    {
        ControlFlags = controlFlags;
        RoutingByte = routingByte;
        PlayerCount = playerCount;
        SequenceAck = sequenceAck;
        ConsistencyAck = consistencyAck;
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
    public uint BaseSequence { get; }
    public uint BaseConsistency { get; }
    public byte CommandTics { get; }
    public byte ConsistencyTics { get; }
    public byte StabilityBuffer { get; }
    public ushort BodyBytes { get; }
    public byte ProtocolVersion { get; }

    public static bool LooksLikeHeader(ReadOnlySpan<byte> payload) =>
        payload.Length >= LiveConstants.ClientInputHeaderSize
        && payload[..4].SequenceEqual(LiveConstants.ClientInputMagic);

    public static bool TryRead(ReadOnlySpan<byte> payload, out ClientInputHeader header)
    {
        header = default;
        if (!LooksLikeHeader(payload))
            return false;

        header = new ClientInputHeader(
            payload[5],
            payload[6],
            payload[7],
            BinaryPrimitives.ReadUInt32BigEndian(payload[8..]),
            BinaryPrimitives.ReadUInt32BigEndian(payload[12..]),
            BinaryPrimitives.ReadUInt32BigEndian(payload[16..]),
            BinaryPrimitives.ReadUInt32BigEndian(payload[20..]),
            payload[24],
            payload[25],
            payload[26],
            BinaryPrimitives.ReadUInt16BigEndian(payload[27..]),
            payload[4]);
        return true;
    }

    public static int Write(Span<byte> payload, ClientInputHeader header)
    {
        if (payload.Length < LiveConstants.ClientInputHeaderSize)
            return 0;

        LiveConstants.ClientInputMagic.CopyTo(payload);
        payload[4] = header.ProtocolVersion;
        payload[5] = header.ControlFlags;
        payload[6] = header.RoutingByte;
        payload[7] = header.PlayerCount;
        BinaryPrimitives.WriteUInt32BigEndian(payload[8..], header.SequenceAck);
        BinaryPrimitives.WriteUInt32BigEndian(payload[12..], header.ConsistencyAck);
        BinaryPrimitives.WriteUInt32BigEndian(payload[16..], header.BaseSequence);
        BinaryPrimitives.WriteUInt32BigEndian(payload[20..], header.BaseConsistency);
        payload[24] = header.CommandTics;
        payload[25] = header.ConsistencyTics;
        payload[26] = header.StabilityBuffer;
        BinaryPrimitives.WriteUInt16BigEndian(payload[27..], header.BodyBytes);
        return LiveConstants.ClientInputHeaderSize;
    }

    public static bool ValidateHeader(
        ClientInputHeader header,
        int payloadSize,
        out string? rejectReason)
    {
        rejectReason = null;
        const NetCommandFlags disallowedFlags =
            NetCommandFlags.Exit
            | NetCommandFlags.Setup
            | NetCommandFlags.LevelReady
            | NetCommandFlags.Quitters
            | NetCommandFlags.Latency
            | NetCommandFlags.LatencyAck
            | NetCommandFlags.Compressed;

        if (header.ProtocolVersion != LiveConstants.ClientInputProtocolVersion)
        {
            rejectReason = "client-input-version-mismatch";
            return false;
        }

        if ((header.ControlFlags & (byte)disallowedFlags) != 0)
        {
            rejectReason = "client-input-disallowed-control-flags";
            return false;
        }

        if (header.PlayerCount > NetConstants.MaxPlayers)
        {
            rejectReason = "client-input-player-count-overflow";
            return false;
        }

        if (header.CommandTics > NetConstants.MaxSendTics)
        {
            rejectReason = "client-input-command-tics-overflow";
            return false;
        }

        if (header.ConsistencyTics > NetConstants.MaxSendTics)
        {
            rejectReason = "client-input-consistency-tics-overflow";
            return false;
        }

        if (header.PlayerCount == 0 && (header.CommandTics != 0 || header.ConsistencyTics != 0))
        {
            rejectReason = "client-input-empty-player-records-with-tics";
            return false;
        }

        if (LiveConstants.ClientInputHeaderSize + header.BodyBytes != payloadSize)
        {
            rejectReason = "client-input-body-length-mismatch";
            return false;
        }

        return true;
    }
}

public readonly struct ClientInputRecordsHeader
{
    public ClientInputRecordsHeader(byte playerCount, byte protocolVersion = LiveConstants.ClientInputRecordsProtocolVersion)
    {
        PlayerCount = playerCount;
        ProtocolVersion = protocolVersion;
    }

    public byte PlayerCount { get; }
    public byte ProtocolVersion { get; }

    public static bool TryRead(ReadOnlySpan<byte> body, out ClientInputRecordsHeader header)
    {
        header = default;
        if (body.Length < LiveConstants.ClientInputRecordsHeaderSize)
            return false;
        if (!body[..4].SequenceEqual(LiveConstants.ClientInputRecordsMagic))
            return false;

        header = new ClientInputRecordsHeader(body[5], body[4]);
        return true;
    }

    public static int Write(Span<byte> body, ClientInputRecordsHeader header)
    {
        if (body.Length < LiveConstants.ClientInputRecordsHeaderSize)
            return 0;

        LiveConstants.ClientInputRecordsMagic.CopyTo(body);
        body[4] = header.ProtocolVersion;
        body[5] = header.PlayerCount;
        return LiveConstants.ClientInputRecordsHeaderSize;
    }

    public static bool Validate(ClientInputRecordsHeader records, byte headerPlayerCount, out string? rejectReason)
    {
        rejectReason = null;
        if (records.ProtocolVersion != LiveConstants.ClientInputRecordsProtocolVersion)
        {
            rejectReason = "client-input-record-version-mismatch";
            return false;
        }

        if (records.PlayerCount != headerPlayerCount)
        {
            rejectReason = "client-input-record-player-count-mismatch";
            return false;
        }

        return true;
    }
}
