namespace HCDE.Net.Core.Tests;

public class VmSvMasterHeartbeatTests
{
    [Fact]
    public void BuildLegacyHeartbeatPacketNative_WritesMarkerAndPort()
    {
        Span<byte> buf = stackalloc byte[8];
        var len = VmSvMasterHeartbeat.BuildLegacyHeartbeatPacketNative(buf, 5029);
        Assert.Equal(6, len);
        Assert.Equal(0xff, buf[0]);
        Assert.Equal(0xA5, buf[4]);
        Assert.Equal(0x13, buf[5]);
    }

    [Fact]
    public void MasterListContainsNative_IsCaseInsensitive()
    {
        Assert.True(VmSvMasterHeartbeat.MasterListContainsNative("Master.Host", "master.host"));
    }

    [Fact]
    public void IsMissingStartupMasterValueNative_DetectsNextSwitch()
    {
        Assert.True(VmSvMasterHeartbeat.IsMissingStartupMasterValueNative(0, 2, i => "-foo"));
    }
}
