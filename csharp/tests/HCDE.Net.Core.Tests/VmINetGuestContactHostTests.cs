namespace HCDE.Net.Core.Tests;

public class VmINetGuestContactHostTests
{
    [Fact]
    public void GuestContactHostRejectionsNative_RejectsFull()
    {
        var err = "";
        var packets = 1;
        VmINetGuestContactReject.GuestContactHostRejectionsNative(new GuestContactHostContext
        {
            NetBuffer = [0x80, 2],
            NcmdSetup = 0x80,
            PreFull = 2,
            GetConnection = () => packets-- > 0 ? new HostGetConnectionResult(true, default) : default,
            FatalNetError = s => err = s,
        });
        Assert.Equal("The game is full", err);
    }

    [Fact]
    public void GuestContactHostRejectionsNative_AppliesGameInfoService()
    {
        var packets = 1;
        var hasGameInfo = false;
        var buffer = new byte[64];
        buffer[0] = 0x80;
        buffer[1] = 12;
        buffer[2] = 4;
        VmINetGuestContactReject.GuestContactHostRejectionsNative(new GuestContactHostContext
        {
            NetBuffer = buffer,
            NetBufferLength = 20,
            NcmdSetup = 0x80,
            PreHcdeService = 12,
            HcdeServiceHeaderSize = 3,
            ConsolePlayer = 0,
            IsHcdeConnect = () => true,
            GetConnection = () => packets-- > 0 ? new HostGetConnectionResult(true, (1u, 2)) : default,
            CheckHcdePregameService = (_, _, _) => true,
            HasGameInfo = _ => hasGameInfo,
            SetHasGameInfo = (_, _) => hasGameInfo = true,
            SetTicDup = _ => { },
            SetGameId = _ => { },
            ReadServerInfo = (_, _) => { },
            MaxTicDup = 4,
        });
        Assert.True(hasGameInfo);
    }

    [Fact]
    public void GuestContactHostRejectionsNative_DedicatedLateJoinRetry()
    {
        var packets = 1;
        var ctx = new GuestContactHostContext
        {
            NetBuffer = [0x80, 3],
            NcmdSetup = 0x80,
            PreInProgress = 3,
            GetConnection = () => packets-- > 0 ? new HostGetConnectionResult(true, default) : default,
        };
        VmINetGuestContactReject.GuestContactHostRejectionsNative(ctx);
        Assert.True(ctx.DedicatedLateJoinRetryPendingSend);
        Assert.True(ctx.DedicatedJoinMode);
        Assert.True(ctx.DedicatedLateJoinRetryAttempted);
    }

    [Fact]
    public void GuestContactHostServicesNative_IgnoresUnnegotiatedService()
    {
        var marked = false;
        VmINetGuestContactHost.GuestContactHostServicesNative(new GuestContactHostContext
        {
            NetBuffer = [0x80, 12, 1],
            PreHcdeService = 12,
            IsHcdeConnect = () => false,
            DebugMark = _ => marked = true,
        });
        Assert.True(marked);
    }

    [Fact]
    public void GameInfoPayloadSizes_AddsNullTerminatorSlot()
    {
        var (_, stream) = VmINetGuestContactHost.GameInfoPayloadSizes(100, 10, 200);
        Assert.Equal(91, stream);
    }

    [Fact]
    public void ValidateRosterClientId_RejectsInvalid()
    {
        Assert.False(VmINetGuestContactHost.ValidateRosterClientId(99, 8, 8));
        Assert.True(VmINetGuestContactHost.ValidateRosterClientId(3, 8, 8));
    }

    [Fact]
    public void RosterPacketLooksValid_AcceptsMinimalEntry()
    {
        var buffer = new byte[32];
        buffer[10] = 1;
        buffer[11] = 0;
        buffer[12] = 0;
        buffer[13] = 0;
        Assert.True(VmINetGuestContactHost.RosterPacketLooksValid(new GuestContactHostContext
        {
            NetBuffer = buffer,
            NetBufferLength = 15,
            HcdeServiceHeaderSize = 10,
            MaxClients = 8,
            MaxPlayers = 8,
        }, 10, VmINetGuestContactHost.GuestRosterAddrSize));
    }

    [Fact]
    public void GuestSetupWatchdog_FiresAfterTimeout()
    {
        var err = "";
        var ctx = new GuestContactHostContext
        {
            IsHcdeConnect = () => true,
            MsTime = () => 10_000,
            HostHcdeServiceRxSeq = 1,
            GuestLastSetupProgressTime = 0,
            GuestLastSetupRxSeq = 0,
            HcdeGuestSetupProgressTimeoutMs = 100,
            FatalNetError = s => err = s,
        };
        ctx.GuestLastSetupProgressTime = 9_800;
        ctx.GuestLastSetupRxSeq = 1;
        VmINetGuestContactHost.RunGuestSetupWatchdog(ctx);
        Assert.Contains("Timed out", err);
    }
}
