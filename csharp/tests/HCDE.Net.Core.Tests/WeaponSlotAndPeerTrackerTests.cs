namespace HCDE.Net.Core.Tests;

public class CanonicalWeaponIndexCodecTests
{
    [Fact]
    public void SingleByteIndex_BecomesBigEndianUInt16()
    {
        var legacy = new byte[] { 42 };
        Span<byte> output = stackalloc byte[8];
        var legacyCursor = 0;
        var cursor = 0;

        Assert.True(CanonicalWeaponIndexCodec.TryAppendFromLegacy(legacy, ref legacyCursor, output, ref cursor));
        Assert.Equal(1, legacyCursor);
        Assert.Equal(2, cursor);
        Assert.Equal(42, System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(output));
    }

    [Fact]
    public void TwoByteIndex_BecomesBigEndianUInt16()
    {
        var legacy = new byte[] { 0x85, 0x02 };
        Span<byte> output = stackalloc byte[8];
        var legacyCursor = 0;
        var cursor = 0;

        Assert.True(CanonicalWeaponIndexCodec.TryAppendFromLegacy(legacy, ref legacyCursor, output, ref cursor));
        Assert.Equal(2, legacyCursor);
        Assert.Equal(0x0105, System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(output));
    }
}

public class LivePeerSlotTrackerTests
{
    [Fact]
    public void ApplyQuitterSlots_MarksPeersDisconnected()
    {
        var tracker = new LivePeerSlotTracker(maxClients: 4);
        Assert.True(tracker.IsConnected(2));

        tracker.ApplyQuitterSlots(new byte[] { 2, 3 });

        Assert.False(tracker.IsConnected(2));
        Assert.False(tracker.IsConnected(3));
        Assert.True(tracker.IsConnected(1));
        Assert.Equal(new[] { 2, 3 }, tracker.DisconnectedSlots);
    }

    [Fact]
    public void MarkDisconnected_IsIdempotent()
    {
        var tracker = new LivePeerSlotTracker(maxClients: 4);
        Assert.True(tracker.MarkDisconnected(1));
        Assert.False(tracker.MarkDisconnected(1));
        Assert.Single(tracker.DisconnectedSlots);
    }
}
