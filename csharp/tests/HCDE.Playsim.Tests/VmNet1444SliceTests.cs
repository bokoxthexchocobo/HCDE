using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1444SliceTests
{
    [Fact]
    public void MarkersAndStatusGates()
    {
        var world = new VmNet1444Slice.World { NetGame = true, Demo = true, MapLoaded = true, GameTic = 4 };
        world.Clients.Add(1);
        VmNet1444Slice.MakeConsistencies(world);
        Assert.Equal(0, world.CurrentConsistency);

        world.Demo = false;
        world.GameTic = 1;
        world.TicDup = 2;
        VmNet1444Slice.MakeConsistencies(world);
        Assert.Equal(0, world.CurrentConsistency);

        world.GameTic = 2;
        world.MapLoaded = false;
        VmNet1444Slice.MakeConsistencies(world);
        Assert.Equal(0, world.CurrentConsistency);

        world.MapLoaded = true;
        world.GameTic = 4;
        world.CurrentConsistency = 174;
        world.Clients.Add(2);
        VmNet1444Slice.MakeConsistencies(world);
        Assert.Equal((short)1, world.Peers[1].Local[174]);
        Assert.Equal((short)1, world.Peers[2].Local[174]);
        Assert.Equal(175, world.CurrentConsistency);
        VmNet1444Slice.MakeConsistencies(world);
        Assert.Equal((short)1, world.Peers[1].Local[0]);
        Assert.Equal(176, world.CurrentConsistency);

        var lone = new VmNet1444Slice.World { NetGame = true, LevelStatus = 2 };
        lone.Clients.Add(1);
        Assert.True(VmNet1444Slice.UpdateStatus(lone));

        var waiting = new VmNet1444Slice.World { NetGame = true, LevelStatus = 2 };
        waiting.Clients.AddRange(new[] { 0, 1 });
        Assert.False(VmNet1444Slice.UpdateStatus(waiting));

        var delayed = new VmNet1444Slice.World { NetGame = true, LevelDelay = 1 };
        delayed.Clients.AddRange(new[] { 0, 1 });
        Assert.False(VmNet1444Slice.UpdateStatus(delayed));

        var held = new VmNet1444Slice.World
        {
            NetGame = true,
            LocalAuthority = true,
            LastEnter = 50,
            LastGame = 0,
            Grace = 51,
            Enter = 40,
        };
        held.Clients.AddRange(new[] { 0, 1 });
        Assert.True(VmNet1444Slice.UpdateStatus(held));
        Assert.Equal(0, held.LastGame);

        var hiccup = new VmNet1444Slice.World
        {
            NetGame = true,
            LocalAuthority = true,
            LastEnter = 100,
            LastGame = 65,
            Enter = 110,
            GameTic = 70,
            Arbitrator = 0,
            LateJoin = 1UL << 3,
        };
        hiccup.Clients.AddRange(new[] { 0, 1, 2, 3 });
        hiccup.Peers[1].Sequence = 10;
        hiccup.Peers[2].Sequence = 80;
        hiccup.Peers[2].Waiting = true;
        hiccup.Peers[3].Sequence = 0;
        hiccup.Peers[3].Waiting = true;
        Assert.True(VmNet1444Slice.UpdateStatus(hiccup));
        Assert.Equal(110, hiccup.LastGame);
        Assert.Equal(VmNet1444Slice.Missing, hiccup.Peers[1].Flags);
        Assert.True(hiccup.Peers[1].Waiting);
        Assert.False(hiccup.Peers[2].Waiting);
        Assert.Equal(0, hiccup.Peers[3].Flags);
        Assert.True(hiccup.Peers[3].Waiting);

        var remote = new VmNet1444Slice.World
        {
            NetGame = true,
            LastEnter = 100,
            LastGame = 65,
            Enter = 110,
            GameTic = 4,
            ClientTic = 2,
            Room = 6,
            Map = "MAP02",
            NonPlayerAuthority = true,
        };
        remote.Clients.AddRange(new[] { 0, 1 });
        Assert.True(VmNet1444Slice.UpdateStatus(remote));
        Assert.Equal(VmNet1444Slice.Missing, remote.Peers[0].Flags);
        Assert.True(remote.AuthorityWaiting);
        Assert.False(remote.Peers[0].Waiting);
        Assert.Contains(remote.Logs, line => line.Contains("authority wait armed client=0") && line.Contains("map=MAP02"));

        var host = new VmNet1444Slice.World { NetGame = true, LocalAuthority = true, LevelStatus = 1 };
        host.Clients.AddRange(new[] { 0, 1 });
        Assert.True(VmNet1444Slice.UpdateStatus(host));
        Assert.Equal(0, host.LevelStatus);
        Assert.Equal(1, host.ReadyResets);
        Assert.Contains(host.Logs, line => line == "host -> ready reason=authority-host-gate-skip");
        host.Logs.Clear();
        Assert.True(VmNet1444Slice.UpdateStatus(host));
        Assert.Empty(host.Logs);
        Assert.Equal(1, host.ReadyResets);

        var blocked = new VmNet1444Slice.World { NetGame = true };
        blocked.Clients.AddRange(new[] { 0, 1 });
        blocked.Peers[1].Waiting = true;
        Assert.False(VmNet1444Slice.UpdateStatus(blocked));

        blocked.LateJoin = 1UL << 1;
        Assert.True(VmNet1444Slice.UpdateStatus(blocked));
    }

    [Fact]
    public void LeadSkipThrottle()
    {
        var dedicated = new VmNet1444Slice.World
        {
            NetGame = true,
            Dedicated = true,
            GameTic = 71,
            ClientTic = 90,
            DesiredLead = 100,
            Stability = 1,
            SkipTimer = 18,
        };
        dedicated.Clients.AddRange(new[] { 0, 1 });
        dedicated.Peers[0].Flags = VmNet1444Slice.Updated;
        Assert.True(VmNet1444Slice.UpdateStatus(dedicated));
        Assert.Equal(0, dedicated.Peers[0].Flags);
        Assert.Equal(0, dedicated.SkipTimer);
        Assert.Equal(4, dedicated.SkipAmount);

        var ahead = new VmNet1444Slice.World
        {
            NetGame = true,
            GameTic = 80,
            CommandsAhead = 3,
            Stability = 1,
            SkipTimer = 18,
            TicDup = 2,
        };
        ahead.Clients.AddRange(new[] { 0, 1 });
        ahead.Peers[0].Flags = VmNet1444Slice.Updated;
        Assert.True(VmNet1444Slice.UpdateStatus(ahead));
        Assert.Equal(4, ahead.SkipAmount);
        Assert.Equal(0, ahead.SkipTimer);

        ahead.SkipAmount = 5;
        ahead.SkipTimer = 18;
        ahead.Peers[0].Flags = VmNet1444Slice.Updated;
        VmNet1444Slice.UpdateStatus(ahead);
        Assert.Equal(5, ahead.SkipAmount);

        ahead.SkipAmount = 0;
        ahead.SkipTimer = 17;
        ahead.Peers[0].Flags = VmNet1444Slice.Updated;
        VmNet1444Slice.UpdateStatus(ahead);
        Assert.Equal(18, ahead.SkipTimer);
        Assert.Equal(0, ahead.SkipAmount);

        ahead.CommandsAhead = 1;
        ahead.Stability = 2;
        ahead.SkipTimer = 9;
        ahead.Peers[0].Flags = VmNet1444Slice.Updated;
        VmNet1444Slice.UpdateStatus(ahead);
        Assert.Equal(0, ahead.SkipTimer);

        var host = new VmNet1444Slice.World
        {
            NetGame = true,
            LocalAuthority = true,
            GameTic = 80,
            ClientTic = 100,
            SkipTimer = 4,
            SkipAmount = 0,
        };
        host.Clients.AddRange(new[] { 0, 1, 2 });
        host.Peers[1].Flags = VmNet1444Slice.Updated;
        host.Peers[1].Sequence = 10;
        host.Peers[2].Flags = VmNet1444Slice.Updated;
        host.Peers[2].Sequence = 90;
        Assert.True(VmNet1444Slice.UpdateStatus(host));
        Assert.Equal(0, host.SkipAmount);
        Assert.Equal(4, host.SkipTimer);
        Assert.Equal(0, host.Peers[1].Flags);
        Assert.Equal(0, host.Peers[2].Flags);
    }
}
