namespace HCDE.Net.Core.Tests;

public class VmDNetGetNetBufferSizeTests
{
    [Fact]
    public void GetNetBufferSizeNative_ExitPacket_IncludesAuthorityByte()
    {
        var buf = new byte[] { VmDNetGetNetBufferSize.NcmdExit };
        Assert.Equal(2u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 1, true, () => false));
        Assert.Equal(1u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 1, false, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_LevelReady_AddsGameticBytesForAuthority()
    {
        var buf = new byte[] { VmDNetGetNetBufferSize.NcmdLevelReady, 0 };
        Assert.Equal(8u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 2, true, () => false));
    }

    [Fact]
    public void HSendPacketShouldSkip_WhenDemoPlayback() =>
        Assert.True(VmDNetGetNetBufferSize.HSendPacketShouldSkip(true));
}
