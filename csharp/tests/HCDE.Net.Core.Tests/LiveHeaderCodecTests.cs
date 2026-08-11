using System.Buffers.Binary;

namespace HCDE.Net.Core.Tests;

public class LiveHeaderCodecTests
{
    [Fact]
    public void LiveHeader_RoundTrip_MatchesCppLayout()
    {
        var header = new LiveHeader(LiveMessageType.ServerSnapshot, 42, 17);
        Span<byte> buffer = stackalloc byte[LiveConstants.HeaderSize + 8];
        Assert.Equal(LiveConstants.HeaderSize, LiveHeader.Write(buffer, header));

        Assert.True(LiveHeader.LooksLikePacket(buffer));
        Assert.True(LiveHeader.TryRead(buffer, out var parsed));
        Assert.Equal(header.MessageType, parsed.MessageType);
        Assert.Equal(header.TxSequence, parsed.TxSequence);
        Assert.Equal(header.Acknowledgement, parsed.Acknowledgement);
        Assert.Equal(LiveConstants.ProtocolVersion, parsed.ProtocolVersion);

        Assert.Equal((byte)0, buffer[0]);
        Assert.True(buffer.Slice(1, 4).SequenceEqual("HLIV"u8));
        Assert.Equal((byte)LiveMessageType.ServerSnapshot, buffer[6]);
        Assert.Equal(42u, BinaryPrimitives.ReadUInt32BigEndian(buffer[7..]));
        Assert.Equal(17u, BinaryPrimitives.ReadUInt32BigEndian(buffer[11..]));
    }

    [Fact]
    public void LiveHeader_RejectsShortOrWrongMagic()
    {
        Assert.False(LiveHeader.LooksLikePacket(ReadOnlySpan<byte>.Empty));
        Span<byte> bad = [1, (byte)'H', (byte)'L', (byte)'I', (byte)'V', 1, 1, 0, 0, 0, 1, 0, 0, 0, 0];
        Assert.False(LiveHeader.LooksLikePacket(bad));
    }
}
