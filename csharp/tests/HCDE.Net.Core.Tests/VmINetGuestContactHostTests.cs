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
}
