using HCDE.Net.Pregame;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame.Tests;

public class VerificationErrorCodecTests
{
    [Fact]
    public void EngineMismatchRoundTrip()
    {
        var packet = new VerificationErrorPacket
        {
            Kind = VerificationErrorKind.Engine,
            HostMajor = 1,
            HostMinor = 2,
            HostRevision = 3,
            GuestMajor = 4,
            GuestMinor = 5,
            GuestRevision = 6,
        };
        var buffer = new byte[32];
        var length = VerificationErrorCodec.Write(buffer, packet);
        Assert.True(VerificationErrorCodec.TryRead(buffer.AsSpan(0, length), out var parsed));
        Assert.Equal(packet.Kind, parsed.Kind);
        Assert.Equal(1, parsed.HostMajor);
        Assert.Equal(6, parsed.GuestRevision);
    }

    [Fact]
    public void FileListRoundTrip()
    {
        var packet = new VerificationErrorPacket
        {
            Kind = VerificationErrorKind.FileMissing,
            Files = ["abc123", "def456"],
        };
        var buffer = new byte[128];
        var length = VerificationErrorCodec.Write(buffer, packet);
        Assert.True(VerificationErrorCodec.TryRead(buffer.AsSpan(0, length), out var parsed));
        Assert.Equal(packet.Files, parsed.Files);
    }

    [Fact]
    public void HostSendsVerificationErrorOnCrcMismatch()
    {
        using var hostTransport = new UdpTransport();
        hostTransport.Bind(0);
        hostTransport.SetNonBlocking(true);
        var hostEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, hostTransport.BoundPort);

        var host = new PregameHost(hostTransport, new PregameHostOptions
        {
            Session = new PregameSessionSnapshot { RequiredWadCrcs = ["expected-crc"] },
        });

        using var guestTransport = new UdpTransport();
        guestTransport.Bind(0);
        guestTransport.SetNonBlocking(true);

        var connect = new byte[128];
        var connectLength = ConnectPacketCodec.Write(
            connect,
            new EngineInfoSnapshot(),
            "",
            HcdeConnectFlags.ServerAuthority);
        PregameWire.TrySend(guestTransport, connect.AsSpan(0, connectLength), hostEndpoint);

        var guest = new PregameGuest(guestTransport, new PregameGuestOptions { ServerAddress = hostEndpoint });
        var deadline = Environment.TickCount64 + 2000;
        while (Environment.TickCount64 < deadline)
        {
            host.Pump((ulong)Environment.TickCount64);
            guest.Pump((ulong)Environment.TickCount64);
            if (guest.Phase == PregameGuestPhase.Rejected)
                break;
        }

        Assert.Equal(PregameGuestPhase.Rejected, guest.Phase);
        Assert.Equal(PregameSetupType.VerificationError, guest.RejectReason);
        Assert.NotNull(guest.VerificationError);
        Assert.Equal(VerificationErrorKind.FileMissing, guest.VerificationError!.Kind);
    }
}

public class StartGameServiceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DelayedConnectAckCannotRegressStartingGuestOrReplaceSession(bool differentToken)
    {
        using var host = new UdpTransport(); host.Bind(0); host.SetNonBlocking(true);
        using var transport = new UdpTransport(); transport.Bind(0); transport.SetNonBlocking(true);
        var endpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, transport.BoundPort);
        var guest = new PregameGuest(transport, new PregameGuestOptions
        { ServerAddress = new NetworkEndpoint(System.Net.IPAddress.Loopback, host.BoundPort) });
        var buffer = new byte[2048];
        void ConnectAck(uint token)
        {
            var length = ConnectAckPacket.Write(buffer, 1, 1, 8, token,
                PreConnectAckFlags.HcdeService, PregameConstants.ConnectProtocolVersion, HcdeConnectFlags.ServerAuthority);
            Assert.True(PregameWire.TrySend(host, buffer.AsSpan(0, length), endpoint));
        }
        void StartGame()
        {
            var length = HcdeServicePacket.Write(buffer, PregameServiceType.StartGame, 123, 1, 0, ReadOnlySpan<byte>.Empty);
            Assert.True(PregameWire.TrySend(host, buffer.AsSpan(0, length), endpoint));
        }
        void PumpUntil(Func<bool> condition)
        {
            var deadline = Environment.TickCount64 + 2000;
            do { guest.Pump((ulong)Environment.TickCount64); }
            while (!condition() && Environment.TickCount64 < deadline);
            Assert.True(condition());
        }
        ConnectAck(123);
        PumpUntil(() => guest.Phase == PregameGuestPhase.WaitingForAssignment);
        StartGame(); PumpUntil(() => guest.Phase == PregameGuestPhase.Starting);
        // Consume pending traffic, dropping the first StartGameAck.
        var receivedAck = false;
        while (PregameWire.TryReceive(host, buffer, out var length, out _, TimeSpan.FromMilliseconds(20)) == SetupPacketDecodeStatus.Ok)
            if (HcdeServicePacket.TryRead(buffer.AsSpan(0, length), out var service)
                && service.Service == PregameServiceType.StartGameAck) receivedAck = true;
        Assert.True(receivedAck);
        ConnectAck(differentToken ? 456u : 123u);
        // A following StartGame retry is rejected as a duplicate. It must not be
        // needed to restore the guest phase after a stale ConnectAck.
        StartGame();
        PumpUntil(() => guest.Connection.ServiceDuplicateCount > 0);
        Assert.Equal(123u, guest.Connection.SessionToken);
        Assert.Equal(PregameGuestPhase.Starting, guest.Phase);
        {
            Assert.Equal(SetupPacketDecodeStatus.Ok,
                PregameWire.TryReceive(host, buffer, out var length, out _, TimeSpan.FromSeconds(2)));
            Assert.True(HcdeServicePacket.TryRead(buffer.AsSpan(0, length), out var ack));
            Assert.Equal(PregameServiceType.StartGameAck, ack.Service);
            Assert.Equal(123u, ack.SessionToken);
        }
    }

    [Fact]
    public void ReadyClientRetransmitsDroppedStartGameWithoutInboundTraffic()
    {
        using var transport = new UdpTransport(); transport.Bind(0); transport.SetNonBlocking(true);
        using var guest = new UdpTransport(); guest.Bind(0); guest.SetNonBlocking(true);
        var host = new PregameHost(transport);
        var client = host.Clients[0];
        client.Address = new NetworkEndpoint(System.Net.IPAddress.Loopback, guest.BoundPort);
        client.Status = ConnectionStatus.Ready; client.Connection.SessionToken = 123;
        host.StartGame(1000);
        var buffer = new byte[2048];
        // Consume the first datagram without acknowledging it, simulating loss.
        Assert.True(guest.TryReceive(buffer, out _, out _, TimeSpan.FromSeconds(2)));
        var pending = Assert.Single(client.Sender.Queue.Pending, item => item.Active);
        Assert.Equal(1u, pending.SendCount);
        var retryTime = 1000UL + PregameConstants.ServiceResendMilliseconds;
        host.Pump(retryTime - 1); Assert.Equal(1u, pending.SendCount);
        host.Pump(retryTime); Assert.Equal(2u, pending.SendCount);
        Assert.Equal(SetupPacketDecodeStatus.Ok,
            PregameWire.TryReceive(guest, buffer, out var length, out _, TimeSpan.FromSeconds(2)));
        Assert.True(HcdeServicePacket.TryRead(buffer.AsSpan(0, length), out var packet));
        Assert.Equal(PregameServiceType.StartGame, packet.Service);
        Assert.False(host.AllReadyClientsAckedStartGame);
        // A real acknowledgement must still be received before live bootstrap.
        var ackLength = HcdeServicePacket.Write(buffer, PregameServiceType.StartGameAck,
            123, 1, pending.Sequence, ReadOnlySpan<byte>.Empty);
        Assert.True(PregameWire.TrySend(guest, buffer.AsSpan(0, ackLength),
            new NetworkEndpoint(System.Net.IPAddress.Loopback, transport.BoundPort)));
        var deadline = Environment.TickCount64 + 2000;
        while (!host.AllReadyClientsAckedStartGame && Environment.TickCount64 < deadline) host.Pump(retryTime + 1);
        Assert.True(host.AllReadyClientsAckedStartGame);
        Assert.False(client.Sender.Queue.HasPending());
    }

    [Fact]
    public async Task HostStartGamePromotesGuestToStarting()
    {
        using var hostTransport = new UdpTransport();
        hostTransport.Bind(0);
        hostTransport.SetNonBlocking(true);
        var hostEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, hostTransport.BoundPort);

        var engineInfo = new EngineInfoSnapshot();
        var host = new PregameHost(hostTransport, new PregameHostOptions
        {
            ExpectedEngineInfo = engineInfo,
            Session = new PregameSessionSnapshot
            {
                MapLoad = new MapLoadInfo { MapName = "MAP01" },
                GameInfo = new GameInfoPayload { GameId = new byte[8] },
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

        Assert.Equal(PregameGuestPhase.Starting, guest.Phase);
    }
}

public class CrossLanguageIntegrationTests
{
    [Fact]
    public void SkipsUnlessHcdeservConfigured()
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (string.IsNullOrWhiteSpace(serverPath) || string.IsNullOrWhiteSpace(iwadPath))
        {
            return;
        }

        Assert.True(File.Exists(serverPath));
        Assert.True(File.Exists(iwadPath));
    }
}
