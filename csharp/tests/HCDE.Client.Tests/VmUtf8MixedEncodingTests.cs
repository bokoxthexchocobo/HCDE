namespace HCDE.Client.Tests;

public class VmUtf8MixedEncodingTests
{
    [Fact]
    public void GetCharFromString_DecodesWin1252Euro()
    {
        ReadOnlySpan<byte> data = [(byte)0x80];
        var chr = VmUtf8MixedEncoding.GetCharFromString(ref data);
        Assert.Equal(0x20AC, chr);
        Assert.True(data.IsEmpty);
    }

    [Fact]
    public void MakeUtf8_ConvertsMixed()
    {
        var text = VmUtf8MixedEncoding.MakeUtf8("\x80");
        Assert.Equal("€", text);
    }
}
