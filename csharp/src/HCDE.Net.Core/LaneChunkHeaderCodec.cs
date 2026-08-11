using System.Buffers.Binary;

namespace HCDE.Net.Core;

public readonly struct ServerWorldDeltaHeader
{
    public ServerWorldDeltaHeader(byte flags, uint gameTic, byte recordCount, byte protocolVersion = LiveConstants.ServerWorldDeltaProtocolVersion)
    {
        Flags = flags;
        GameTic = gameTic;
        RecordCount = recordCount;
        ProtocolVersion = protocolVersion;
    }

    public byte Flags { get; }
    public uint GameTic { get; }
    public byte RecordCount { get; }
    public byte ProtocolVersion { get; }

    public static bool TryRead(ReadOnlySpan<byte> chunk, out ServerWorldDeltaHeader header)
    {
        header = default;
        if (chunk.Length < LiveConstants.ServerWorldDeltaHeaderSize)
            return false;
        if (!chunk[..4].SequenceEqual(LiveConstants.ServerWorldDeltaMagic))
            return false;

        header = new ServerWorldDeltaHeader(
            chunk[5],
            BinaryPrimitives.ReadUInt32BigEndian(chunk[6..]),
            chunk[10],
            chunk[4]);
        return true;
    }

    public static int Write(Span<byte> chunk, ServerWorldDeltaHeader header)
    {
        if (chunk.Length < LiveConstants.ServerWorldDeltaHeaderSize)
            return 0;

        LiveConstants.ServerWorldDeltaMagic.CopyTo(chunk);
        chunk[4] = header.ProtocolVersion;
        chunk[5] = header.Flags;
        BinaryPrimitives.WriteUInt32BigEndian(chunk[6..], header.GameTic);
        chunk[10] = header.RecordCount;
        return LiveConstants.ServerWorldDeltaHeaderSize;
    }
}

public readonly struct AuthorityEventsHeader
{
    public AuthorityEventsHeader(byte flags, byte eventCount, byte protocolVersion = LiveConstants.AuthorityEventsProtocolVersion)
    {
        Flags = flags;
        EventCount = eventCount;
        ProtocolVersion = protocolVersion;
    }

    public byte Flags { get; }
    public byte EventCount { get; }
    public byte ProtocolVersion { get; }

    public static bool TryRead(ReadOnlySpan<byte> chunk, out AuthorityEventsHeader header)
    {
        header = default;
        if (chunk.Length < LiveConstants.AuthorityEventsHeaderSize)
            return false;
        if (!chunk[..4].SequenceEqual(LiveConstants.AuthorityEventsMagic))
            return false;

        header = new AuthorityEventsHeader(chunk[5], chunk[6], chunk[4]);
        return true;
    }

    public static int Write(Span<byte> chunk, AuthorityEventsHeader header)
    {
        if (chunk.Length < LiveConstants.AuthorityEventsHeaderSize)
            return 0;

        LiveConstants.AuthorityEventsMagic.CopyTo(chunk);
        chunk[4] = header.ProtocolVersion;
        chunk[5] = header.Flags;
        chunk[6] = header.EventCount;
        chunk[7] = 0;
        return LiveConstants.AuthorityEventsHeaderSize;
    }
}
