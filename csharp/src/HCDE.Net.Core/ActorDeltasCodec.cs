namespace HCDE.Net.Core;

public readonly struct ActorDeltasHeader
{
    public ActorDeltasHeader(byte flags, byte recordCount, byte protocolVersion = LiveConstants.ActorDeltasProtocolVersion)
    {
        Flags = flags;
        RecordCount = recordCount;
        ProtocolVersion = protocolVersion;
    }

    public byte Flags { get; }
    public byte RecordCount { get; }
    public byte ProtocolVersion { get; }

    public static bool TryRead(ReadOnlySpan<byte> chunk, out ActorDeltasHeader header)
    {
        header = default;
        if (chunk.Length < LiveConstants.ActorDeltasHeaderSize)
            return false;
        if (!chunk[..4].SequenceEqual(LiveConstants.ActorDeltasMagic))
            return false;

        header = new ActorDeltasHeader(chunk[5], chunk[6], chunk[4]);
        return true;
    }

    public static int Write(Span<byte> chunk, ActorDeltasHeader header)
    {
        if (chunk.Length < LiveConstants.ActorDeltasHeaderSize)
            return 0;

        LiveConstants.ActorDeltasMagic.CopyTo(chunk);
        chunk[4] = header.ProtocolVersion;
        chunk[5] = header.Flags;
        chunk[6] = header.RecordCount;
        chunk[7] = 0;
        return LiveConstants.ActorDeltasHeaderSize;
    }
}

public static class ActorDeltasCodec
{
    public static int WriteEmpty(Span<byte> chunk) =>
        ActorDeltasHeader.Write(chunk, new ActorDeltasHeader(LiveConstants.ActorDeltasFlagComplete, recordCount: 0));
}
