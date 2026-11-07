using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1457SliceTests
{
    [Fact]
    public void AggregateTraceAndEndian()
    {
        Assert.True(VmNet1457Slice.ShouldAggregate(0x01));
        Assert.True(VmNet1457Slice.ShouldAggregate(0x07));
        Assert.False(VmNet1457Slice.ShouldAggregate(0x09));
        Assert.False(VmNet1457Slice.ShouldAggregate(0x00));

        var world = new VmNet1457Slice.World { GameTic = 4, ClientTic = 6, Room = 2, Console = 1 };
        for (int i = 0; i < 31; i++)
            VmNet1457Slice.TraceIn(world, 3, 0x01, 8, 0);
        Assert.Equal(31u, world.InCount);
        Assert.Empty(world.Logs);
        VmNet1457Slice.TraceIn(world, 3, 0x01, 8, 0);
        Assert.Equal(0u, world.InCount);
        Assert.Contains("net.in aggregate packets=32 gametic=4 room=2", Assert.Single(world.Logs));

        world.Logs.Clear();
        VmNet1457Slice.TraceOut(world, 1, 0x01, 8, 50);
        Assert.Equal(0u, world.OutCount);
        VmNet1457Slice.TraceOut(world, 3, 0x0a, 12, 50);
        Assert.Contains("net.out client=3 flags=0x0a size=12 gametic=4 room=2", Assert.Single(world.Logs));
        world.OutCount = 1;
        world.LastLog = 0;
        VmNet1457Slice.Flush(world, 1000);
        Assert.Equal(1000UL, world.LastLog);
        Assert.Equal(0u, world.OutCount);

        var buf = new byte[8];
        VmNet1457Slice.WriteBE16(buf, 0, 0x0102);
        VmNet1457Slice.WriteBE32(buf, 2, 0x03040506);
        VmNet1457Slice.WriteBE64(buf, 0, 0x0102030405060708);
        Assert.Equal(0x0102030405060708UL, VmNet1457Slice.ReadBE64(buf, 0));
        Assert.Equal("control", VmNet1457Slice.MessageName(1));
        Assert.Equal("unknown", VmNet1457Slice.MessageName(9));
        Assert.Equal(0xf001fUL, VmNet1457Slice.LocalCapabilities());
        Assert.Contains(VmNet1457Slice.CapabilityNames, entry => entry.Name == "lane-budgets-v1");
    }

    [Fact]
    public void CapabilitiesAndLegacyReject()
    {
        var world = new VmNet1457Slice.World { Netgame = true, NativeOnly = true, LocalAuthority = true, Room = 8 };
        Assert.Equal(0UL, VmNet1457Slice.Negotiated(world, -1));
        world.Peers[2].Negotiated = 1;
        Assert.True(VmNet1457Slice.HasCapability(world, 2, 1));
        Assert.False(VmNet1457Slice.HasRequired(world, 2));
        world.Peers[2].Negotiated = VmNet1457Slice.LocalCapabilities();
        Assert.True(VmNet1457Slice.HasRequired(world, 2));
        Assert.True(VmNet1457Slice.EnforcesNative(world, 2));
        world.Peers[2].Setup = true;
        Assert.False(VmNet1457Slice.EnforcesNative(world, 2));
        world.Demo = true;
        world.Peers[2].Setup = false;
        Assert.False(VmNet1457Slice.EnforcesNative(world, 2));

        world.Peers[2].Name = "ada";
        world.Peers[2].Sequence = 5;
        world.Peers[2].Ack = 4;
        world.Peers[2].UsesHcde = true;
        world.Peers[2].Negotiated = 1;
        VmNet1457Slice.RejectLegacy(world, -1, "in", "old");
        Assert.Equal(0, world.LegacyRejected);
        VmNet1457Slice.RejectLegacy(world, 2, null, null);
        Assert.Equal(1, world.Peers[2].Unsupported);
        Assert.Equal(VmNet1457Slice.Quit, world.Peers[2].Flags);
        Assert.Contains(world.Logs, line => line.Contains("rejected legacy gameplay gameplay client=2 reason=unknown")
            && line.Contains("negotiated=0x1 required=0xf001f seq=5 ack=4 room=8"));
        Assert.Contains(world.Logs, line => line.Contains("Disconnecting client 2 'ada'") && line.Contains("reason=unknown"));

        world.Peers[3].AuthoritySlot = true;
        VmNet1457Slice.RejectLegacy(world, 3, "out", "kept");
        Assert.Equal(0, world.Peers[3].Flags);
        Assert.Equal(1, world.Peers[3].Unsupported);
    }
}
