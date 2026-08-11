using System.Buffers.Binary;

namespace HCDE.Net.Core;

public readonly struct LiveHeader
{
    public LiveHeader(
        LiveMessageType messageType,
        uint txSequence,
        uint acknowledgement,
        byte protocolVersion = LiveConstants.ProtocolVersion)
    {
        MessageType = messageType;
        TxSequence = txSequence;
        Acknowledgement = acknowledgement;
        ProtocolVersion = protocolVersion;
    }

    public LiveMessageType MessageType { get; }
    public uint TxSequence { get; }
    public uint Acknowledgement { get; }
    public byte ProtocolVersion { get; }

    public static bool LooksLikePacket(ReadOnlySpan<byte> buffer) =>
        buffer.Length >= LiveConstants.HeaderSize
        && buffer[0] == 0
        && buffer.Slice(1, 4).SequenceEqual(LiveConstants.LiveMagic);

    public static bool TryRead(ReadOnlySpan<byte> buffer, out LiveHeader header)
    {
        header = default;
        if (!LooksLikePacket(buffer))
            return false;

        var version = buffer[5];
        var type = (LiveMessageType)buffer[6];
        var txSequence = BinaryPrimitives.ReadUInt32BigEndian(buffer[7..]);
        var acknowledgement = BinaryPrimitives.ReadUInt32BigEndian(buffer[11..]);
        header = new LiveHeader(type, txSequence, acknowledgement, version);
        return true;
    }

    public static int Write(Span<byte> buffer, LiveHeader header)
    {
        if (buffer.Length < LiveConstants.HeaderSize)
            return 0;

        buffer[0] = 0;
        LiveConstants.LiveMagic.CopyTo(buffer[1..]);
        buffer[5] = header.ProtocolVersion;
        buffer[6] = (byte)header.MessageType;
        BinaryPrimitives.WriteUInt32BigEndian(buffer[7..], header.TxSequence);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[11..], header.Acknowledgement);
        return LiveConstants.HeaderSize;
    }
}
