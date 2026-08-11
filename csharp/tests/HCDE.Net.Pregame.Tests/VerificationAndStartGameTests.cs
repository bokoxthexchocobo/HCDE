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
