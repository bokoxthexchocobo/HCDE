namespace HCDE.Net.Core.Tests;

public class VmSvMasterResolveTests
{
    [Fact]
    public void IsMasterNameNative_DistinguishesIpFromHostname()
    {
        Assert.False(VmSvMasterResolve.IsMasterNameNative("192.168.0.1"));
        Assert.True(VmSvMasterResolve.IsMasterNameNative("master.example.com"));
    }

    [Fact]
    public void TryParseMasterEndpointNative_ParsesOptionalPort()
    {
        VmSvMasterResolve.TryParseMasterEndpointNative("host:5029", 28910, out var host, out var port, null);
        Assert.Equal("host", host);
        Assert.Equal((ushort)5029, port);
    }

    [Fact]
    public void NextNms1RequestIdNative_SkipsZero()
    {
        var state = new MasterServerNms1State { NextRequestId = uint.MaxValue };
        var id = VmSvMasterResolve.NextNms1RequestIdNative(ref state);
        Assert.Equal(uint.MaxValue, id);
        Assert.Equal(1u, state.NextRequestId);
    }

    [Fact]
    public void MakeNms1TextNative_UsesFallbackWhenEmpty()
    {
        var text = VmSvMasterResolve.MakeNms1TextNative("  \t", 32, "HCDE");
        Assert.Equal("HCDE", text);
    }
}
