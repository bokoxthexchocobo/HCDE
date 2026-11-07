using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1449SliceTests
{
    [Fact]
    public void WrapAuthorityAndSettings()
    {
        var world = new VmNet1449Slice.World { NetGame = true, SendInput = { } };
        world.SendInput[2] = true;
        world.SendSnapshot[2] = true;
        Assert.True(VmNet1449Slice.WrapClient(world, 2));
        Assert.True(VmNet1449Slice.WrapSnapshot(world, 2));
        world.Demo = true;
        Assert.False(VmNet1449Slice.WrapClient(world, 2));
        world.Demo = false;
        world.NetGame = false;
        Assert.False(VmNet1449Slice.WrapSnapshot(world, 2));

        world.NetGame = true;
        world.LocalLiveAuthority = true;
        world.DedicatedSlot = true;
        Assert.True(VmNet1449Slice.InputAuthorized(world, 1, 1, 1));
        Assert.False(VmNet1449Slice.InputAuthorized(world, 1, 4, 2));
        Assert.Contains("reject-unauthorized-player player=4 count=2", Assert.Single(world.Logs));
        world.DedicatedSlot = false;
        Assert.True(VmNet1449Slice.InputAuthorized(world, 1, 4, 2));
        Assert.False(VmNet1449Slice.DeltasMutate(world));
        world.DedicatedSlot = true;
        Assert.True(VmNet1449Slice.DeltasMutate(world));

        world.DedicatedMode = true;
        world.AuthoritySlot = 1;
        world.Reserved[1] = true;
        world.ServerName = "svc";
        world.Peers[1].Name = "ada";
        Assert.Equal("svc", VmNet1449Slice.AuthorityName(world));
        VmNet1449Slice.SetSettings(world, true);
        Assert.False(world.Peers[1].Settings);
        Assert.Equal("settings True", Assert.Single(world.ServerCalls));
        VmNet1449Slice.SetWaiting(world, true);
        Assert.False(world.Peers[1].Waiting);
        Assert.Contains("waiting True", world.ServerCalls);

        world.Arbitrator = 1;
        world.ServerCalls.Clear();
        VmNet1449Slice.Initialize(world);
        Assert.Contains("state 1 False true false HCDE dedicated server", world.ServerCalls);
        Assert.Contains("settings True", world.ServerCalls);

        var local = new VmNet1449Slice.World();
        local.Peers[2].Settings = false;
        Assert.True(VmNet1449Slice.LocalCanControl(local));
        local.NetGame = true;
        local.Console = -1;
        Assert.False(VmNet1449Slice.LocalCanControl(local));
        local.DedicatedMode = true;
        local.LocalAuthority = true;
        local.ServerCanControl = true;
        Assert.True(VmNet1449Slice.LocalCanControl(local));
        local.DedicatedMode = false;
        local.Console = 2;
        local.Peers[2].Settings = true;
        Assert.True(VmNet1449Slice.LocalCanControl(local));
        Assert.Equal("client inputs", VmNet1449Slice.PayloadName(3));
        Assert.Equal("unknown", VmNet1449Slice.PayloadName(9));
    }

    [Fact]
    public void EnvelopePrefixStopsBeforeRoom()
    {
        var world = new VmNet1449Slice.World { Room = 4, GameTic = -3, OutgoingFlags = 1 };
        var data = new byte[12];
        VmNet1449Slice.WriteEnvelope(world, data, 4);
        Assert.Equal(new byte[] { (byte)'H', (byte)'G', (byte)'P', (byte)'L', 1, 4, 4, 1, 0, 0, 0, 0 }, data);
        Assert.Equal(0, world.OutgoingFlags);

        world.GameTic = 258;
        world.OutgoingFlags = 0;
        world.Room = 9;
        VmNet1449Slice.WriteEnvelope(world, data, VmNet1449Slice.ClientInputs);
        Assert.Equal(1, data[10]);
        Assert.Equal(2, data[11]);

        Array.Copy(data, 0, world.Buffer, VmNet1449Slice.LiveHeader, 12);
        world.Buffer[VmNet1449Slice.LiveHeader + 6] = 1;
        var opened = VmNet1449Slice.UnwrapPrefix(world, 3, 12, "inputs", VmNet1449Slice.ClientInputs);
        Assert.True(opened.Ok);
        Assert.Equal(258u, opened.RemoteTic);
        Assert.Equal(0, world.Peers[3].Unsupported);

        world.Buffer[VmNet1449Slice.LiveHeader + 7] = 1;
        opened = VmNet1449Slice.UnwrapPrefix(world, 3, 12, "inputs", VmNet1449Slice.ClientInputs);
        Assert.True(opened.Ok);
        world.Buffer[VmNet1449Slice.LiveHeader + 7] = 2;
        opened = VmNet1449Slice.UnwrapPrefix(world, 3, 12, "inputs", VmNet1449Slice.ClientInputs);
        Assert.False(opened.Ok);
        Assert.Contains(world.Logs, line => line.Contains("version=1 kind=client inputs flags=2"));

        opened = VmNet1449Slice.UnwrapPrefix(world, 3, 11, "inputs", 4);
        Assert.False(opened.Ok);
        Assert.Equal(0u, opened.RemoteTic);
        Assert.Contains(world.Logs, line => line.Contains("malformed HCDE live inputs payload from client=3 size=11"));
        opened = VmNet1449Slice.UnwrapPrefix(world, 3, 14001, "inputs", 4);
        Assert.False(opened.Ok);

        world.Buffer[VmNet1449Slice.LiveHeader] = (byte)'X';
        opened = VmNet1449Slice.UnwrapPrefix(world, 3, 20, "snap", 4);
        Assert.False(opened.Ok);
        Assert.Contains(world.Logs, line => line.Contains("without gameplay envelope from client=3"));

        world.Buffer[VmNet1449Slice.LiveHeader] = (byte)'H';
        world.Buffer[VmNet1449Slice.LiveHeader + 4] = 2;
        opened = VmNet1449Slice.UnwrapPrefix(world, 3, 20, "snap", 4);
        Assert.False(opened.Ok);
        Assert.Equal(258u, opened.RemoteTic);
        Assert.Contains(world.Logs, line => line.Contains("kind=client inputs flags=2"));
    }
}
