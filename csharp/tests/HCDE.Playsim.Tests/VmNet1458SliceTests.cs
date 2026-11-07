using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1458SliceTests
{
    [Fact]
    public void CapabilityRejectThrottlesPerSecond()
    {
        var world = new VmNet1458Slice.World();
        VmNet1458Slice.Reject(world, 64, 1, "in", true, 1000);
        Assert.Equal(0, world.CapabilityRejects);
        world.Peers[2].Negotiated = 1;
        VmNet1458Slice.Reject(world, 2, 2, null, false, 1000);
        Assert.Equal(0, world.Peers[2].Unsupported);
        Assert.Contains("rejected HCDE live client commands gameplay client=2 negotiated=0x1 required=0xf001f suppressed=0 native-only", Assert.Single(world.Logs));
        VmNet1458Slice.Reject(world, 2, 2, "in", true, 1500);
        VmNet1458Slice.Reject(world, 2, 2, "in", true, 1800);
        Assert.Equal(2u, world.Peers[2].Suppressed);
        Assert.Equal(2, world.Peers[2].Unsupported);
        Assert.Single(world.Logs);
        VmNet1458Slice.Reject(world, 2, 3, "in", true, 50);
        Assert.Contains(world.Logs, line => line.Contains("server snapshot in client=2") && line.Contains("suppressed=2"));
        Assert.Equal(0u, world.Peers[2].Suppressed);
    }

    [Fact]
    public void BandwidthAutoDemotesPromotesAndBudgets()
    {
        Assert.Equal(VmNet1458Slice.Light, VmNet1458Slice.MaxByPlayerCount(6));
        Assert.Equal(VmNet1458Slice.Medium, VmNet1458Slice.MaxByPlayerCount(4));
        Assert.Equal(VmNet1458Slice.Heavy, VmNet1458Slice.MaxByPlayerCount(3));

        var world = new VmNet1458Slice.World { AuthoritySlot = 1 };
        world.Clients.AddRange(new[] { 1, 0, 99, 2 });
        world.Latency[0] = 10;
        world.Latency[1] = 40;
        world.Latency[2] = 25;
        Assert.Equal(25, VmNet1458Slice.MaxRemoteRtt(world, out int peers));
        Assert.Equal(2, peers);

        world.Bandwidth = "HEAVY";
        Assert.Equal(3000, VmNet1458Slice.DefaultBudget(world, VmNet1458Slice.ActorDeltaLane));
        world.Bandwidth = "";
        Assert.Equal(96, VmNet1458Slice.DefaultBudget(world, 0));
        world.Bandwidth = "nope";
        Assert.Equal(0, VmNet1458Slice.DefaultBudget(world, 7));
        world.Auto.Resolved = VmNet1458Slice.Heavy;
        world.Bandwidth = "auto";
        Assert.Equal(3000, VmNet1458Slice.DefaultBudget(world, VmNet1458Slice.ActorDeltaLane));

        world.LocalAuthority = false;
        world.Clamps = 9;
        VmNet1458Slice.Tick(world, 5000);
        Assert.Equal(0UL, world.Auto.LastSample);
        world.LocalAuthority = true;
        world.Bandwidth = "light";
        VmNet1458Slice.Tick(world, 5000);
        Assert.Equal(0UL, world.Auto.LastSample);

        world.Bandwidth = "auto";
        world.Auto.Resolved = VmNet1458Slice.Heavy;
        world.Clients.Clear();
        for (int i = 1; i <= 6; i++)
        {
            world.Clients.Add(i);
            world.Latency[i] = 10;
        }
        world.AuthoritySlot = 0;
        VmNet1458Slice.Tick(world, 10000);
        Assert.Equal(10000UL, world.Auto.LastSample);
        VmNet1458Slice.Tick(world, 10499);
        Assert.Equal(10000UL, world.Auto.LastSample);
        VmNet1458Slice.Tick(world, 10500);
        Assert.Contains("auto switched heavy -> light (player-count-cap: clamps/sec=0.00 deferred/sec=0.00 rtt=10ms peers=6 cap=light)", Assert.Single(world.Logs));
        Assert.Equal(VmNet1458Slice.Light, world.Auto.Resolved);

        world.Logs.Clear();
        world.Clamps = 3;
        VmNet1458Slice.Tick(world, 11000);
        Assert.Equal(VmNet1458Slice.Light, world.Auto.Resolved);
        Assert.Empty(world.Logs);
        Assert.Equal(3UL, world.Auto.LastClampSample);

        world.Auto = new VmNet1458Slice.AutoState();
        Assert.Equal(VmNet1458Slice.Medium, world.Auto.Resolved);
        world.PromoteWindowSec = 1;
        world.Clients.Clear();
        world.Clients.Add(2);
        world.Latency[2] = 10;
        world.Clamps = 0;
        world.Deferred = 0;
        VmNet1458Slice.Tick(world, 10000);
        VmNet1458Slice.Tick(world, 10500);
        Assert.Equal(500, world.Auto.CleanWindow);
        VmNet1458Slice.Tick(world, 11000);
        Assert.Equal(VmNet1458Slice.Heavy, world.Auto.Resolved);
        Assert.Contains(world.Logs, line => line.Contains("auto switched medium -> heavy (clean-window-promote:"));
    }
}
