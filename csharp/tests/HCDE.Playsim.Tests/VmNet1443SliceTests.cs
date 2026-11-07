using Xunit;

namespace HCDE.Playsim.Tests;

public sealed class VmNet1443SliceTests
{
    [Fact]
    public void HeartbeatAndDeadClients()
    {
        var world = new VmNet1443Slice.World
        {
            Console = 0,
            LastLatencyUpdate = 35,
            Now = 80,
        };
        world.Clients.AddRange(new[] { 0, 1, 2, 3 });
        world.Peers[1].Setup = true;
        var probe = world.Peers[2];
        probe.CurrentLatency = 35;
        probe.NewLatency = true;
        probe.Sent[0] = 100;
        probe.Recv[0] = 160;
        probe.Sent[1] = 100;
        probe.Recv[1] = 50;
        world.Peers[3].AverageLatency = 12;
        world.Peers[3].CurrentLatency = 4;

        VmNet1443Slice.Heartbeat(world);

        Assert.Equal(2, world.Sent.Count);
        Assert.Equal(2, world.Sent[0].Client);
        Assert.Equal(new byte[] { 0x01, 35 }, world.Sent[0].Bytes);
        Assert.Equal(3, world.Sent[1].Client);
        Assert.Equal(new byte[] { 0x01, 4 }, world.Sent[1].Bytes);
        Assert.Equal(60, probe.AverageLatency);
        Assert.False(probe.NewLatency);
        Assert.Equal(80ul, probe.Sent[0]);
        Assert.Equal(12, world.Peers[3].AverageLatency);

        var stalled = new VmNet1443Slice.World { Console = 0, LastLatencyUpdate = 34 };
        stalled.Clients.Add(1);
        stalled.Peers[1].AverageLatency = 7;
        stalled.Peers[1].Sent[0] = 10;
        stalled.Peers[1].Recv[0] = 20;
        VmNet1443Slice.Heartbeat(stalled);
        Assert.Equal(7, stalled.Peers[1].AverageLatency);
        Assert.Single(stalled.Sent);
        Assert.Equal(1, stalled.Sent[0].Client);
        Assert.Equal(new byte[] { 0x01, 0 }, stalled.Sent[0].Bytes);

        var clamped = new VmNet1443Slice.World { Console = -1, LastLatencyUpdate = 35 };
        clamped.Peers[1].CurrentLatency = 35;
        clamped.Clients.Add(1);
        clamped.Peers[1].Sent[0] = 10;
        clamped.Peers[1].Recv[0] = 9010;
        VmNet1443Slice.Heartbeat(clamped);
        Assert.Equal(5000, clamped.Peers[1].AverageLatency);

        var dead = new VmNet1443Slice.World
        {
            Arbitrator = 0,
            Reserved = 1,
            NetGame = true,
            LocalAuthority = true,
            Timeout = 1000,
            Now = 5000,
            GameTic = 9,
            ClientTic = 2,
            Room = 3,
        };
        dead.Clients.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        dead.Peers[2].InGame = false;
        dead.Peers[3].InGame = true;
        dead.Peers[3].Setup = true;
        dead.Peers[4].InGame = true;
        dead.Peers[5].InGame = true;
        dead.Peers[5].Name = "Ace";
        dead.Peers[5].LastPacket = 1000;
        dead.Peers[6].InGame = true;
        dead.Peers[6].LastPacket = 4500;
        dead.Peers[7].InGame = true;
        dead.Peers[7].LastPacket = 6000;
        VmNet1443Slice.CheckDead(dead);
        Assert.Equal(new[] { 5, 7 }, dead.Disconnects);
        Assert.Contains(dead.Logs, line => line.Contains("Client 5 'Ace' timed out after 1000 ms"));
        Assert.Equal(5000ul, dead.Peers[4].LastPacket);

        dead.LocalAuthority = false;
        dead.Disconnects.Clear();
        VmNet1443Slice.CheckDead(dead);
        Assert.Empty(dead.Disconnects);
    }

    [Fact]
    public void SeedsConsistencyAndDump()
    {
        var world = new VmNet1443Slice.World();
        world.Clients.AddRange(new[] { 1, 2 });
        world.Peers[1].CurrentNet = 4;
        world.Peers[2].CurrentNet = -1;
        VmNet1443Slice.CheckConsistencies(world);
        Assert.Equal(4, world.Peers[1].LastVerified);
        Assert.Equal(-1, world.Peers[2].LastVerified);

        world.SpawnSeed = 1;
        world.AcsSeed = 2;
        world.ChaseSeed = 3;
        world.DamageSeed = 4;
        Assert.Equal(10u, VmNet1443Slice.SumSeeds(world));
        Assert.False(VmNet1443Slice.UsesAuthoritative(world));
        world.NetGame = true;
        Assert.True(VmNet1443Slice.UsesAuthoritative(world));

        Assert.Equal((short)1, VmNet1443Slice.Consistency(world, 1, 0));
        Assert.Equal((short)9, VmNet1443Slice.Consistency(world, 1, 9));
        Assert.Equal((short)1, VmNet1443Slice.Consistency(world, 1, 0x10000));
        world.Peers[1].Actor = new VmNet1443Slice.Body { X = 1, Y = 1, Z = 1, Yaw = 2, Pitch = 3 };
        Assert.Equal((short)776, VmNet1443Slice.Consistency(world, 1, 0));
        world.Peers[1].Health = 1;
        Assert.Equal((short)777, VmNet1443Slice.Consistency(world, 1, 0));

        VmNet1443Slice.Dump(world, 1, 0, 3, 4);
        Assert.Equal(1, world.Dumps);
        Assert.Equal("", world.Saved);

        world.DesyncDebug = true;
        world.LocalAuthority = true;
        world.Arbitrator = 0;
        world.Console = 0;
        world.Map = "MAP01";
        world.Now = 42;
        world.AppData = "app";
        world.ClientTic = 1;
        world.Peers[1].Name = "Ace";
        world.Peers[1].CurrentSequence = 1;
        world.Peers[1].Commands[0].Buttons = 255;
        world.Peers[1].Local[0] = 8;
        world.Peers[1].Net[0] = 9;
        world.Local[1].Yaw = 6;
        world.Dumps = 0;
        VmNet1443Slice.Dump(world, 1, 0, 3, 4);

        Assert.Contains(world.Logs, line => line == "DESYNC REPORT BEGIN player=1 name=Ace consistency=0 local=3 net=4");
        Assert.Contains(world.Logs, line => line.Contains("sum=10"));
        Assert.Contains(world.Logs, line => line.Contains("pos=(1.000,1.000,1.000)"));
        Assert.Contains(world.Logs, line => line.Contains("con=0 local=8 net=9 marker=mismatch"));
        Assert.Contains(world.Logs, line => line.Contains("desync netcmd client=1 tic=0 buttons=0x000000ff"));
        Assert.Contains(world.Logs, line => line == "desync localcmd client=0 tic=1 buttons=0x00000000 pitch=0 yaw=6 roll=0 fwd=0 side=0 up=0");
        Assert.Equal("app/hcde-net-desync-42.txt", world.Saved);
        Assert.Contains(world.Logs, line => line == "DESYNC REPORT END");
        Assert.Equal(1, world.Dumps);
    }
}
