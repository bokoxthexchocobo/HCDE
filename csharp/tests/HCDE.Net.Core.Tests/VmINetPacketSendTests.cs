namespace HCDE.Net.Core.Tests;

public class VmINetPacketSendTests
{
    [Fact]
    public void FindClientNative_MatchesAddress()
    {
        var idx = VmINetPacketSend.FindClientNative(
            0x01020304, 5029, 4,
            i => i == 1 ? (byte)1 : (byte)0,
            i => i == 1 ? 0x01020304u : 0u,
            i => i == 1 ? (ushort)5029 : (ushort)0,
            0);
        Assert.Equal(1, idx);
    }

    [Fact]
    public void IClearClientNative_InvokesClear()
    {
        int? cleared = null;
        VmINetPacketSend.IClearClientNative(3, c => cleared = c);
        Assert.Equal(3, cleared);
    }
}
