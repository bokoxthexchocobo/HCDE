namespace HCDE.Net.Core.Tests;

public class LiveAuthorityRoutingTests
{
    [Fact]
    public void AuthoritySendsControlAndSnapshotsToClients_NotSelf()
    {
        var authority = new LivePeerRoutingState(
            consolePlayer: 0,
            maxClients: 4,
            authoritySlot: 0,
            isLocalAuthority: true,
            usesHcdeService: true);

        Assert.False(authority.ShouldSendControlTo(0));
        Assert.True(authority.ShouldSendControlTo(1));
        Assert.True(authority.ShouldSendServerSnapshotTo(1));
        Assert.False(authority.ShouldSendServerSnapshotTo(0));
        Assert.True(authority.ShouldAcceptClientInputFrom(1));
    }

    [Fact]
    public void GuestSendsControlAndInputToAuthorityOnly()
    {
        var guest = new LivePeerRoutingState(
            consolePlayer: 1,
            maxClients: 4,
            authoritySlot: 0,
            isLocalAuthority: false,
            usesHcdeService: true);

        Assert.True(guest.ShouldSendControlTo(0));
        Assert.False(guest.ShouldSendControlTo(1));
        Assert.True(guest.ShouldSendClientInputTo(0));
        Assert.True(guest.ShouldAcceptServerSnapshotFrom(0));
        Assert.False(guest.ShouldAcceptServerSnapshotFrom(1));
    }

    [Fact]
    public void SetupInProgressPeerIsNotRoutableOnAuthority()
    {
        var authority = new LivePeerRoutingState(0, 4, 0, isLocalAuthority: true, usesHcdeService: true);
        Assert.False(authority.ShouldSendControlTo(1, client => client == 1));
        Assert.True(authority.ShouldSendControlTo(1, _ => false));
    }
}
