using System.Buffers.Binary;

namespace HCDE.Net.Core;

public readonly struct GameplayEnvelope
{
    public GameplayEnvelope(
        GameplayPayloadKind kind,
        byte roomId,
        GameplayEnvelopeFlags flags,
        uint gameTic,
        byte protocolVersion = LiveConstants.GameplayProtocolVersion)
    {
        Kind = kind;
        RoomId = roomId;
        Flags = flags;
        GameTic = gameTic;
        ProtocolVersion = protocolVersion;
    }

    public GameplayPayloadKind Kind { get; }
    public byte RoomId { get; }
    public GameplayEnvelopeFlags Flags { get; }
    public uint GameTic { get; }
    public byte ProtocolVersion { get; }

    public static bool TryRead(ReadOnlySpan<byte> payload, out GameplayEnvelope envelope)
    {
        envelope = default;
        if (payload.Length < LiveConstants.GameplayHeaderSize)
            return false;
        if (!payload[..4].SequenceEqual(LiveConstants.GameplayMagic))
            return false;

        var version = payload[4];
        var kind = (GameplayPayloadKind)payload[5];
        var room = payload[6];
        var flags = (GameplayEnvelopeFlags)payload[7];
        var gameTic = BinaryPrimitives.ReadUInt32BigEndian(payload[8..]);
        envelope = new GameplayEnvelope(kind, room, flags, gameTic, version);
        return true;
    }

    public static int Write(Span<byte> payload, GameplayEnvelope envelope)
    {
        if (payload.Length < LiveConstants.GameplayHeaderSize)
            return 0;

        LiveConstants.GameplayMagic.CopyTo(payload);
        payload[4] = envelope.ProtocolVersion;
        payload[5] = (byte)envelope.Kind;
        payload[6] = envelope.RoomId;
        payload[7] = (byte)envelope.Flags;
        BinaryPrimitives.WriteUInt32BigEndian(payload[8..], envelope.GameTic);
        return LiveConstants.GameplayHeaderSize;
    }

    public static bool Validate(
        GameplayEnvelope envelope,
        GameplayPayloadKind expectedKind,
        byte currentRoomId,
        out string? rejectReason)
    {
        rejectReason = null;
        var permittedFlags = expectedKind == GameplayPayloadKind.ClientInputs
            ? GameplayEnvelopeFlags.ActorRepairRequest
            : GameplayEnvelopeFlags.None;

        if (envelope.ProtocolVersion != LiveConstants.GameplayProtocolVersion)
        {
            rejectReason = $"unsupported gameplay version {envelope.ProtocolVersion}";
            return false;
        }

        if (envelope.Kind != expectedKind)
        {
            rejectReason = $"unexpected gameplay kind {envelope.Kind}";
            return false;
        }

        if ((envelope.Flags & ~permittedFlags) != 0)
        {
            rejectReason = $"unsupported gameplay flags {(byte)envelope.Flags}";
            return false;
        }

        if (envelope.RoomId != currentRoomId)
        {
            rejectReason = $"stale room {envelope.RoomId} (current {currentRoomId})";
            return false;
        }

        return true;
    }
}
