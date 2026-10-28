namespace HCDE.Net.Core.Tests;

public class VmSvMasterNms1LifecycleTests
{
    [Fact]
    public void TryRegisterNms1MasterNative_SkipsWhenCooldownActive()
    {
        var master = new MasterServerNms1State { LastRegisterAttempt = 1000 };
        var ctx = BuildRegisterContext(now: 1010, sendOk: true, challengeOk: true, registerOk: true);
        Assert.False(VmSvMasterNms1Lifecycle.TryRegisterNms1MasterNative(ctx, ref master, force: false));
        Assert.False(master.Registered);
    }

    [Fact]
    public void TryHeartbeatNms1MasterNative_NeedsRegisterWhenSocketInvalid()
    {
        var master = new MasterServerNms1State { Registered = true };
        var ctx = new TryHeartbeatNms1MasterContext
        {
            MasterSocket = -1,
            Unregister = new TryUnregisterNms1MasterContext
            {
                MasterSocket = -1,
                GetAdvertisedGamePort = () => 5029,
                Wait = MinimalWait(),
                WriteUnregisterRequest = (_, _, _) => new byte[] { 1 },
                SendPacket = (_, _) => true,
                DebugMark = _ => { },
                SetErrorText = (ref MasterServerNms1State _, string _) => { },
            },
            NowMs = () => 0,
            GetAdvertisedGamePort = () => 5029,
            GetLocalSnapshot = () => new MasterServerSnapshotParts(null, null, null, null, 0, 8),
            Wait = MinimalWait(),
            WriteHeartbeatRequest = (_, _, _, _, _) => new byte[] { 1 },
            SendPacket = (_, _) => true,
            DebugMark = _ => { },
            ClearError = (ref MasterServerNms1State _) => { },
            SetErrorText = (ref MasterServerNms1State _, string _) => { },
        };
        var result = VmSvMasterNms1Lifecycle.TryHeartbeatNms1MasterNative(ctx, ref master);
        Assert.Equal(VmSvMasterNms1Wait.Nms1HeartbeatResult.NeedsRegister, result);
    }

    [Fact]
    public void TryUnregisterNms1MasterNative_ClearsRegistrationAfterAck()
    {
        var master = new MasterServerNms1State
        {
            Registered = true,
            EntryToken = new byte[] { 9 },
            GamePort = 5029,
        };
        var ctx = new TryUnregisterNms1MasterContext
        {
            MasterSocket = 1,
            GetAdvertisedGamePort = () => 5029,
            Wait = MinimalWait(unregisterOk: true),
            WriteUnregisterRequest = (_, _, _) => new byte[] { 1 },
            SendPacket = (_, _) => true,
            DebugMark = _ => { },
            SetErrorText = (ref MasterServerNms1State _, string _) => { },
        };
        Assert.True(VmSvMasterNms1Lifecycle.TryUnregisterNms1MasterNative(ctx, ref master));
        Assert.False(master.Registered);
    }

    static TryRegisterNms1MasterContext BuildRegisterContext(ulong now, bool sendOk, bool challengeOk, bool registerOk)
    {
        return new TryRegisterNms1MasterContext
        {
            MasterSocket = 1,
            MasterIp = "127.0.0.1",
            NowMs = () => now,
            Wait = MinimalWait(challengeOk: challengeOk, registerOk: registerOk),
            GetLocalSnapshot = () => new MasterServerSnapshotParts("b", "d", "g", "m", 1, 8),
            GetAdvertisedGamePort = () => 5029,
            WriteChallengeRequest = _ => new byte[] { 1 },
            SendPacket = (_, _) => sendOk,
            DebugMark = _ => { },
            Print = _ => { },
            ClearError = (ref MasterServerNms1State _) => { },
            SetErrorText = (ref MasterServerNms1State _, string _) => { },
            WriteRegisterRequest = (_, _, _, _, _, _, _, _, _) => new byte[] { 2 },
        };
    }

    static Nms1WaitParseContext MinimalWait(bool challengeOk = false, bool registerOk = false, bool unregisterOk = false)
    {
        return new Nms1WaitParseContext
        {
            MasterIp = "127.0.0.1",
            MaxPacketSize = 256,
            NowMs = () => 0,
            DebugMark = _ => { },
            Print = null,
            SetError = (_, _, _) => { },
            Reply = new Nms1MasterReplyContext
            {
                MasterSocket = 1,
                NowMs = () => 0,
                IsWindows = true,
                ErrnoEintr = 4,
                ExpectedAddress = default,
                SelectRead = (_, _) => 1,
                RecvFrom = _ => (4, default),
                LastSocketError = () => 0,
                SameAddress = (_, _) => true,
                DebugMark = _ => { },
                PrintError = _ => { },
            },
            TryReadChallenge = (_, _, _) => challengeOk
                ? Nms1ParseOutcome.Ok(new byte[] { 7 }, 0)
                : Nms1ParseOutcome.Simple(VmSvMasterNms1Wait.Nms1ParseResult.Malformed),
            TryReadRegisterAck = (_, _, _) => registerOk
                ? Nms1ParseOutcome.Ok(new byte[] { 8 }, 90)
                : Nms1ParseOutcome.Simple(VmSvMasterNms1Wait.Nms1ParseResult.Malformed),
            TryReadHeartbeatAck = (_, _, _) => Nms1ParseOutcome.HeartbeatOk(60),
            TryReadUnregisterAck = (_, _, _) => unregisterOk
                ? Nms1ParseOutcome.Simple(VmSvMasterNms1Wait.Nms1ParseResult.Ok)
                : Nms1ParseOutcome.Simple(VmSvMasterNms1Wait.Nms1ParseResult.Malformed),
        };
    }
}
