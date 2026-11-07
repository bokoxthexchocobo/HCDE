using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1445SliceTests
{
    [Fact]
    public void HeartbeatDelayAndLead()
    {
        var idle = new VmNet1445Slice.World { NetGame = true, LocalAuthority = true };
        VmNet1445Slice.NetUpdate(idle, 0);
        Assert.Equal(1, idle.Packets);
        Assert.Equal(0, idle.Heartbeats);
        Assert.Equal(0, idle.ClientTic);

        var host = new VmNet1445Slice.World
        {
            NetGame = true,
            LocalAuthority = true,
            GameTic = 9,
            LastLatencyUpdate = 30,
            FullLatency = 4,
            LevelStatus = 1,
        };
        VmNet1445Slice.NetUpdate(host, 10);
        Assert.Equal("host heartbeat gametic=9", Assert.Single(host.Logs.FindAll(line => line.StartsWith("host heartbeat"))));
        Assert.Equal(0, host.LastLatencyUpdate);
        Assert.Equal(0, host.FullLatency);
        Assert.Equal(1, host.Heartbeats);
        Assert.Equal(1, host.Masters);
        Assert.Equal(1, host.Consistencies);
        Assert.Equal(0, host.LevelStatus);
        Assert.Equal(1, host.ReadyResets);
        Assert.Contains(host.Logs, line => line == "host -> ready reason=authority-host-gate-skip");
        Assert.Equal(10, host.Built.Count);

        var delayed = new VmNet1445Slice.World { LevelDelay = 3 };
        VmNet1445Slice.NetUpdate(delayed, 10);
        Assert.Equal(0, delayed.LevelDelay);
        Assert.Equal(7, delayed.Built.Count);

        var partial = new VmNet1445Slice.World { LevelDelay = 10 };
        VmNet1445Slice.NetUpdate(partial, 4);
        Assert.Equal(6, partial.LevelDelay);
        Assert.Equal(4, partial.Built.Count);

        var capped = new VmNet1445Slice.World { ClientTic = 53 };
        VmNet1445Slice.NetUpdate(capped, 100);
        Assert.Equal(34, capped.Built.Count);
        Assert.Equal(87, capped.ClientTic);

        var over = new VmNet1445Slice.World { ClientTic = 90 };
        VmNet1445Slice.NetUpdate(over, 10);
        Assert.Empty(over.Built);
        Assert.Equal(90, over.ClientTic);
        Assert.Equal(1, over.Starts);

        var remote = new VmNet1445Slice.World { NetGame = true, LevelStatus = 2, MapLoaded = true, Room = 4, NetGood = false };
        VmNet1445Slice.NetUpdate(remote, 3);
        Assert.Equal(1, remote.LiveControls);
        Assert.Equal(0, remote.Heartbeats);
        Assert.Equal(new byte[] { 0x10, 4 }, Assert.Single(remote.Sent).Bytes);
        Assert.Empty(remote.Built);
    }

    [Fact]
    public void CommandDupAndButtons()
    {
        var world = new VmNet1445Slice.World { LocalAuthority = true, Console = 0, SkipAmount = 2 };
        world.Script.Add(new VmNet1445Slice.Move { Buttons = VmNet1445Slice.Attack, Pitch = 4 });
        world.Script.Add(new VmNet1445Slice.Move { Buttons = VmNet1445Slice.Attack | VmNet1445Slice.Use, Pitch = 8 });
        VmNet1445Slice.NetUpdate(world, 4);
        Assert.Equal(0, world.SkipAmount);
        Assert.Equal(new[] { 0, 1 }, world.Built);
        Assert.Equal(new[] { 0 }, world.Attacks);
        Assert.Equal(new[] { 1 }, world.Uses);
        Assert.Same(world.Local[0], world.Peers[0].Commands[0]);
        Assert.Equal(2, world.NewTics);

        world.Reserved = 0;
        world.Script.Add(new VmNet1445Slice.Move { Buttons = VmNet1445Slice.Use });
        VmNet1445Slice.NetUpdate(world, 1);
        Assert.Null(world.Peers[0].Commands[2]);

        var doubled = new VmNet1445Slice.World { TicDup = 2, ClientTic = 0 };
        doubled.Script.Add(new VmNet1445Slice.Move { Buttons = 1, Pitch = 10, Yaw = 1 });
        doubled.Script.Add(new VmNet1445Slice.Move { Buttons = 2, Pitch = 21, Yaw = 5 });
        VmNet1445Slice.NetUpdate(doubled, 2);
        Assert.Equal(1, doubled.NewTics);
        Assert.Equal(3u, doubled.Local[0].Buttons);
        Assert.Equal(2u, doubled.Local[1].Buttons);
        Assert.Equal(15, doubled.Local[0].Pitch);
        Assert.Equal(15, doubled.Local[1].Pitch);
        Assert.Equal(3, doubled.Local[0].Yaw);
        Assert.Equal(3, doubled.Local[1].Yaw);

        var paused = new VmNet1445Slice.World { Pause = true };
        VmNet1445Slice.NetUpdate(paused, 3);
        Assert.Empty(paused.Built);
        Assert.Equal(0, paused.ClientTic);
        Assert.Equal(1, paused.Starts);
        Assert.Equal(1, paused.Events);
    }
}
