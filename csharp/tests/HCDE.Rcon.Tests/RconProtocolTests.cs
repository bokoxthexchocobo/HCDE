using HCDE.Protocol;

namespace HCDE.Rcon.Tests;

public class RconProtocolTests
{
    [Fact]
    public void Fnv1aHashMatchesCppImplementation()
    {
        var hash = RconProtocol.Fnv1aHash("nonce123:secret");
        Assert.Equal("c3b0293e", RconProtocol.FormatAuthHash(hash));
    }

    [Fact]
    public void FormatAuthHashUsesLowerHex()
    {
        Assert.Equal("0000002a", RconProtocol.FormatAuthHash(42));
        Assert.Equal("ffffffff", RconProtocol.FormatAuthHash(uint.MaxValue));
    }

    [Fact]
    public void Fnv1aHashIsDeterministic()
    {
        var first = RconProtocol.Fnv1aHash("auth test");
        var second = RconProtocol.Fnv1aHash("auth test");
        Assert.Equal(first, second);
    }
}
