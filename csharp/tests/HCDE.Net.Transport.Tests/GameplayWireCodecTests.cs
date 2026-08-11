using HCDE.Net.Transport;

namespace HCDE.Net.Transport.Tests;

public class GameplayWireCodecTests
{
    [Fact]
    public void GameplayCrcDiffersFromSetupCrcWhenGameIdPresent()
    {
        var gameId = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        Span<byte> netBuffer = [0, (byte)'H', (byte)'L', (byte)'I', (byte)'V'];
        var setupCrc = Crc32.Calc(netBuffer);
        var gameplayCrc = GameplayWireCodec.ComputeCrc(netBuffer, gameId);
        Assert.NotEqual(setupCrc, gameplayCrc);
    }

    [Fact]
    public void RoundTrip_WithGameId()
    {
        var gameId = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x02, 0x03, 0x04 };
        Span<byte> netBuffer = stackalloc byte[32];
        netBuffer[0] = 0;
        "HLIV"u8.CopyTo(netBuffer[1..]);

        Span<byte> wire = stackalloc byte[GameplayWireCodec.CrcPrefixSize + netBuffer.Length];
        var encoded = GameplayWireCodec.Encode(netBuffer[..5], gameId, wire);
        Assert.True(encoded > 0);

        Span<byte> decoded = stackalloc byte[32];
        var status = GameplayWireCodec.TryDecode(wire[..encoded], gameId, decoded, out var netLength);
        Assert.Equal(GameplayWireDecodeStatus.Ok, status);
        Assert.Equal(5, netLength);
        Assert.True(decoded[..netLength].SequenceEqual(netBuffer[..5]));
    }

    [Fact]
    public void RejectsWrongGameId()
    {
        var gameId = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        Span<byte> net = [0];
        Span<byte> wire = stackalloc byte[16];
        GameplayWireCodec.Encode(net, gameId, wire);

        Span<byte> decoded = stackalloc byte[16];
        var otherId = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 };
        var status = GameplayWireCodec.TryDecode(wire, otherId, decoded, out _);
        Assert.Equal(GameplayWireDecodeStatus.BadCrc, status);
    }
}
