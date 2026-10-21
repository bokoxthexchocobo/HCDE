namespace HCDE.Net.Core.Tests;

public class VmINetGetPacketTests
{
    [Fact]
    public void FinishGetPacketNative_ClampsLength()
    {
        var remote = 0;
        var len = 0;
        VmINetGetPacket.FinishGetPacketNative(2, 99999, 100, ref remote, ref len, null);
        Assert.Equal(2, remote);
        Assert.Equal(100, len);
    }

    [Fact]
    public void TryUncompressPayloadNative_CopiesUncompressed()
    {
        Span<byte> tx = [0, 0, 0, 0, 0x00, 1, 2, 3];
        Span<byte> net = stackalloc byte[8];
        var outSize = 0;
        Assert.True(VmINetGetPacket.TryUncompressPayloadNative(tx, 8, net, ref outSize, null));
        Assert.Equal(4, outSize);
    }
}
