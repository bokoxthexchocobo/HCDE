namespace HCDE.Net.Core.Tests;

public class VmINetReliableQueueTests
{
    [Fact]
    public void WriteBe16Native_RoundTrips()
    {
        Span<byte> buf = stackalloc byte[2];
        VmINetReliableQueue.WriteBe16Native(buf, 0x1234);
        Assert.Equal(0x1234, VmINetReliableQueue.ReadBe16Native(buf));
    }

    [Fact]
    public void CommitReliableHcdePregameServiceNative_QueuesPacket()
    {
        var slots = new[] { new HcdeReliableServiceSlot() };
        var packet = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 };
        Assert.True(VmINetReliableQueue.CommitReliableHcdePregameServiceNative(
            slots, 1, 0, packet, packet.Length, 7, _ => slots[0], null));
        Assert.True(slots[0].Active);
    }
}
