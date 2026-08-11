using HCDE.Net.Transport;

namespace HCDE.Net.Core.Tests;

public class LiveWireLoopbackTests
{
    [Fact]
    public void AuthorityAndGuestExchangeLiveControl()
    {
        var gameId = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveControlEndpoint(authorityTransport, gameId);
        var guest = new LiveControlEndpoint(guestTransport, gameId);

        Assert.True(authority.TrySendControl(
            guestEndpoint,
            new LiveControlBasePayload(gameTic: 1, consolePlayer: 0, maxClients: 2),
            new LiveControlCapabilities(LiveConstants.DefaultLocalCapabilities)));

        Assert.True(guest.TryReceiveControl(
            authorityEndpoint,
            out var basePayload,
            out var caps,
            out var negotiation));

        Assert.Equal(1u, basePayload.GameTic);
        Assert.NotNull(caps);
        Assert.NotNull(negotiation);
        Assert.Equal(LiveConstants.DefaultLocalCapabilities, negotiation!.Value.Negotiated);
    }

    [Fact]
    public void GuestSendsEmptyClientInputs_AuthorityReceivesHcIn()
    {
        var gameId = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guest = new LiveGameplayEndpoint(guestTransport, gameId);
        var authority = new LiveGameplayEndpoint(authorityTransport, gameId);

        Assert.True(guest.TrySendEmptyClientInputs(authorityEndpoint, roomId: 0, gameTic: 10));

        Assert.True(authority.TryReceiveGameplay(
            guestEndpoint,
            GameplayPayloadKind.ClientInputs,
            currentRoomId: 0,
            out var header,
            out var envelope,
            out var nativePayload));

        Assert.Equal(LiveMessageType.ClientCommands, header.MessageType);
        Assert.Equal(10u, envelope.GameTic);
        Assert.True(ClientInputHeader.LooksLikeHeader(nativePayload.Span));
    }
}
