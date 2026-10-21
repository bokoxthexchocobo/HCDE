namespace HCDE.Net.Core.Tests;

public class VmDNetGetNetBufferSizeTests
{
    [Fact]
    public void GetNetBufferSizeNative_ZeroLength_ReturnsZero()
    {
        Assert.Equal(0u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(ReadOnlySpan<byte>.Empty, 0, false, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_ExitPacket_IncludesAuthorityByte()
    {
        var buf = new byte[] { VmDNetGetNetBufferSize.NcmdExit };
        Assert.Equal(2u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 1, true, () => false));
        Assert.Equal(1u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 1, false, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_Setup_TruncatedReturnsTwo()
    {
        var buf = new byte[] { VmDNetGetNetBufferSize.NcmdSetup };
        Assert.Equal(2u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 1, false, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_Setup_FullLengthPassthrough()
    {
        var buf = new byte[] { VmDNetGetNetBufferSize.NcmdSetup, 1, 2 };
        Assert.Equal(3u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 3, false, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_Latency_ReturnsTwo()
    {
        var buf = new byte[] { VmDNetGetNetBufferSize.NcmdLatency };
        Assert.Equal(2u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 1, false, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_LivePacket_ReturnsBufferLength()
    {
        var buf = new byte[] { 0 };
        Assert.Equal(5u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 5, false, () => true));
    }

    [Fact]
    public void GetNetBufferSizeNative_LevelReady_NonAuthorityTwoBytes()
    {
        var buf = new byte[] { VmDNetGetNetBufferSize.NcmdLevelReady, 0 };
        Assert.Equal(2u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 2, false, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_LevelReady_AddsGameticBytesForAuthority()
    {
        var buf = new byte[] { VmDNetGetNetBufferSize.NcmdLevelReady, 0 };
        Assert.Equal(8u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 2, true, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_GameplayHeader_TruncatedReturnsTen()
    {
        var buf = new byte[9];
        Assert.Equal(10u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 9, false, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_Quitters_ExtendsByCountByte()
    {
        var buf = new byte[12];
        buf[0] = VmDNetGetNetBufferSize.NcmdQuitters;
        buf[10] = 2;
        Assert.Equal(13u, VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 11, false, () => false));
    }

    [Fact]
    public void GetNetBufferSizeNative_NumTicsPositive_AddsFourBytes()
    {
        var buf = new byte[20];
        buf[10] = 1;
        buf[11] = 3;
        Assert.True(VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 12, false, () => false) >= 16);
    }

    [Fact]
    public void GetNetBufferSizeNative_PlayerPadding_WhenAuthorityAndShortBuffer()
    {
        var buf = new byte[20];
        buf[10] = 2;
        buf[11] = 0;
        buf[12] = 0;
        var need = VmDNetGetNetBufferSize.GetNetBufferSizeNative(buf, 14, true, () => false);
        Assert.Equal(20u, need);
    }

    [Fact]
    public void HSendPacketShouldSkip_WhenDemoPlayback() =>
        Assert.True(VmDNetGetNetBufferSize.HSendPacketShouldSkip(true));

    [Fact]
    public void HSendPacketShouldNotSkip_WhenNotDemoPlayback() =>
        Assert.False(VmDNetGetNetBufferSize.HSendPacketShouldSkip(false));

    [Fact]
    public void HGetPacketFromLocalBuffer_ReturnsFalseWhenEmpty()
    {
        uint local = 0;
        Assert.False(VmDNetGetNetBufferSize.HGetPacketFromLocalBuffer(ref local, out var remote));
        Assert.Equal(0, remote);
    }

    [Fact]
    public void HGetPacketFromLocalBuffer_ClearsLocalAndReturnsTrue()
    {
        uint local = 8;
        Assert.True(VmDNetGetNetBufferSize.HGetPacketFromLocalBuffer(ref local, out _));
        Assert.Equal(0u, local);
    }
}
