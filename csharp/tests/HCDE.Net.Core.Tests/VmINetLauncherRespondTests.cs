using System.Buffers.Binary;
using HCDE.Net.Transport;

namespace HCDE.Net.Core.Tests;

public class VmINetLauncherRespondTests
{
    [Fact]
    public void GetCachedServerQuerySnapshotNative_RefreshesAfterInterval()
    {
        var state = new ServerQuerySnapshotCacheState();
        var built = 0;
        _ = VmINetLauncherRespond.GetCachedServerQuerySnapshotNative(ref state, 500, () =>
        {
            built++;
            return new ServerQuerySnapshotBuildResult { HostName = $"h{built}" };
        });
        Assert.Equal(1, built);
        _ = VmINetLauncherRespond.GetCachedServerQuerySnapshotNative(ref state, 600, () =>
        {
            built++;
            return new ServerQuerySnapshotBuildResult { HostName = $"h{built}" };
        });
        Assert.Equal(1, built);
        _ = VmINetLauncherRespond.GetCachedServerQuerySnapshotNative(ref state, 800, () =>
        {
            built++;
            return new ServerQuerySnapshotBuildResult { HostName = $"h{built}" };
        });
        Assert.Equal(2, built);
        Assert.Equal("h2", state.CachedSnapshot.HostName);
    }

    [Fact]
    public void TryHandleServerQueryNative_MatchesLauncherChallenge()
    {
        var handled = VmINetLauncherRespond.TryHandleServerQueryNative(
            (uint)NetConstants.LauncherChallenge,
            msgSize: 4,
            connectedClients: 2,
            debugMark: null,
            sendLauncherInfo: () => true);
        Assert.True(handled);
    }

    [Fact]
    public void EncodeLauncherInfoNative_WritesChallengeHeader()
    {
        var snapshot = new ServerQuerySnapshotBuildResult { HostName = "srv", MapName = "m", SessionState = "idle" };
        Span<byte> buf = stackalloc byte[4096];
        Assert.True(VmINetLauncherRespond.EncodeLauncherInfoNative(
            snapshot, ReadOnlySpan<byte>.Empty, 0, buf, out var len,
            (uint)NetConstants.MsgChallenge, () => 42));
        Assert.Equal((uint)NetConstants.MsgChallenge, BinaryPrimitives.ReadUInt32BigEndian(buf));
        Assert.True(len > 8);
    }
}
