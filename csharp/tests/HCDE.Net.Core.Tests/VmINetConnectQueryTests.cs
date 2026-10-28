namespace HCDE.Net.Core.Tests;

public class VmINetConnectQueryTests
{
    [Fact]
    public void ReadHcdeConnectInfoNative_AcceptsMagic()
    {
        Span<byte> buf = [.. "HCD3"u8, 1, 2];
        Assert.True(VmINetConnectQuery.ReadHcdeConnectInfoNative(buf, buf.Length, 0, out var info));
        Assert.True(info.Present);
        Assert.Equal((byte)1, info.Version);
        Assert.Equal((byte)2, info.Flags);
    }

    [Fact]
    public void BuildLocalHcdeConnectFlagsNative_SetsDedicatedBits()
    {
        var flags = VmINetConnectQuery.BuildLocalHcdeConnectFlagsNative(dedicatedJoinMode: true, silentNetStartMode: false);
        Assert.Equal(
            VmINetConnectQuery.HcdeConnectDedicatedJoin
                | VmINetConnectQuery.HcdeConnectSuppressRoomUi
                | VmINetConnectQuery.HcdeConnectServerAuthority,
            flags);
    }

    [Fact]
    public void BuildServerQuerySnapshotNative_UsesMapFallbackChain()
    {
        var snap = VmINetConnectQuery.BuildServerQuerySnapshotNative(new ServerQuerySnapshotBuildContext
        {
            LevelMapName = "MAP01",
            MaxClients = 8,
            FirstPlayableClientSlot = 0,
            NetConnectFlow = VmINetConnectQuery.NetConnectFlowSyncing,
            VersionString = "1.0",
            GitHash = "abc",
            IsPlayerInGame = i => i == 1,
            GetPlayerName = i => $"P{i}",
            GetConnectedStatus = _ => 0,
            CstatNone = 0,
        });
        Assert.Equal("MAP01", snap.MapName);
        Assert.Equal("syncing", snap.SessionState);
        Assert.Single(snap.Players);
    }
}
