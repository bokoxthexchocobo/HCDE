namespace HCDE.Client.Tests;

public class VmUtf8CodecTests
{
    [Fact]
    public void EncodeDecode_RoundTripsAscii()
    {
        Span<byte> buffer = stackalloc byte[4];
        Assert.True(VmUtf8Codec.TryEncode('A', buffer, out var size));
        Assert.True(VmUtf8Codec.TryDecode(buffer[..size], out var cp, out _));
        Assert.Equal('A', cp);
    }
}
