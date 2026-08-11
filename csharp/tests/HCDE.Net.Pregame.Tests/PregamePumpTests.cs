using System.Net;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame.Tests;

public class EngineInfoCodecTests
{
    [Fact]
    public void RoundTripWithNoWads()
    {
        var original = new EngineInfoSnapshot { Major = 5, Minor = 2, Revision = 1 };
        var buffer = new byte[32];
        var length = EngineInfoCodec.Write(buffer, original);
        Assert.Equal(EngineInfoCodec.HeaderSize, length);
        Assert.True(EngineInfoCodec.TryRead(buffer.AsSpan(0, length), out var parsed, out var bytesRead));
        Assert.Equal(length, bytesRead);
        Assert.True(EngineInfoCodec.Matches(original, parsed));
    }
}

public class ConnectPacketCodecTests
{
    [Fact]
    public void RoundTripIncludesHcd3Block()
    {
        var engine = new EngineInfoSnapshot();
        var buffer = new byte[128];
        var length = ConnectPacketCodec.Write(buffer, engine, "", HcdeConnectFlags.ServerAuthority);
        Assert.True(length > ConnectPacketCodec.MinimumSize);
        Assert.True(ConnectPacketCodec.TryRead(buffer.AsSpan(0, length), out var parsed));
        Assert.True(parsed.HasConnectInfo);
        Assert.Equal(HcdeConnectFlags.ServerAuthority, parsed.ConnectFlags);
    }
}

public class SessionTokenTests
{
    [Fact]
    public void NeverReturnsZero()
    {
        var endpoint = new NetworkEndpoint(IPAddress.Loopback, 5029);
        var token = SessionToken.Mint(endpoint, 1, new byte[8], 12345);
        Assert.NotEqual(0u, token);
    }

    [Fact]
    public void StableForSameInputs()
    {
        var endpoint = new NetworkEndpoint(IPAddress.Loopback, 5029);
        var gameId = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var a = SessionToken.Mint(endpoint, 1, gameId, 999);
        var b = SessionToken.Mint(endpoint, 1, gameId, 999);
        Assert.Equal(a, b);
    }
}

public class SetupPacketCompressionTests
{
    [Fact]
    public void DecodeCompressedSetupPacket()
    {
        var netBuffer = new byte[600];
        netBuffer[0] = (byte)NetCommandFlags.Setup;
        netBuffer[1] = (byte)PregameSetupType.ConnectAck;
        for (var i = 2; i < netBuffer.Length; i++)
            netBuffer[i] = (byte)(i & 0xFF);

        var wire = new byte[SetupPacketCodec.CrcPrefixSize + netBuffer.Length + 64];
        var encoded = SetupPacketCodec.EncodeCompressed(netBuffer, wire);
        Assert.True(encoded > SetupPacketCodec.CrcPrefixSize);
        Assert.NotEqual(0, wire[SetupPacketCodec.CrcPrefixSize] & (byte)NetCommandFlags.Compressed);

        var decoded = new byte[netBuffer.Length];
        var status = SetupPacketCodec.TryDecode(wire.AsSpan(0, encoded), decoded, out var length);
        Assert.Equal(SetupPacketDecodeStatus.Ok, status);
        Assert.Equal(netBuffer.Length, length);
        Assert.Equal(netBuffer, decoded.AsSpan(0, length).ToArray());
    }
}

public class PregameHostGuestLoopbackTests
{
    [Fact]
    public async Task GuestCompletesAdmissionHandshake()
    {
        using var hostTransport = new UdpTransport();
        hostTransport.Bind(0);
        hostTransport.SetNonBlocking(true);
        var hostEndpoint = new NetworkEndpoint(IPAddress.Loopback, hostTransport.BoundPort);

        var engineInfo = new EngineInfoSnapshot();
        var host = new PregameHost(hostTransport, new PregameHostOptions
        {
            MaxClients = 8,
            ExpectedEngineInfo = engineInfo,
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

            if (guest.Phase == PregameGuestPhase.Assigned)
                break;

            await Task.Delay(10);
        }

        Assert.Equal(PregameGuestPhase.Assigned, guest.Phase);
        Assert.NotEqual(0u, guest.Connection.SessionToken);
        Assert.True(guest.AssignedClientSlot >= 1);
        Assert.Equal(8, guest.MaxClients);
    }

    [Fact]
    public async Task GuestCompletesWaitingSetupHandshake()
    {
        using var hostTransport = new UdpTransport();
        hostTransport.Bind(0);
        hostTransport.SetNonBlocking(true);
        var hostEndpoint = new NetworkEndpoint(IPAddress.Loopback, hostTransport.BoundPort);

        var engineInfo = new EngineInfoSnapshot();
        var session = new PregameSessionSnapshot
        {
            MapLoad = new MapLoadInfo { MapName = "MAP01", RngSeed = 42 },
            GameInfo = new GameInfoPayload { TicDup = 1, GameId = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } },
            HostUserInfo = "name\\host",
        };

        var host = new PregameHost(hostTransport, new PregameHostOptions
        {
            MaxClients = 8,
            ExpectedEngineInfo = engineInfo,
            GameId = session.GameInfo.GameId,
            Session = session,
        });

        using var guestTransport = new UdpTransport();
        guestTransport.Bind(0);
        guestTransport.SetNonBlocking(true);

        var guest = new PregameGuest(guestTransport, new PregameGuestOptions
        {
            ServerAddress = hostEndpoint,
            EngineInfo = engineInfo,
            UserInfo = "name\\guest",
        });

        var deadline = Environment.TickCount64 + 5000;
        while (Environment.TickCount64 < deadline)
        {
            var now = (ulong)Environment.TickCount64;
            host.Pump(now);
            guest.Pump(now);

            if (guest.Phase == PregameGuestPhase.Ready
                && host.Clients.Any(c => c.ClientSlot == guest.AssignedClientSlot && c.Status == ConnectionStatus.Ready))
                break;

            await Task.Delay(10);
        }

        Assert.Equal(PregameGuestPhase.Ready, guest.Phase);
        Assert.NotNull(guest.ReceivedMapLoad);
        Assert.Equal("MAP01", guest.ReceivedMapLoad!.MapName);
        Assert.Equal(42, guest.ReceivedMapLoad.RngSeed);
        Assert.NotNull(guest.ReceivedGameInfo);
        Assert.Equal(session.GameInfo.GameId, guest.ReceivedGameInfo!.GameId);
        Assert.True(guest.ReceivedRoster.Count >= 1);
        Assert.Contains(host.Clients, c => c.Status == ConnectionStatus.Ready);
    }

    [Fact]
    public async Task HostRejectsMissingHcd3Block()
    {
        using var hostTransport = new UdpTransport();
        hostTransport.Bind(0);
        hostTransport.SetNonBlocking(true);
        var hostEndpoint = new NetworkEndpoint(IPAddress.Loopback, hostTransport.BoundPort);

        var host = new PregameHost(hostTransport, new PregameHostOptions
        {
            RequireHcdeConnectInfo = true,
        });

        using var guestTransport = new UdpTransport();
        guestTransport.Bind(0);
        guestTransport.SetNonBlocking(true);

        var netBuffer = new byte[64];
        netBuffer[0] = (byte)NetCommandFlags.Setup;
        netBuffer[1] = (byte)PregameSetupType.Connect;
        var engineLength = EngineInfoCodec.Write(netBuffer.AsSpan(2), new EngineInfoSnapshot());
        netBuffer[2 + engineLength] = 0;
        var packetLength = 2 + engineLength + 1;
        PregameWire.TrySend(guestTransport, netBuffer.AsSpan(0, packetLength), hostEndpoint);

        var guest = new PregameGuest(guestTransport, new PregameGuestOptions
        {
            ServerAddress = hostEndpoint,
        });

        var deadline = Environment.TickCount64 + 2000;
        PregameSetupType? reject = null;
        while (Environment.TickCount64 < deadline)
        {
            host.Pump((ulong)Environment.TickCount64);
            var status = PregameWire.TryReceive(guestTransport, netBuffer, out var length, out _, TimeSpan.FromMilliseconds(50));
            if (status == SetupPacketDecodeStatus.Ok && length >= 2)
                reject = (PregameSetupType)netBuffer[1];
            if (reject == PregameSetupType.ProtocolError)
                break;
            await Task.Delay(10);
        }

        Assert.Equal(PregameSetupType.ProtocolError, reject);
        guest.Pump((ulong)Environment.TickCount64);
        Assert.Equal(PregameGuestPhase.SentConnect, guest.Phase);
    }
}
