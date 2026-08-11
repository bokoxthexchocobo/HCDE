using HCDE.Net.Core;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame.Tests;

public class PregameLiveHandoffTests
{
    [Fact]
    public async Task HostWaitsForStartGameAckBeforeLiveSession()
    {
        using var hostTransport = new UdpTransport();
        hostTransport.Bind(0);
        hostTransport.SetNonBlocking(true);
        var hostEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, hostTransport.BoundPort);

        var gameId = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var engineInfo = new EngineInfoSnapshot();
        var host = new PregameHost(hostTransport, new PregameHostOptions
        {
            GameId = gameId,
            ExpectedEngineInfo = engineInfo,
            Session = new PregameSessionSnapshot
            {
                MapLoad = new MapLoadInfo { MapName = "MAP01" },
                GameInfo = new GameInfoPayload { GameId = gameId },
            },
        });

        using var guestTransport = new UdpTransport();
        guestTransport.Bind(0);
        guestTransport.SetNonBlocking(true);

        var guest = new PregameGuest(guestTransport, new PregameGuestOptions
        {
            ServerAddress = hostEndpoint,
            EngineInfo = engineInfo,
        });

        var deadline = Environment.TickCount64 + 5000;
        while (Environment.TickCount64 < deadline)
        {
            var now = (ulong)Environment.TickCount64;
            host.Pump(now);
            guest.Pump(now);

            if (guest.Phase == PregameGuestPhase.Ready
                && host.Clients.Any(c => c.ClientSlot == guest.AssignedClientSlot && c.Status == ConnectionStatus.Ready))
            {
                host.StartGame(now);
            }

            if (guest.Phase == PregameGuestPhase.Starting)
                break;

            await Task.Delay(10);
        }

        Assert.True(host.StartGameSent);
        Assert.Null(host.TryCreateLiveAuthoritySession());

        while (Environment.TickCount64 < deadline)
        {
            var now = (ulong)Environment.TickCount64;
            host.Pump(now);
            guest.Pump(now);
            if (host.AllReadyClientsAckedStartGame)
                break;
            await Task.Delay(10);
        }

        var session = host.TryCreateLiveAuthoritySession(authoritySlot: 0);
        Assert.NotNull(session);

        var client = host.Clients.First(c => c.ClientSlot == guest.AssignedClientSlot);
        host.PumpLiveClients((ulong)Environment.TickCount64, session!);
        Assert.True(client.HasStartGameAck);
    }
}
