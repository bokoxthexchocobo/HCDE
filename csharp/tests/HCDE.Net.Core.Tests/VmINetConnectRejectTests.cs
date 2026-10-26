namespace HCDE.Net.Core.Tests;

public class VmINetConnectRejectTests
{
    [Fact]
    public void RejectConnectionNative_SetsSetupPacket()
    {
        var buf = new byte[8];
        var len = 0;
        VmINetConnectReject.RejectConnectionNative(0x80, 3, buf, ref len, () => { });
        Assert.Equal(2, len);
        Assert.Equal(0x80, buf[0]);
        Assert.Equal(3, buf[1]);
    }

    [Fact]
    public void SendVerificationErrorNative_EnginePayload()
    {
        var buf = new byte[16];
        var len = 0;
        var err = new VerificationErrorPayload(0, 2, 3, 4, 5, 6, 7, null);
        VmINetConnectReject.SendVerificationErrorNative(0x80, 9, 0, err, buf, ref len, 512, () => { }, null, null);
        Assert.Equal(9, len);
        Assert.Equal(2, buf[3]);
        Assert.Equal(7, buf[8]);
    }

    [Fact]
    public void SendVerificationErrorNative_FileList_PacksCountAndNames()
    {
        var buf = new byte[64];
        var len = 0;
        var err = new VerificationErrorPayload(
            VmINetConnectReject.VeFileMissing,
            0, 0, 0, 0, 0, 0,
            null,
            null,
            ["a.wad", "b.wad"]);
        VmINetConnectReject.SendVerificationErrorNative(0x80, 9, 0, err, buf, ref len, 512, () => { }, null, null);
        Assert.Equal(2, buf[2]);
        Assert.Equal(2, buf[6]);
        Assert.Equal((byte)'a', buf[7]);
    }

    [Fact]
    public void AddClientConnectionNative_DemotesReadyPeersWhenNotRuntimeJoin()
    {
        var demoted = new List<int>();
        VmINetConnectReject.AddClientConnectionNative(
            client: 2,
            runtimeJoin: false,
            connectInfo: new HcdeConnectInfoPacket { Present = true, Version = 1, Flags = 0 },
            maxClients: 4,
            peerAddress: 0x7F000001,
            peerPort: 1234,
            cstatReady: 3,
            cstatWaiting: 2,
            getClientStatus: i => i == 1 ? (byte)3 : (byte)0,
            makeSessionToken: null,
            isServerReservedSlot: null,
            playerHasLivePawnOrCamera: null,
            resetClientState: null,
            clearConnectedSlot: null,
            setClientStatus: (c, s) => { if (c == 1) demoted.Add(s); },
            setConnectedAddress: null,
            setSessionToken: null,
            setHcdeConnectFields: null,
            setRuntimeJoinFlag: null,
            addNetworkClient: null,
            setPlayeringame: null,
            setPlayerWaiting: null,
            setPlayerInconsistant: null,
            setSettingsController: null,
            setPlayerState: null,
            playerStateGone: 0,
            playerStateDead: 0,
            netClientUpdated: null,
            netLog: null,
            admissionPolicyBugLog: null);
        Assert.Equal(2, demoted[0]);
    }
}
