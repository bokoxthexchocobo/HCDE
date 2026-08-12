using HCDE.Net.Transport;

namespace HCDE.Net.Core.Tests;

public class LiveSessionTests
{
    [Fact]
    public void AuthorityAndGuestExchangeClientInputWithUserCmd()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);

        var now = (ulong)Environment.TickCount64;
        guest.Pump(now);
        authority.PumpClient(now + 1000, guestEndpoint, clientSlot: 1);

        Assert.True(authority.TryReceiveClientInput(guestEndpoint, out var header, out var players));
        Assert.Equal((byte)1, header.PlayerCount);
        Assert.Single(players);
        Assert.Equal((byte)1, players[0].PlayerNum);
    }

    [Fact]
    public void GuestReceivesServerSnapshotWithMinimalTail()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);

        var now = (ulong)Environment.TickCount64;
        authority.PumpClient(now, guestEndpoint, clientSlot: 1);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out var header, out var players, out var tailSections));
        Assert.Equal((byte)1, header.PlayerCount);
        Assert.Single(players);
        Assert.NotNull(tailSections);
        Assert.Equal(1u, tailSections.Value.WorldDelta.GameTic);
        Assert.Null(tailSections.Value.InvasionSnapshot);
        Assert.False(tailSections.Value.HasChecksum);
    }
}
