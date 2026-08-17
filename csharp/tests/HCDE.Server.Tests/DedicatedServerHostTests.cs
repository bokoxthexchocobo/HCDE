using HCDE.MapLoader.Tests;
using HCDE.Net.Core;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;
using HCDE.Protocol;
using System.Net;

namespace HCDE.Server.Tests;

public class DedicatedServerHostTests
{
    [Fact]
    public void Constructor_BindsUdpTransport()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        using var host = new DedicatedServerHost(new DedicatedServerOptions
        {
            Port = 0,
            IwadBytes = wad,
            Pregame = new PregameHostOptions
            {
                Session = new PregameSessionSnapshot
                {
                    MapLoad = new MapLoadInfo { MapName = "MAP01", RngSeed = 3 },
                },
            },
        });

        Assert.True(host.BoundPort > 0);
    }

    [Fact]
    public async Task Pump_BootstrapsLiveSessionAfterStartGameAck()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        var gameId = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2 };
        using var host = new DedicatedServerHost(new DedicatedServerOptions
        {
            Port = 0,
            IwadBytes = wad,
            Pregame = new PregameHostOptions
            {
                GameId = gameId,
                ExpectedEngineInfo = new EngineInfoSnapshot(),
                Session = new PregameSessionSnapshot
                {
                    MapLoad = new MapLoadInfo { MapName = "MAP01", RngSeed = 3 },
                    GameInfo = new GameInfoPayload { GameId = gameId },
                },
            },
        });

        var hostEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, host.BoundPort);
        using var guestTransport = new UdpTransport();
        guestTransport.Bind(0);
        guestTransport.SetNonBlocking(true);

        var guest = new PregameGuest(guestTransport, new PregameGuestOptions
        {
            ServerAddress = hostEndpoint,
            EngineInfo = new EngineInfoSnapshot(),
        });

        var deadline = Environment.TickCount64 + 5000;
        while (Environment.TickCount64 < deadline)
        {
            var now = (ulong)Environment.TickCount64;
            host.Pump(now);
            guest.Pump(now);

            if (guest.Phase == PregameGuestPhase.Ready
                && host.PregameHost.Clients.Any(c => c.ClientSlot == guest.AssignedClientSlot && c.Status == ConnectionStatus.Ready))
            {
                host.PregameHost.StartGame(now);
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
            if (host.PregameHost.AllReadyClientsAckedStartGame)
                break;

            await Task.Delay(10);
        }

        while (Environment.TickCount64 < deadline)
        {
            var now = (ulong)Environment.TickCount64;
            host.Pump(now);
            guest.Pump(now);
            if (host.LiveSession is not null)
                break;

            await Task.Delay(10);
        }

        Assert.NotNull(host.LiveSession);

        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);
        host.LiveSession!.TrackClient(guestEndpoint, guest.AssignedClientSlot);

        var guestStore = new GuestWorldStateStore();
        var guestChecksum = new SnapshotChecksumSession();
        var liveGuest = new LiveGuestSession(
            guestTransport,
            gameId,
            hostEndpoint,
            guestPlayerSlot: guest.AssignedClientSlot,
            authoritySlot: 0,
            maxClients: 8);
        liveGuest.SetGuestWorldState(guestStore, guestChecksum, rngSeed: 3);

        host.PregameHost.PumpLiveClients((ulong)Environment.TickCount64, host.LiveSession);
        Assert.True(liveGuest.TryReceiveAuthorityControl(out _));
        Assert.True(liveGuest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guestStore.Sectors.ContainsKey(0));
    }

    [Fact]
    public async Task Pump_RespondsToLauncherServerQuery()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        using var host = new DedicatedServerHost(new DedicatedServerOptions
        {
            Port = 0,
            IwadBytes = wad,
            ServerName = "Iter33 Query Host",
            Skill = 4,
            Deathmatch = true,
            GameMode = 2,
            GameModeName = "Invasion",
            Pregame = new PregameHostOptions
            {
                Session = new PregameSessionSnapshot
                {
                    MapLoad = new MapLoadInfo { MapName = "MAP01", RngSeed = 3 },
                },
            },
        });

        var hostEndpoint = new NetworkEndpoint(IPAddress.Loopback, host.BoundPort);
        var clientTask = Task.Run(async () =>
        {
            await Task.Delay(50);
            var client = new ServerQueryClient();
            return client.Query(new ServerQueryClientOptions
            {
                Address = hostEndpoint.ToString(),
                Timeout = TimeSpan.FromSeconds(2),
            });
        });

        var deadline = Environment.TickCount64 + 5000;
        while (Environment.TickCount64 < deadline)
        {
            host.Pump((ulong)Environment.TickCount64);
            if (clientTask.IsCompleted)
                break;

            await Task.Delay(10);
        }

        var snapshot = await clientTask;
        Assert.Equal("Iter33 Query Host", snapshot.HostName);
        Assert.Equal("MAP01", snapshot.MapName);
        Assert.Equal("waiting", snapshot.SessionState);
        Assert.Equal((byte)4, snapshot.Skill);
        Assert.True(snapshot.Deathmatch);
        Assert.Equal((byte)2, snapshot.GameMode);
        Assert.Equal("Invasion", snapshot.GameModeName);
    }

    [Fact]
    public void Advertiser_SendsHeartbeatToMaster()
    {
        using var master = new UdpTransport();
        master.Bind(0);
        master.SetNonBlocking(true);

        using var server = new UdpTransport();
        server.Bind(0);
        server.SetNonBlocking(true);

        var masterEndpoint = new NetworkEndpoint(IPAddress.Loopback, master.BoundPort);
        var advertiser = new DedicatedServerAdvertiser(server, masterEndpoint, (ushort)server.BoundPort, intervalSeconds: 1);
        advertiser.Pump(1000);

        var buffer = new byte[64];
        Assert.True(master.TryReceive(buffer, out var received, out var remote, TimeSpan.FromSeconds(2)));
        Assert.True(MasterPackets.TryReadServerHeartbeat(buffer.AsSpan(0, received), out var gamePort));
        Assert.Equal((ushort)server.BoundPort, gamePort);
        Assert.Equal(IPAddress.Loopback, remote.Address);
    }
}
