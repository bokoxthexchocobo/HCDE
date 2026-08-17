using HCDE.MapLoader.Tests;
using HCDE.Net.Core;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame.Tests;

public class PregameMapLoadBootstrapTests
{
    [Fact]
    public async Task TryCreateBootstrappedLiveAuthoritySession_SeedsAuthorityFromSessionMap()
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
                MapLoad = new MapLoadInfo { MapName = "MAP01", RngSeed = 11 },
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

        while (Environment.TickCount64 < deadline)
        {
            var now = (ulong)Environment.TickCount64;
            host.Pump(now);
            guest.Pump(now);
            if (host.AllReadyClientsAckedStartGame)
                break;

            await Task.Delay(10);
        }

        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(host.TryCreateBootstrappedLiveAuthoritySession(
            wad,
            out var session,
            out _,
            replicateSectorMetadata: true));
        Assert.NotNull(session);

        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);
        session!.TrackClient(guestEndpoint, guest.AssignedClientSlot);

        var guestStore = new GuestWorldStateStore();
        var guestChecksum = new SnapshotChecksumSession();
        var liveGuest = new LiveGuestSession(
            guestTransport,
            gameId,
            hostEndpoint,
            guestPlayerSlot: guest.AssignedClientSlot,
            authoritySlot: 0,
            maxClients: 8);
        liveGuest.SetGuestWorldState(guestStore, guestChecksum, rngSeed: 11);

        host.PumpLiveClients((ulong)Environment.TickCount64, session);
        Assert.True(liveGuest.TryReceiveAuthorityControl(out _));
        Assert.True(liveGuest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.True(guestStore.Sectors.ContainsKey(0));
    }

    [Fact]
    public void TryCreateBootstrappedLiveAuthoritySession_RejectsBeforeStartGameAck()
    {
        using var hostTransport = new UdpTransport();
        hostTransport.Bind(0);
        var host = new PregameHost(hostTransport, new PregameHostOptions
        {
            Session = new PregameSessionSnapshot
            {
                MapLoad = new MapLoadInfo { MapName = "MAP01" },
            },
        });

        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.False(host.TryCreateBootstrappedLiveAuthoritySession(wad, out var session, out var reason));
        Assert.Null(session);
        Assert.Equal("start-game-not-ready", reason);
    }
}
