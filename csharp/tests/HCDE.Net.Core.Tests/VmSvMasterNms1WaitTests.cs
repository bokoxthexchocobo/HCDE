namespace HCDE.Net.Core.Tests;

public class VmSvMasterNms1WaitTests
{
    [Fact]
    public void IsStaleNms1TokenErrorNative_DetectsStaleCodes()
    {
        Assert.True(VmSvMasterNms1Wait.IsStaleNms1TokenErrorNative(VmSvMasterNms1Wait.Nms1ErrorStaleEntry));
        Assert.True(VmSvMasterNms1Wait.IsStaleNms1TokenErrorNative(VmSvMasterNms1Wait.Nms1ErrorTokenInvalid));
        Assert.False(VmSvMasterNms1Wait.IsStaleNms1TokenErrorNative(0));
    }

    [Fact]
    public void SendNms1PacketNative_ReturnsFalseWhenSocketInvalid()
    {
        var sent = false;
        Assert.False(VmSvMasterNms1Wait.SendNms1PacketNative(
            masterSocket: -1,
            packetEmpty: false,
            packetData: new byte[] { 1 },
            packetSize: 1,
            label: "test",
            sendTo: (_, _) => { sent = true; return; },
            debugMark: null,
            printError: null));
        Assert.False(sent);
    }

    [Fact]
    public void WaitForNms1HeartbeatAckNative_ReturnsRefreshedOnOk()
    {
        var ctx = new Nms1WaitParseContext
        {
            MasterIp = "127.0.0.1",
            MaxPacketSize = 512,
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
            TryReadChallenge = (_, _, _) => Nms1ParseOutcome.Simple(VmSvMasterNms1Wait.Nms1ParseResult.Malformed),
            TryReadRegisterAck = (_, _, _) => Nms1ParseOutcome.Simple(VmSvMasterNms1Wait.Nms1ParseResult.Malformed),
            TryReadHeartbeatAck = (_, _, _) => Nms1ParseOutcome.HeartbeatOk(120),
            TryReadUnregisterAck = (_, _, _) => Nms1ParseOutcome.Simple(VmSvMasterNms1Wait.Nms1ParseResult.Malformed),
        };
        var result = VmSvMasterNms1Wait.WaitForNms1HeartbeatAckNative(ctx, 1, out var ttl);
        Assert.Equal(VmSvMasterNms1Wait.Nms1HeartbeatResult.Refreshed, result);
        Assert.Equal((ushort)120, ttl);
    }
}
