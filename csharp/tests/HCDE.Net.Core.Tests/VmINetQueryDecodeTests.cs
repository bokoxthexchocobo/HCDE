using HCDE.Net.Transport;

namespace HCDE.Net.Core.Tests;

public class VmINetQueryDecodeTests
{
    [Fact]
    public void TryReadServerQuerySnapshotNative_RoundTripsViaCodec()
    {
        var original = new HCDE.Net.Transport.ServerQuerySnapshot
        {
            HostName = "host",
            MapName = "MAP01",
            SessionState = "waiting",
            Version = "v",
            GitHash = "g",
            PlayerCount = 1,
            MaxPlayers = 8,
        };
        original.Players.Add(new HCDE.Net.Transport.ServerQueryPlayer { Name = "alice", Ping = 5, Frags = 1 });

        Span<byte> buf = stackalloc byte[4096];
        Assert.True(ServerQueryCodec.TryWriteResponse(original, 0, buf, out var len));

        var target = new ServerQuerySnapshotDecodeTarget();
        string? err = null;
        Assert.True(VmINetQueryDecode.TryReadServerQuerySnapshotNative(
            buf[..len], len, target, s => err = s), err);
        Assert.Equal("host", target.HostName);
        Assert.Equal("MAP01", target.MapName);
        Assert.Single(target.Players);
    }

    [Fact]
    public void TryReadServerQuerySnapshotNative_RejectsShortBuffer()
    {
        var target = new ServerQuerySnapshotDecodeTarget();
        string? err = null;
        Assert.False(VmINetQueryDecode.TryReadServerQuerySnapshotNative(
            [0, 1, 2], 3, target, s => err = s));
        Assert.Contains("too short", err);
    }
}
