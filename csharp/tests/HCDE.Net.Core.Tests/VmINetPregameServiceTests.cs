namespace HCDE.Net.Core.Tests;

public class VmINetPregameServiceTests
{
    [Fact]
    public void CheckHcdePregameServiceNative_AcceptsAdvancingSeq()
    {
        var state = new HcdeConnectionServiceState
        {
            SessionToken = 0x11223344,
            RxSeq = 1,
            TxSeq = 5,
            PeerAck = 0,
            PendingServices = new HcdePendingServiceSlot[4],
        };
        Span<byte> buf = stackalloc byte[20];
        buf[3] = 0x11;
        buf[4] = 0x22;
        buf[5] = 0x33;
        buf[6] = 0x44;
        VmINetPregameService.WriteBe32Native(buf.Slice(7, 4), 2);
        VmINetPregameService.WriteBe32Native(buf.Slice(11, 4), 0);
        var ok = VmINetPregameService.CheckHcdePregameServiceNative(
            ref state, netBufferLength: 15, minimumSize: 15, buf, now: 1000,
            malformedStrikeLimit: 4, quarantineMs: 3000);
        Assert.True(ok);
        Assert.Equal(2u, state.RxSeq);
    }
}
