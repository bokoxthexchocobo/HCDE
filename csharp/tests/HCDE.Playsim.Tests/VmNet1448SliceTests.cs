using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1448SliceTests
{
    [Fact]
    public void StallWindowAndFreshLane()
    {
        var world = new VmNet1448Slice.World { GameTic = 10 };
        Assert.False(VmNet1448Slice.ShouldFire(world, -1, 1));
        Assert.False(VmNet1448Slice.ShouldFire(world, 1, -1));
        Assert.False(VmNet1448Slice.ShouldFire(world, 1, 5));
        Assert.Equal(5, world.Peers[1].LastAck);
        Assert.Equal(10, world.Peers[1].AckChanged);
        world.GameTic = 17;
        Assert.False(VmNet1448Slice.ShouldFire(world, 1, 5));
        world.GameTic = 18;
        Assert.True(VmNet1448Slice.ShouldFire(world, 1, 5));
        Assert.Equal(18, world.Peers[1].LastRequest);
        world.GameTic = 25;
        Assert.False(VmNet1448Slice.ShouldFire(world, 1, 5));
        world.GameTic = 26;
        Assert.True(VmNet1448Slice.ShouldFire(world, 1, 5));
        world.GameTic = 30;
        Assert.False(VmNet1448Slice.ShouldFire(world, 1, 6));
        Assert.Equal(0, world.Peers[1].LastRequest);
        Assert.Equal(30, world.Peers[1].AckChanged);

        world.Now = 100;
        world.GameTic = 4;
        world.ClientTic = 9;
        Assert.False(VmNet1448Slice.IsFresh(world, 2, VmNet1448Slice.Control, 0));
        Assert.Contains("with zero sequence", Assert.Single(world.Logs));
        Assert.Equal(0u, world.Peers[2].Duplicates);
        world.Peers[2].RxControl = 3;
        world.Peers[2].Rx = 8;
        world.Logs.Clear();
        Assert.False(VmNet1448Slice.IsFresh(world, 2, VmNet1448Slice.Control, 3));
        Assert.Equal(1u, world.Peers[2].Duplicates);
        Assert.Empty(world.Prints);
        world.Peers[2].RxSnapshot = 5;
        Assert.False(VmNet1448Slice.IsFresh(world, 2, VmNet1448Slice.ServerSnapshot, 5));
        Assert.Contains("type=server snapshot from=2 seq=5 last-type=5 last-any=8 duplicates=2 gametic=4 clienttic=9", Assert.Single(world.Prints));
        Assert.Equal(100UL, world.LastReject);
        world.Prints.Clear();
        Assert.False(VmNet1448Slice.IsFresh(world, 2, VmNet1448Slice.ServerSnapshot, 4));
        Assert.Empty(world.Prints);
        world.Now = 2100;
        world.Peers[2].RxCommand = 2;
        Assert.False(VmNet1448Slice.IsFresh(world, 2, VmNet1448Slice.ClientCommands, 1));
        Assert.Single(world.Prints);
        Assert.True(VmNet1448Slice.IsFresh(world, 2, VmNet1448Slice.ServerSnapshot, 6));
        VmNet1448Slice.Accept(world, 2, VmNet1448Slice.Control, 9);
        Assert.Equal(9u, world.Peers[2].RxControl);
        Assert.Equal(9u, world.Peers[2].Rx);
        VmNet1448Slice.Accept(world, 2, VmNet1448Slice.ClientCommands, 4);
        Assert.Equal(4u, world.Peers[2].RxCommand);
        Assert.Equal(9u, world.Peers[2].Rx);
        VmNet1448Slice.Accept(world, 2, 9, 12);
        Assert.Equal(12u, world.Peers[2].Rx);
        Assert.Equal(9u, world.Peers[2].RxControl);

        world.Peers[2].Rx = 12;
        var header = VmNet1448Slice.BeginPacket(world, 2, VmNet1448Slice.ServerSnapshot);
        Assert.Equal(new byte[] { 0, (byte)'H', (byte)'L', (byte)'I', (byte)'V', 1, 3, 0, 0, 0, 1, 0, 0, 0, 12 }, header);
        world.Peers[2].Tx = uint.MaxValue;
        VmNet1448Slice.BeginPacket(world, 2, VmNet1448Slice.Control);
        Assert.Equal(1u, world.Peers[2].Tx);
    }

    [Fact]
    public void ReplayPressureAndClear()
    {
        var world = new VmNet1448Slice.World { Room = 7 };
        VmNet1448Slice.ClearPressure(world, 1, null);
        Assert.Empty(world.Logs);
        world.Peers[1].Strikes = 2;
        world.Peers[1].Suppressed = 40;
        VmNet1448Slice.ClearPressure(world, 1, null);
        Assert.Contains("reason=unknown strikes=2 suppressed-until=40 room=7", Assert.Single(world.Logs));
        Assert.Equal(0u, world.Peers[1].Strikes);
        VmNet1448Slice.ClearPressure(world, -1, "x");
        Assert.Single(world.Logs);

        var kick = new VmNet1448Slice.World
        {
            LocalAuthority = true,
            AuthoritySlot = 0,
            Now = 1000,
            GameTic = 15,
            ClientTic = 16,
            Room = 3,
        };
        kick.Peers[2].Name = "ada";
        kick.Peers[2].Sequence = 8;
        kick.Peers[2].SequenceAck = 4;
        for (int i = 0; i < 5; i++)
            VmNet1448Slice.RequestReplay(kick, 2, "decode");
        Assert.Equal(5u, kick.Peers[2].Strikes);
        Assert.Equal(VmNet1448Slice.MissingSeq, kick.Peers[2].Flags);
        Assert.Equal(5, kick.Requests);
        VmNet1448Slice.RequestReplay(kick, 2, "decode");
        Assert.Equal(0u, kick.Peers[2].Strikes);
        Assert.Equal(VmNet1448Slice.Quit | VmNet1448Slice.MissingSeq, kick.Peers[2].Flags);
        Assert.Equal(1, kick.Escalations);
        Assert.Equal(1000UL, kick.Peers[2].Window);
        Assert.Contains("Disconnecting client 2 'ada'", Assert.Single(kick.Prints));
        Assert.Contains(kick.Logs, line => line.Contains("disconnecting replay-loop source client=2 name=ada reason=decode strikes=6 seq=8 ack=4"));

        var held = new VmNet1448Slice.World { LocalAuthority = true, AuthoritySlot = 0, Now = 2000, Room = 1 };
        held.Peers[0].Sequence = 3;
        held.Peers[0].SequenceAck = 1;
        for (int i = 0; i < 6; i++)
            VmNet1448Slice.RequestReplay(held, 0, null);
        Assert.Equal(6u, held.Peers[0].Strikes);
        Assert.Equal(2000UL + 5000UL, held.Peers[0].Suppressed);
        Assert.Equal(VmNet1448Slice.MissingSeq, held.Peers[0].Flags);
        Assert.Equal(1, held.Escalations);
        Assert.Contains(held.Logs, line => line.Contains("suppress-ms=5000 seq=3 ack=1"));
        Assert.Contains(held.Logs, line => line.Contains("requested HCDE live replay client=0 reason=unknown strikes=1 window-ms=0"));
        held.Logs.Clear();
        held.Now = 6999;
        VmNet1448Slice.RequestReplay(held, 0, null);
        Assert.Equal(1, held.Suppressions);
        Assert.Equal(6u, held.Peers[0].Strikes);
        Assert.Contains("suppress-ms-left=1", Assert.Single(held.Logs));
        held.Now = 7000;
        held.Logs.Clear();
        VmNet1448Slice.RequestReplay(held, 0, null);
        Assert.Equal(1, held.Suppressions);
        Assert.Equal(7u, held.Peers[0].Strikes);
        Assert.Equal(2, held.Escalations);

        var restarted = new VmNet1448Slice.World { Now = 1000 };
        restarted.Peers[4].Window = 8000;
        restarted.Peers[4].Strikes = 4;
        VmNet1448Slice.RequestReplay(restarted, 4, "late");
        Assert.Equal(1u, restarted.Peers[4].Strikes);
        Assert.Equal(1000UL, restarted.Peers[4].Window);

        var edge = new VmNet1448Slice.World { Now = 6000 };
        edge.Peers[5].Window = 1000;
        edge.Peers[5].Strikes = 2;
        VmNet1448Slice.RequestReplay(edge, 5, "edge");
        Assert.Equal(3u, edge.Peers[5].Strikes);
        edge.Now = 6001;
        VmNet1448Slice.RequestReplay(edge, 5, "edge");
        Assert.Equal(1u, edge.Peers[5].Strikes);
    }
}
