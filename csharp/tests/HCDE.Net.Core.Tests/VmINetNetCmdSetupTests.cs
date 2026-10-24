namespace HCDE.Net.Core.Tests;

public class VmINetNetCmdSetupTests
{
    [Fact]
    public void TryParseStrictIntNative_ParsesDecimal()
    {
        Assert.True(VmINetNetCmdSetup.TryParseStrictIntNative("42", out var v));
        Assert.Equal(42, v);
    }

    [Fact]
    public void INetCmdNative_DispatchesGet()
    {
        var got = false;
        VmINetNetCmdSetup.INetCmdNative(VmINetNetCmdSetup.CmdGet, -1, null, () => got = true);
        Assert.True(got);
    }
}
