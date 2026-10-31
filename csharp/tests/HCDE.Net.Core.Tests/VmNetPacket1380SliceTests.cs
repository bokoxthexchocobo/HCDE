namespace HCDE.Net.Core.Tests;

public class VmNetPacket1380SliceTests
{
    [Fact]
    public void WaitingAndHeaderSizes()
    {
        Assert.Equal(2, VmNetPacket1380Slice.LstWaiting);
        Assert.Equal(0x80, VmNetPacket1380Slice.NcmdExit);
        var slice = new VmNetPacket1380Slice { HasPlayable = () => throw new InvalidOperationException("called") };
        slice.SetWaiting();
        Assert.Equal(9, slice.LevelStartDelay);

        slice.Netgame = true;
        slice.DemoPlayback = true;
        slice.SetWaiting();
        Assert.Equal(9u, slice.LevelStartAck);

        slice.DemoPlayback = false;
        slice.HasPlayable = () => false;
        slice.SetWaiting();
        Assert.Empty(slice.Lines);

        slice.HasPlayable = () => true;
        slice.ResetWatchdog = _ => { };
        slice.SetStatus = (_, _) => { };
        slice.RoomId = 4;
        slice.TransportClients = 3;
        slice.IsLocalAuthority = () => false;
        slice.PlayableCount = () => 2;
        slice.SetWaiting();
        Assert.Equal(0, slice.LevelStartDelay);
        Assert.Equal(0, slice.LevelStartDebug);
        Assert.Equal(0, slice.FullLatencyCycle);
        Assert.Equal(0u, slice.LevelStartAck);
        Assert.Equal(["level-wait"], slice.Watchdogs);
        Assert.Equal((2, "map-load-wait"), Assert.Single(slice.Statuses));
        Assert.Equal(
            "net.levelstart level start waiting room=4 map=<none> authority=0 transport-clients=3 playable-clients=2",
            Assert.Single(slice.Lines));

        slice.MapName = "MAP01";
        slice.IsLocalAuthority = () => true;
        slice.Lines.Clear();
        slice.SetWaiting();
        Assert.Contains("map=MAP01", slice.Lines[0], StringComparison.Ordinal);
        Assert.Contains("authority=1", slice.Lines[0], StringComparison.Ordinal);

        slice.NetBufferLength = 0;
        Assert.Equal(0, slice.GetNetBufferSize());
        slice.IsAuthoritySlot = client => client == 7;
        slice.RemoteClient = 7;
        slice.NetBuffer = [0x80];
        slice.NetBufferLength = 1;
        Assert.Equal(2, slice.GetNetBufferSize());
        slice.RemoteClient = 1;
        Assert.Equal(1, slice.GetNetBufferSize());

        slice.NetBuffer = [0x20];
        slice.NetBufferLength = 1;
        Assert.Equal(2, slice.GetNetBufferSize());
        slice.NetBuffer = [0x20, 1, 2, 3, 4];
        slice.NetBufferLength = 5;
        Assert.Equal(5, slice.GetNetBufferSize());

        slice.LivePacket = () => throw new InvalidOperationException("live");
        slice.NetBuffer = [0x01, 9, 9, 9];
        slice.NetBufferLength = 4;
        Assert.Equal(2, slice.GetNetBufferSize());
        slice.NetBuffer[0] = 0x03;
        Assert.Equal(2, slice.GetNetBufferSize());

        slice.LivePacket = () => true;
        slice.NetBuffer = [0x00, 1, 2];
        slice.NetBufferLength = 3;
        Assert.Equal(3, slice.GetNetBufferSize());

        slice.LivePacket = () => false;
        slice.NetBuffer = [0x10, 1];
        slice.NetBufferLength = 2;
        Assert.Equal(2, slice.GetNetBufferSize());
        slice.RemoteClient = 7;
        Assert.Equal(8, slice.GetNetBufferSize());

        slice.RemoteClient = 1;
        slice.NetBuffer = new byte[9];
        slice.NetBufferLength = 9;
        Assert.Equal(10, slice.GetNetBufferSize());

        slice.NetBuffer = new byte[12];
        slice.NetBuffer[0] = 0x08;
        slice.NetBuffer[10] = 1;
        slice.NetBufferLength = 11;
        Assert.Equal(12, slice.GetNetBufferSize());
    }

    [Fact]
    public void SendGetAndTicSkip()
    {
        var slice = new VmNetPacket1380Slice
        {
            DemoPlayback = true,
            Netgame = true,
            RemoteClient = 3,
            LocalNetBuffer = [9, 8],
            LocalNetBufferSize = 2,
            NetBuffer = new byte[4],
        };
        slice.SendPacket(1, 1);
        Assert.Equal(3, slice.RemoteClient);
        Assert.False(slice.GetPacket());
        Assert.Equal(2, slice.LocalNetBufferSize);

        slice.DemoPlayback = false;
        slice.Netgame = false;
        slice.ConsolePlayer = 4;
        slice.LocalNetBuffer = [0x11, 0x22];
        slice.LocalNetBufferSize = 2;
        slice.NetBuffer = new byte[2];
        Assert.True(slice.GetPacket());
        Assert.Equal(4, slice.RemoteClient);
        Assert.Equal(0, slice.LocalNetBufferSize);
        Assert.Equal(2, slice.NetBufferLength);
        Assert.Equal(0x11, slice.NetBuffer[0]);
        Assert.Empty(slice.Commands);

        slice.NetBufferLength = 0;
        slice.SendPacket(4, 2);
        Assert.Equal(2, slice.LocalNetBufferSize);
        Assert.Equal(0x11, slice.LocalNetBuffer[0]);
        Assert.Empty(slice.Commands);

        var offline = Assert.Throws<InvalidOperationException>(() => slice.SendPacket(1, 1));
        Assert.Equal("Tried to send a packet to a client while offline", offline.Message);

        var traced = new List<(int Client, byte Header, int Size)>();
        slice.Netgame = true;
        slice.TraceOutgoing = (client, header, size) => traced.Add((client, header, size));
        slice.NetCmd = _ => { };
        slice.NetBuffer = [0x20, 0x01];
        slice.SendPacket(1, 0);
        Assert.Empty(traced);
        Assert.Equal(VmNetPacket1380Slice.CmdSend, Assert.Single(slice.Commands));
        Assert.Equal(0, slice.NetBufferLength);

        slice.Commands.Clear();
        slice.SendPacket(1, 2);
        Assert.Equal((1, (byte)0x20, 2), Assert.Single(traced));
        Assert.Equal(VmNetPacket1380Slice.CmdSend, Assert.Single(slice.Commands));

        slice.Commands.Clear();
        Assert.Equal(2, slice.LocalNetBufferSize);
        Assert.True(slice.GetPacket());
        Assert.Equal(0, slice.LocalNetBufferSize);
        Assert.Empty(slice.Commands);
        slice.NetCmd = _ => slice.RemoteClient = -1;
        Assert.False(slice.GetPacket());
        Assert.Equal(VmNetPacket1380Slice.CmdGet, Assert.Single(slice.Commands));

        slice.NetCmd = _ =>
        {
            slice.RemoteClient = 2;
            slice.NetBuffer = [0x00];
            slice.NetBufferLength = 1;
        };
        slice.LivePacket = () => false;
        slice.IsAuthoritySlot = _ => false;
        Assert.False(slice.GetPacket());
        Assert.Equal("Incorrect packet size 1 (expected 10)\n", slice.Lines[^1]);

        var skips = new List<(bool Last, byte Tics, ulong Seen, byte Remaining, int Size)>();
        slice.SkipUserCmd = (byte[] data, int offset, int size, bool last, byte tics, ulong seen, byte remaining, out int payload) =>
        {
            skips.Add((last, tics, seen, remaining, size));
            payload = 1;
            return data[offset] == 0xAB;
        };
        var buffer = new byte[23];
        buffer[10] = 1;
        buffer[11] = 2;
        buffer[18] = 7;
        buffer[19] = 0;
        buffer[20] = 0xAB;
        buffer[21] = 1;
        buffer[22] = 0xAB;
        slice.NetBuffer = buffer;
        slice.NetBufferLength = 23;
        slice.NetCmd = _ => slice.RemoteClient = 2;
        Assert.True(slice.GetPacket());
        Assert.Equal(2, skips.Count);
        Assert.False(skips[0].Last);
        Assert.Equal((byte)2, skips[0].Tics);
        Assert.Equal(1ul, skips[0].Seen);
        Assert.Equal((byte)1, skips[0].Remaining);
        Assert.True(skips[1].Last);
        Assert.Equal(3ul, skips[1].Seen);
        Assert.Equal((byte)0, skips[1].Remaining);

        buffer[21] = 0;
        skips.Clear();
        Assert.False(slice.GetPacket());
        Assert.Equal("Incorrect packet size 23 (expected 24)\n", slice.Lines[^1]);
        Assert.Single(skips);
    }
}
