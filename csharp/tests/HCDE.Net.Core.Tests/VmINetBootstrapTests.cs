namespace HCDE.Net.Core.Tests;

public class VmINetBootstrapTests
{
    [Fact]
    public void ClampSvMaxPlayersNative_ClampsHighAndLow()
    {
        Assert.Equal(0, VmINetBootstrap.ClampSvMaxPlayersNative(-1, 31));
        Assert.Equal(31, VmINetBootstrap.ClampSvMaxPlayersNative(99, 31));
        Assert.Equal(8, VmINetBootstrap.ClampSvMaxPlayersNative(8, 31));
    }

    [Fact]
    public void MatchesHcdeConnectMagicNative_RecognizesTag()
    {
        ReadOnlySpan<byte> good = "HCD3"u8;
        ReadOnlySpan<byte> bad = "HCD2"u8;
        Assert.True(VmINetBootstrap.MatchesHcdeConnectMagicNative(good));
        Assert.False(VmINetBootstrap.MatchesHcdeConnectMagicNative(bad));
    }

    [Fact]
    public void ClearConnectionFieldsNative_ClearsServiceSlots()
    {
        var slots = new HcdePendingServiceSlot[2];
        slots[0].Active = true;
        slots[0].Sequence = 9;
        var state = new HcdeBootstrapConnectionState
        {
            Status = VmINetBootstrap.CstatReady,
            SessionToken = 0x11223344,
            PendingServices = slots,
        };
        VmINetBootstrap.ClearConnectionFieldsNative(ref state);
        Assert.Equal(VmINetBootstrap.CstatNone, state.Status);
        Assert.Equal(0u, state.SessionToken);
        Assert.False(slots[0].Active);
    }

    [Fact]
    public void ShouldUseCompressionNative_UsesThreshold()
    {
        Assert.False(VmINetBootstrap.ShouldUseCompressionNative(128));
        Assert.True(VmINetBootstrap.ShouldUseCompressionNative(512));
    }

    [Fact]
    public void ResetGlobalNetStateNative_RestoresDefaults()
    {
        var state = new HcdeGlobalNetState
        {
            Netgame = true,
            MySocket = 42,
            GamePort = 9999,
        };
        VmINetBootstrap.ResetGlobalNetStateNative(ref state);
        Assert.False(state.Netgame);
        Assert.Equal(-1, state.MySocket);
        Assert.Equal(VmINetBootstrap.DefaultGamePort, state.GamePort);
    }
}
