using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1459SliceTests
{
    [Fact]
    public void ResetBudgetShapeAndInterval()
    {
        var world = new VmNet1459Slice.World();
        world.Bandwidth.Auto.Resolved = VmNet1458Slice.Light;
        world.Bandwidth.Auto.LastSample = 4;
        VmNet1459Slice.Reset(world);
        Assert.Equal(VmNet1458Slice.Medium, world.Bandwidth.Auto.Resolved);
        Assert.Equal(0UL, world.Bandwidth.Auto.LastSample);

        Assert.Equal(0, VmNet1459Slice.BudgetBytes(world, 1, 4));
        Assert.Equal(2000, VmNet1459Slice.BudgetEnd(world, 1, 4, 100, 2000));
        world.Peers[1].Negotiated = VmNet1459Slice.LaneBudgets;
        world.Bandwidth.Bandwidth = "heavy";
        Assert.Equal(3000, VmNet1459Slice.BudgetBytes(world, 1, 4));
        Assert.Equal(2000, VmNet1459Slice.BudgetEnd(world, 1, 4, 100, 2000));
        Assert.Equal(0, world.ProfileClamps[4]);
        Assert.Equal(3100, VmNet1459Slice.BudgetEnd(world, 1, 4, 100, 5000));
        Assert.Equal(1, world.ProfileClamps[4]);
        Assert.Equal(1, world.Peers[1].Clamps[4]);
        Assert.Equal("none", VmNet1459Slice.PrintNames(0, 0));
        Assert.Equal("control-v1 lane-budgets-v1", VmNet1459Slice.PrintNames((1UL << 0) | VmNet1459Slice.LaneBudgets, 0));
        Assert.Equal("none", VmNet1459Slice.PrintNames(VmNet1459Slice.Predator, 0));
        Assert.Equal("control-v1 unknown-0x100000", VmNet1459Slice.PrintNames(1, 0x100000));

        var packet = new byte[15];
        packet[1] = (byte)'H'; packet[2] = (byte)'L'; packet[3] = (byte)'I'; packet[4] = (byte)'V';
        Assert.True(VmNet1459Slice.LooksLike(packet, 15));
        Assert.False(VmNet1459Slice.LooksLike(packet, 14));
        packet[0] = 1;
        Assert.False(VmNet1459Slice.LooksLike(packet, 15));
        ulong last = 0;
        Assert.True(VmNet1459Slice.ReportElapsed(ref last, 1000, 10));
        Assert.False(VmNet1459Slice.ReportElapsed(ref last, 1000, 20));
        Assert.Equal(10UL, last);
        Assert.True(VmNet1459Slice.ReportElapsed(ref last, 1000, 5));
    }

    [Fact]
    public void ApplyAcceptsExtensionAndRejectsLegacy()
    {
        var world = new VmNet1459Slice.World { LocalSession = 0x2A, LocalAuthority = true };
        world.Buffer = new byte[64];
        VmNet1459Slice.Apply(world, 3, 19);
        Assert.Equal(1, world.Peers[3].LegacyControl);
        Assert.Equal(1, world.LegacyControls);
        Assert.Empty(world.Logs);

        world.Buffer[21] = (byte)'H';
        VmNet1459Slice.Apply(world, 3, 20);
        Assert.Equal(2, world.LegacyControls);

        world.Buffer[22] = (byte)'C';
        world.Buffer[23] = (byte)'A';
        world.Buffer[24] = (byte)'P';
        world.Buffer[25] = 2;
        VmNet1459Slice.Apply(world, 3, 20);
        Assert.Contains("ignored HCDE live capabilities from client=3 version=2 flags=0x0", Assert.Single(world.Logs));
        Assert.Equal(1, world.Peers[3].Unsupported);

        world.Logs.Clear();
        world.Buffer[25] = 1;
        world.Buffer[26] = 0;
        ulong remote = VmNet1457Slice.LocalCapabilities() | VmNet1459Slice.Predator | (1UL << 20);
        VmNet1457Slice.WriteBE64(world.Buffer, 27, remote);
        VmNet1459Slice.Apply(world, 3, 20);
        Assert.Equal(VmNet1457Slice.LocalCapabilities(), world.Peers[3].Negotiated);
        Assert.Equal(1UL << 20, world.Peers[3].UnsupportedCaps);
        Assert.Equal(0u, world.Peers[3].RemoteSession);
        Assert.Contains(world.Logs, line => line.Contains("remote=0x1f003f negotiated=0xf001f"));
        Assert.Contains(world.Logs, line => line.Contains("unknown capabilities=0x100000"));

        VmNet1459Slice.Apply(world, 3, 20);
        Assert.Equal(3, world.Logs.Count);
        Assert.Single(world.Logs, line => line.Contains("remote=0x1f003f"));
        Assert.Equal(2, world.CapabilityControls);

        var built = new byte[64];
        VmNet1459Slice.Append(built, 15, 0x11);
        world.Buffer = built;
        world.Logs.Clear();
        world.Peers[3].Negotiated = VmNet1457Slice.LocalCapabilities();
        VmNet1459Slice.Apply(world, 3, 24);
        Assert.Equal(0x11u, world.Peers[3].RemoteSession);
        Assert.Contains("PEER_CORRELATE local_sess=0000002A client=3 remote_sess=00000011 negotiated=0xf001f role=authority", Assert.Single(world.Logs));
        world.LocalAuthority = false;
        world.Netgame = false;
        world.Logs.Clear();
        world.Peers[3].Negotiated = 0;
        VmNet1459Slice.Apply(world, 3, 24);
        Assert.Contains(world.Logs, line => line.Contains("role=local"));
    }
}
