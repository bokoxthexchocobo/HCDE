using System.Net;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame.Tests;

public class BootstrapControlPayloadTests
{
    [Fact]
    public void RoundTrip()
    {
        var original = new BootstrapControlPayload(roomId: 3, gameTic: 120, clientTic: 115, consistency: 42);
        var buffer = new byte[PregameServicePayloads.BootstrapControlPayloadSize];
        Assert.Equal(PregameServicePayloads.BootstrapControlPayloadSize, PregameServicePayloads.WriteBootstrapControl(buffer, original));
        Assert.True(PregameServicePayloads.TryReadBootstrapControl(buffer, out var parsed));
        Assert.Equal(original.RoomId, parsed.RoomId);
        Assert.Equal(original.GameTic, parsed.GameTic);
        Assert.Equal(original.ClientTic, parsed.ClientTic);
        Assert.Equal(original.Consistency, parsed.Consistency);
    }
}

public class BootstrapResyncLoopbackTests
{
    [Fact]
    public async Task RuntimeJoinGuestReceivesBootstrapAndAcks()
    {
        using var hostTransport = new UdpTransport();
        hostTransport.Bind(0);
        hostTransport.SetNonBlocking(true);
        var hostEndpoint = new NetworkEndpoint(IPAddress.Loopback, hostTransport.BoundPort);

        var engineInfo = new EngineInfoSnapshot();
        var host = new PregameHost(hostTransport, new PregameHostOptions
        {
            MaxClients = 8,
            AdmitAsRuntimeJoin = true,
            ExpectedEngineInfo = engineInfo,
            Session = new PregameSessionSnapshot
            {
                RoomId = 7,
                AuthorityGameTic = 250,
                AuthorityClientTic = 240,
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
            guest.Pump(now);
            host.Pump(now);
            await Task.Delay(5);

            if (guest.ReceivedBootstrap is { } bootstrap)
            {
                Assert.Equal((byte)7, guest.AdoptedRoomId);
                Assert.Equal(250u, bootstrap.GameTic);
                Assert.Equal(240u, bootstrap.ClientTic);
                Assert.Equal(0u, bootstrap.Consistency);
                Assert.Contains(host.Clients, c => c.HasBootstrapAck);
                return;
            }
        }

        Assert.Fail("Timed out waiting for runtime bootstrap handshake");
    }

    [Fact]
    public async Task GuestResyncRequestTriggersResyncBegin()
    {
        using var hostTransport = new UdpTransport();
        hostTransport.Bind(0);
        hostTransport.SetNonBlocking(true);
        var hostEndpoint = new NetworkEndpoint(IPAddress.Loopback, hostTransport.BoundPort);

        var engineInfo = new EngineInfoSnapshot();
        var host = new PregameHost(hostTransport, new PregameHostOptions
        {
            MaxClients = 8,
            AdmitAsRuntimeJoin = true,
            ExpectedEngineInfo = engineInfo,
            Session = new PregameSessionSnapshot
            {
                RoomId = 2,
                AuthorityGameTic = 90,
                AuthorityClientTic = 88,
                Consistency = 55,
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

        var requestedResync = false;
        var deadline = Environment.TickCount64 + 5000;
        while (Environment.TickCount64 < deadline)
        {
            var now = (ulong)Environment.TickCount64;
            guest.Pump(now);
            host.Pump(now);
            await Task.Delay(5);

            if (guest.Phase >= PregameGuestPhase.Ready && !requestedResync)
            {
                Assert.True(guest.RequestResync());
                requestedResync = true;
            }

            if (guest.ReceivedResync is { } resync)
            {
                Assert.Equal((byte)2, guest.AdoptedRoomId);
                Assert.Equal(90u, resync.GameTic);
                Assert.Equal(88u, resync.ClientTic);
                Assert.Equal(55u, resync.Consistency);
                return;
            }
        }

        Assert.Fail("Timed out waiting for resync begin");
    }
}
