using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmNet1441SliceTests
{
    [Fact]
    public void AuthorityHandoffAndMask()
    {
        Assert.Equal(5457, VmNet1441Slice.BandStart);
        Assert.Equal(5792, VmNet1441Slice.BandEnd);
        var world = new VmNet1441Slice.World { AuthorityName = "Red" };
        world.Clients.AddRange(new[] { 1, 2 });
        world.Peers[1].Latency = 40;
        world.Peers[2].Latency = 50;
        VmNet1441Slice.SetArbitrator(world, 3);
        Assert.Equal(3, world.Arbitrator);
        Assert.Equal(0, world.Peers[1].Latency);
        Assert.Equal(0, world.Peers[2].Latency);
        Assert.False(world.ResetCommands);
        Assert.Equal(1, world.Inits);
        Assert.Equal(1, world.WaitingSets);
        Assert.Equal("Red is the new authority\n", world.Logs[0]);

        world.Clients.Clear();
        world.Clients.AddRange(new[] { 0, 1, 2 });
        world.Reserved = 0;
        Assert.Equal(1, VmNet1441Slice.SelectNext(world, 0));
        world.Clients.Clear();
        world.Clients.AddRange(new[] { 1, 2 });
        world.Reserved = 2;
        Assert.Equal(2, VmNet1441Slice.SelectNext(world, 1));
        world.Clients.Clear();
        world.Clients.Add(5);
        world.Reserved = 5;
        world.Dedicated = true;
        Assert.Equal(5, VmNet1441Slice.SelectNext(world, 5));
        world.Dedicated = false;
        world.Reserved = -1;
        Assert.Equal(5, VmNet1441Slice.SelectNext(world, 5));

        world.Peers[2].InGame = false;
        VmNet1441Slice.ClientQuit(world, 2, 0);
        VmNet1441Slice.ClientQuit(world, -1, 0);
        Assert.Equal(0, world.Disconnects);
        world.Arbitrator = 0;
        world.LocalAuthority = false;
        world.GameTic = 8;
        world.ClientTic = 3;
        world.Room = 4;
        world.Peers[2].InGame = true;
        VmNet1441Slice.ClientQuit(world, 2, 0);
        Assert.Contains(world.Logs, line => line.Contains("Ignored unexpected disconnect packet from non-authority client 2 at gametic=8 clienttic=3 room=4"));
        Assert.Equal(0, world.Peers[2].Flags);
        Assert.Equal(0, world.Disconnects);
        world.LocalAuthority = true;
        world.Peers[2].Name = "Ace";
        VmNet1441Slice.ClientQuit(world, 2, 0);
        Assert.Equal(VmNet1441Slice.Quit, world.Peers[2].Flags);
        Assert.Contains("Client 2 'Ace' sent exit", world.Logs[^1]);

        var handoff = new VmNet1441Slice.World { Arbitrator = 4, Demo = true };
        handoff.Clients.AddRange(new[] { 4, 7 });
        handoff.Peers[4].InGame = true;
        handoff.Peers[7].InGame = true;
        handoff.Peers[4].Latency = 12;
        handoff.Reserved = 7;
        VmNet1441Slice.ClientQuit(handoff, 4, 7);
        Assert.Equal(7, handoff.Arbitrator);
        Assert.Equal(1, handoff.Disconnects);
        Assert.Equal(1, handoff.DemoChecks);
        Assert.Equal(0, handoff.Peers[4].Latency);
        handoff.Arbitrator = 4;
        handoff.Peers[6].InGame = true;
        VmNet1441Slice.ClientQuit(handoff, 4, 6);
        Assert.Equal(6, handoff.Arbitrator);

        var mask = new VmNet1441Slice.World { Arbitrator = 0 };
        mask.Clients.AddRange(new[] { 0, 1, 2, 3 });
        mask.Peers[2].Late = true;
        Assert.Equal(10ul, VmNet1441Slice.PlayableMask(mask));
        Assert.Equal(2, VmNet1441Slice.PlayableCount(mask));
        Assert.True(VmNet1441Slice.HasPlayable(mask));
        Assert.True(VmNet1441Slice.MapLoaded(mask));
        mask.GameState = 1;
        Assert.False(VmNet1441Slice.MapLoaded(mask));
        Assert.Equal(VmNet1441Slice.Ready, VmNet1441Slice.AfterRelease());
        mask.Clients.Clear();
        mask.Clients.Add(0);
        Assert.False(VmNet1441Slice.HasPlayable(mask));
    }

    [Fact]
    public void LevelStartReleaseAndSeat()
    {
        var world = new VmNet1441Slice.World
        {
            Arbitrator = 0,
            Status = VmNet1441Slice.Waiting,
            GameTic = 0x01020304,
            ClientTic = 6,
            Room = 9,
            Map = "E1M1",
            Ack = 6ul,
        };
        world.Clients.AddRange(new[] { 0, 1, 2 });
        world.Peers[1].Latency = 0;
        world.Peers[2].Latency = 350;
        Assert.False(VmNet1441Slice.TryRelease(new VmNet1441Slice.World { Arbitrator = 0 }));
        world.GameState = 1;
        Assert.False(VmNet1441Slice.TryRelease(world));
        Assert.Empty(world.Sent);
        world.GameState = VmNet1441Slice.Level;
        world.Ack = 2ul;
        Assert.False(VmNet1441Slice.TryRelease(world));
        world.Ack = 6ul;
        Assert.True(VmNet1441Slice.TryRelease(world));
        Assert.Equal(3, world.Sent.Count);
        Assert.Equal(new byte[] { 0x10, 9, 0, 0, 1, 2, 3, 4 }, world.Sent[0].Bytes);
        Assert.Equal(0, world.Sent[0].Client);
        Assert.Equal(new byte[] { 0x10, 9, 0, 12, 1, 2, 3, 4 }, world.Sent[1].Bytes);
        Assert.Equal(1, world.Sent[1].Client);
        Assert.Equal(new byte[] { 0x10, 9, 0, 0, 1, 2, 3, 4 }, world.Sent[2].Bytes);
        Assert.Equal(0ul, world.Ack);
        Assert.Equal(VmNet1441Slice.Ready, world.Status);
        Assert.Equal("try-release-level-start", world.StatusReason);
        Assert.Equal(0, world.Delay);
        Assert.Equal(0, world.DebugDelay);
        Assert.Equal("authority-release", world.Watchdogs[0]);
        Assert.Equal(1, world.JitterResets);
        Assert.Contains("map=E1M1 transport-clients=3 playable-clients=2", world.Logs[^1]);
        Assert.False(VmNet1441Slice.TryRelease(world));

        world.Status = VmNet1441Slice.Ready;
        world.LocalAuthority = true;
        world.Console = 0;
        world.Sent.Clear();
        world.GameTic = 8;
        world.Room = 2;
        VmNet1441Slice.CheckLevelStart(world, 0, 1);
        Assert.Empty(world.Sent);
        VmNet1441Slice.CheckLevelStart(world, 3, 1);
        Assert.Equal(new byte[] { 0x10, 2, 0, 0, 0, 0, 0, 8 }, world.Sent[0].Bytes);
        world.LocalAuthority = false;
        world.Sent.Clear();
        VmNet1441Slice.CheckLevelStart(world, 3, 1);
        Assert.Empty(world.Sent);

        var seat = new VmNet1441Slice.World
        {
            Arbitrator = 1,
            Status = VmNet1441Slice.Waiting,
            Dedicated = true,
            LocalAuthority = false,
            Console = 0,
            GameTic = 4,
            ClientTic = 1,
            DesiredLead = 2,
            TicDup = 1,
            Room = 3,
            Map = "MAP01",
            Delay = 4,
        };
        seat.Buffer[6] = 0x03;
        seat.Buffer[7] = 0xE8;
        seat.Peers[0].CurrentSequence = 3;
        seat.Peers[0].SequenceAck = 3;
        seat.Peers[0].AppliedSequence = 3;
        VmNet1441Slice.CheckLevelStart(seat, 1, 9);
        Assert.Equal(1000, seat.GameTic);
        Assert.Equal(1002, seat.ClientTic);
        Assert.Equal(1002, seat.Peers[0].CurrentSequence);
        Assert.Equal(1002, seat.Peers[0].SequenceAck);
        Assert.Equal(1000, seat.Peers[0].AppliedSequence);
        Assert.Equal(1, seat.Clears);
        Assert.Equal(9, seat.Delay);
        Assert.Equal(9, seat.DebugDelay);
        Assert.Equal(VmNet1441Slice.Ready, seat.Status);
        Assert.Equal("authority-start", seat.Watchdogs[0]);
        Assert.Contains("Aligning local gametic=4 to server gametic=1000", seat.Logs[0]);
        Assert.Contains("delay=9", seat.Logs[^1]);

        var clamp = new VmNet1441Slice.World
        {
            Arbitrator = 1,
            Status = VmNet1441Slice.Waiting,
            Dedicated = true,
            GameTic = 100,
            ClientTic = 500,
            DesiredLead = 20,
            Console = 0,
        };
        clamp.Buffer[7] = 100;
        clamp.Peers[0].CurrentSequence = 40;
        VmNet1441Slice.CheckLevelStart(clamp, 1, 0);
        Assert.Equal(108, clamp.ClientTic);
        Assert.Equal(40, clamp.Peers[0].AppliedSequence);
        Assert.Equal(1, clamp.Clears);
        Assert.Equal(100, clamp.GameTic);

        var local = new VmNet1441Slice.World
        {
            Arbitrator = 1,
            Status = VmNet1441Slice.Waiting,
            Dedicated = true,
            LocalAuthority = true,
            GameTic = 4,
            ClientTic = 50,
            Console = 0,
        };
        local.Buffer[7] = 20;
        VmNet1441Slice.CheckLevelStart(local, 1, 2);
        Assert.Equal(20, local.GameTic);
        Assert.Equal(50, local.ClientTic);
        Assert.Equal(0, local.Clears);

        var ack = new VmNet1441Slice.World
        {
            Arbitrator = 0,
            Status = VmNet1441Slice.Waiting,
            GameState = 1,
            Ack = 4ul,
        };
        ack.Clients.AddRange(new[] { 0, 1, 2 });
        VmNet1441Slice.CheckLevelStart(ack, 1, 0);
        Assert.Equal(6ul, ack.Ack);
        Assert.Equal(VmNet1441Slice.Waiting, ack.Status);
        Assert.Empty(ack.Sent);
        ack.GameState = VmNet1441Slice.Level;
        VmNet1441Slice.CheckLevelStart(ack, 1, 0);
        Assert.Equal(VmNet1441Slice.Ready, ack.Status);
        Assert.Equal(0ul, ack.Ack);
        Assert.Equal(3, ack.Sent.Count);
    }
}
