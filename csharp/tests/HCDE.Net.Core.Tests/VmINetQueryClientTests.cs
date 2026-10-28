namespace HCDE.Net.Core.Tests;

public class VmINetQueryClientTests
{
    [Fact]
    public void QueryServerInfoNative_SucceedsWhenPipelineWorks()
    {
        var snapshot = new ServerQuerySnapshotDecodeTarget();
        var ok = VmINetQueryClient.QueryServerInfoNative(
            "127.0.0.1",
            snapshot,
            _ => { },
            (_, _) => true,
            null,
            _ => true,
            () => 1,
            _ => 64,
            (data, size, target, _) =>
            {
                target.HostName = "remote";
                return true;
            });
        Assert.True(ok);
        Assert.Equal("remote", snapshot.HostName);
    }

    [Fact]
    public void CloseNetworkNative_ClearsSessionFlags()
    {
        var socket = 7;
        var netgame = true;
        var bGameStarted = true;
        var dedicatedJoin = true;
        var retry = true;
        var pending = true;
        ulong progress = 9;
        uint rx = 3;
        var abort = true;
        VmINetSessionLifecycle.CloseNetworkNative(
            ref socket, ref netgame, null, _ => { }, ref bGameStarted, ref dedicatedJoin,
            ref retry, ref pending, ref progress, ref rx, ref abort, () => true, null);
        Assert.Equal(-1, socket);
        Assert.False(netgame);
        Assert.False(bGameStarted);
        Assert.False(dedicatedJoin);
        Assert.False(abort);
    }
}
