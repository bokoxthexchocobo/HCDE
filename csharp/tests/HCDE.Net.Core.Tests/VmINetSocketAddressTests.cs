namespace HCDE.Net.Core.Tests;

public class VmINetSocketAddressTests
{
    [Fact]
    public void ValidateNetPasswordCvarNative_RejectsLongPassword()
    {
        var failed = false;
        Assert.False(VmINetSocketAddress.ValidateNetPasswordCvarNative(
            new string('x', 300), 256, _ => failed = true));
        Assert.True(failed);
    }

    [Fact]
    public void TryBuildAddressNative_ParsesHostPort()
    {
        Assert.True(VmINetSocketAddress.TryBuildAddressNative(
            "127.0.0.1:5029", 5029, (_, _) => true, out var parts, null));
        Assert.Equal("127.0.0.1", parts.Host);
        Assert.Equal((ushort)5029, parts.Port);
    }

    [Fact]
    public void ReadQueryStringNative_ReadsNullTerminated()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("hello\0rest");
        var offset = 0;
        Assert.True(VmINetSocketAddress.ReadQueryStringNative(data, ref offset, data.Length, out var value));
        Assert.Equal("hello", value);
    }
}
