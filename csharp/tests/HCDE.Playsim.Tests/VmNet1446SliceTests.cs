using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1446SliceTests
{
    [Fact]
    public void WindowScaleAndDemo()
    {
        var demo = new VmNet1446Slice.World { Demo = true, TicDup = 2, ClientTic = 7 };
        demo.Clients.AddRange(new[] { 1, 4 });
        demo.Peers[1].Sequence = 9;
        VmNet1446Slice.SendWindow(demo);
        Assert.True(demo.Stopped);
        Assert.Equal(3, demo.Peers[1].Sequence);
        Assert.Equal(3, demo.Peers[4].Sequence);
        Assert.Empty(demo.Outbound);

        Assert.Equal(35, Scale(1).MaxCommands);
        Assert.Equal(16, Scale(2).MaxCommands);
        Assert.Equal(12, Scale(3).MaxCommands);
        Assert.Equal(8, Scale(4).MaxCommands);
        Assert.Equal(7, Scale(5).MaxCommands);
        Assert.Equal(4, Scale(8).MaxCommands);
        Assert.Equal(2, Scale(15).MaxCommands);
        Assert.Equal(1, Scale(15).PlayerLoops);
        Assert.Equal(2, Scale(16).MaxCommands);
        var wide = Scale(17);
        Assert.Equal(2, wide.MaxCommands);
        Assert.Equal(2, wide.PlayerLoops);
        Assert.Equal(17, wide.Players);

        var gated = new VmNet1446Slice.World
        {
            LocalAuthority = true,
            TicDup = 2,
            ClientTic = 5,
            GameTic = 4,
        };
        gated.Clients.AddRange(new[] { 0, 1, 2, 3, 4 });
        gated.Peers[2].Flags = VmNet1446Slice.Retransmit;
        gated.Peers[3].Flags = VmNet1446Slice.Missing;
        gated.Peers[4].Flags = VmNet1446Slice.Quit;
        VmNet1446Slice.SendWindow(gated);
        Assert.True(gated.ResendOnly);
        Assert.Equal(new[] { 4 }, gated.Quits);
        Assert.Equal(4, gated.Players);
        Assert.Equal(8, gated.MaxCommands);
        Assert.Equal(new[] { 2, 3 }, gated.Outbound.ConvertAll(item => item.Client));
    }

    [Fact]
    public void HeadersAndPassiveResend()
    {
        var world = new VmNet1446Slice.World
        {
            Arbitrator = 0,
            Console = 2,
            CommandStart = 4,
            ClientTic = 20,
            Room = 6,
        };
        world.Clients.AddRange(new[] { 0, 1, 2 });
        world.Peers[0].Flags = VmNet1446Slice.Missing | VmNet1446Slice.RetransmitSeq | VmNet1446Slice.RetransmitCon;
        world.Peers[0].Sequence = 100;
        world.Peers[0].Net = 7;
        world.Peers[0].SequenceAck = 3;
        world.Peers[0].ConsistencyAck = 9;
        world.Peers[0].ResendId = 9;
        world.Peers[1].Sequence = -1;
        world.Peers[1].Net = 1;
        world.Peers[2].Sequence = 50;
        world.Peers[2].Net = 4;
        world.Peers[2].SequenceAck = 19;
        VmNet1446Slice.SendWindow(world);

        var plan = Assert.Single(world.Outbound);
        Assert.Equal(0, plan.Client);
        Assert.Equal(new byte[] { 0x40, 9, 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 1 }, plan.Header);
        Assert.Equal(0, world.Peers[0].Flags);
        Assert.Equal(4, world.Peers[0].ResendSequence);
        Assert.Equal(10, world.Peers[0].ResendConsistency);
        Assert.Equal(4, plan.SequenceNum);
        Assert.Equal(16, plan.NumTics);
        Assert.Equal(0, world.ClientResends);

        var armed = new VmNet1446Slice.World
        {
            LocalAuthority = true,
            Console = 0,
            GameTic = 10,
            ClientTic = 12,
            Room = 3,
        };
        armed.Clients.AddRange(new[] { 0, 1 });
        armed.Peers[1].SequenceAck = 1;
        armed.Peers[1].Sequence = 8;
        armed.Peers[1].Fire = true;
        armed.Peers[1].ResendSequence = 8;
        VmNet1446Slice.SendWindow(armed);
        Assert.Equal(1, armed.AuthorityResends);
        Assert.Contains(armed.Logs, line => line == "passive authority resend client=1 from=2 start=10 end=12 seq=8 ack=1 room=3");
        Assert.Equal(8, armed.Outbound[1].SequenceNum);
        Assert.Equal(4, armed.Outbound[1].NumTics);

        var soft = new VmNet1446Slice.World { LocalAuthority = true, Console = 0, GameTic = 10, ClientTic = 10 };
        soft.Clients.Add(1);
        soft.Peers[1].SequenceAck = 6;
        soft.Peers[1].Fire = true;
        VmNet1446Slice.SendWindow(soft);
        Assert.Equal(0, soft.AuthorityResends);
        Assert.Equal(10, Assert.Single(soft.Outbound).SequenceNum);

        var remote = new VmNet1446Slice.World { Arbitrator = 0, Console = 2, ClientTic = 20, CommandStart = 0, Room = 5 };
        remote.Clients.AddRange(new[] { 0, 2 });
        remote.Peers[2].SequenceAck = 10;
        remote.Peers[2].Fire = true;
        VmNet1446Slice.SendWindow(remote);
        var sent = Assert.Single(remote.Outbound);
        Assert.Equal(11, sent.SequenceNum);
        Assert.Equal(9, sent.NumTics);
        Assert.Equal(1, remote.ClientResends);
        Assert.Contains(remote.Logs, line => line == "passive client resend to authority=0 from=11 newest=20 ack=10 lag=10 room=5");

        var clamped = new VmNet1446Slice.World { LocalAuthority = true, GameTic = 0, ClientTic = 100 };
        clamped.Clients.Add(1);
        VmNet1446Slice.SendWindow(clamped);
        Assert.Equal(35, Assert.Single(clamped.Outbound).NumTics);
    }

    static VmNet1446Slice.World Scale(int peers)
    {
        var world = new VmNet1446Slice.World { LocalAuthority = true, TicDup = 1, ClientTic = 4, GameTic = 4 };
        for (int i = 0; i < peers; i++)
            world.Clients.Add(i);
        VmNet1446Slice.SendWindow(world);
        return world;
    }
}
