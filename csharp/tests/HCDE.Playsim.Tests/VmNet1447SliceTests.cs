using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1447SliceTests
{
    [Fact]
    public void AuthorityBodyQuittersAndCursor()
    {
        var world = new VmNet1447Slice.World
        {
            LocalAuthority = true,
            Console = 0,
            Newest = 10,
            EndSequence = 6,
            Players = 2,
            MaxCommands = 16,
            CurrentConsistency = 2,
            LastSent = 1,
        };
        world.Clients.AddRange(new[] { 0, 1, 2 });
        world.Quits.Add(2);
        world.Destinations.Add(1);
        world.Peers[1].SequenceNum = 5;
        world.Peers[1].NumTics = 1;
        world.Peers[1].Sequence = 12;
        world.Peers[1].Stability = 2;
        world.Peers[1].AverageLatency = 258;
        world.Peers[1].ResendSequence = 3;
        world.Peers[0].Consist[1] = 9;
        world.Peers[1].Consist[1] = 4;
        world.Peers[2].Flags = VmNet1447Slice.Quit;
        world.Peers[2].InGame = true;
        world.Peers[3].InGame = false;
        VmNet1447Slice.WriteBodies(world);

        var packet = Assert.Single(world.Sent);
        Assert.Equal("live", packet.Channel);
        Assert.Equal(new byte[]
        {
            8, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 2, 2, 1, 0, 0, 0, 5, 1, 0, 0, 0, 1, 4,
            0, 0, 0, 0, 0, 9, 0, 2,
            1, 1, 2, 0, 0, 4, 0, 2,
        }, packet.Bytes);
        Assert.Equal(4, world.Peers[1].ResendSequence);
        Assert.Equal(new[] { 2 }, world.Disconnects);
        Assert.Equal(2, world.LastSent);
        Assert.Equal(1, world.Packets);

        var split = new VmNet1447Slice.World
        {
            LocalAuthority = true,
            Console = 0,
            Players = 2,
            MaxCommands = 1,
            CurrentConsistency = 1,
            LastSent = 1,
            EndSequence = 9,
        };
        split.Headerless(1);
        split.Clients.Add(0);
        split.Peers[1].NumTics = 2;
        split.Peers[1].Header[0] = 0x40;
        split.Quits.Add(8);
        split.Peers[8].InGame = false;
        VmNet1447Slice.WriteBodies(split);
        Assert.Equal(2, split.Sent.Count);
        Assert.Equal(0x48, split.Sent[0].Bytes[0]);
        Assert.Equal(8, split.Sent[0].Bytes[11]);
        Assert.Equal(0x40, split.Sent[1].Bytes[0]);
        Assert.Empty(split.Disconnects);
        Assert.Equal(1, split.LastSent);
    }

    [Fact]
    public void ChannelsCursorAndHeldYaw()
    {
        var snapshot = new VmNet1447Slice.World
        {
            LocalAuthority = true,
            NativeSnapshot = true,
            Extra = true,
            Console = 0,
            Players = 1,
            CurrentConsistency = 3,
            LastSent = 3,
        };
        snapshot.Headerless(1);
        snapshot.Peers[1].NumTics = 1;
        snapshot.Peers[1].ResendConsistency = 1;
        VmNet1447Slice.WriteBodies(snapshot);
        Assert.Equal(2, snapshot.Sent.Count);
        Assert.Equal("snapshot", snapshot.Sent[0].Channel);
        Assert.Equal(snapshot.Sent[0].Bytes, snapshot.Sent[1].Bytes);
        Assert.Equal(-1, snapshot.Peers[1].ResendConsistency);
        Assert.Equal(3, snapshot.LastSent);

        var remote = new VmNet1447Slice.World
        {
            Console = 2,
            NativeClient = true,
            Extra = true,
            Players = 1,
            Stability = 3,
            CurrentConsistency = 4,
            LastSent = 4,
            ResendOnly = true,
        };
        remote.Headerless(0);
        remote.Peers[0].SequenceNum = 1;
        remote.Peers[0].NumTics = 1;
        remote.Local[0].Yaw = 5;
        remote.Local[1].Yaw = 5;
        VmNet1447Slice.WriteBodies(remote);
        Assert.Equal(2, remote.Sent.Count);
        Assert.All(remote.Sent, packet => Assert.Equal("client", packet.Channel));
        Assert.Equal(new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 0, 0, 0, 1, 0, 3,
            2, 0, 1, 4, 0, 5,
        }, remote.Sent[0].Bytes);
        Assert.Equal(4, remote.LastSent);

        var self = new VmNet1447Slice.World
        {
            LocalAuthority = true,
            Console = 0,
            Players = 1,
            MaxCommands = 1,
            EndSequence = 5,
            CurrentConsistency = 0,
        };
        self.Headerless(0);
        self.Peers[0].NumTics = 2;
        self.Peers[0].ResendSequence = 3;
        VmNet1447Slice.WriteBodies(self);
        var body = Assert.Single(self.Sent);
        Assert.Equal("live", body.Channel);
        Assert.Equal(2, body.Bytes[11]);
        Assert.Equal(-1, self.Peers[0].ResendSequence);
    }
}

static class Net1447Fixture
{
    public static void Headerless(this VmNet1447Slice.World world, int client)
    {
        world.Clients.Add(client);
        world.Destinations.Add(client);
    }
}
