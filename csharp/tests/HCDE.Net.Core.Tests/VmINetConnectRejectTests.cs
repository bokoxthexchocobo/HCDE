namespace HCDE.Net.Core.Tests;

public class VmINetConnectRejectTests
{
    [Fact]
    public void RejectConnectionNative_SetsSetupPacket()
    {
        var buf = new byte[8];
        var len = 0;
        VmINetConnectReject.RejectConnectionNative(0x80, 3, buf, ref len, () => { });
        Assert.Equal(2, len);
        Assert.Equal(0x80, buf[0]);
        Assert.Equal(3, buf[1]);
    }

    [Fact]
    public void SendVerificationErrorNative_EnginePayload()
    {
        var buf = new byte[16];
        var len = 0;
        var err = new VerificationErrorPayload(1, 2, 3, 4, 5, 6, 7, null);
        VmINetConnectReject.SendVerificationErrorNative(0x80, 9, 1, err, buf, ref len, 512, () => { }, null);
        Assert.Equal(9, len);
        Assert.Equal(2, buf[3]);
        Assert.Equal(7, buf[8]);
    }
}
