using System.Net;
using HCDE.Net.Transport;

namespace HCDE.Net.Transport.Tests;

public class NetConstantsTests
{
    [Fact]
    public void ConstantsMatchCppHeader()
    {
        Assert.Equal(64, NetConstants.MaxPlayers);
        Assert.Equal(14000, NetConstants.MaxMessageLength);
        Assert.Equal(5029, NetConstants.DefaultGamePort);
        Assert.Equal(5560020, NetConstants.MsgChallenge);
        Assert.Equal(777123, NetConstants.LauncherChallenge);
        Assert.Equal(15, PregameConstants.ServiceHeaderSize);
    }
}

public class NetworkEndpointTests
{
    [Fact]
    public void ParseHostAndPort()
    {
        Assert.True(NetworkEndpoint.TryParse("127.0.0.1:10666", out var endpoint));
        Assert.Equal(IPAddress.Loopback, endpoint.Address);
        Assert.Equal(10666, endpoint.Port);
    }

    [Fact]
    public void ParseHostUsesDefaultPort()
    {
        Assert.True(NetworkEndpoint.TryParse("127.0.0.1", out var endpoint, 5029));
        Assert.Equal(5029, endpoint.Port);
    }
}

public class PregameServiceHeaderTests
{
    [Fact]
    public void RoundTrip()
    {
        var header = new PregameServiceHeader(0xAABBCCDD, (byte)PregameSetupType.HcdeService, 42, 17);
        var buffer = new byte[PregameConstants.ServiceHeaderSize];
        Assert.Equal(PregameConstants.ServiceHeaderSize, header.Write(buffer));
        Assert.True(PregameServiceHeader.TryRead(buffer, out var parsed));
        Assert.Equal(header.Crc, parsed.Crc);
        Assert.Equal(header.CommandByte, parsed.CommandByte);
        Assert.Equal(header.Sequence, parsed.Sequence);
        Assert.Equal(header.Acknowledgement, parsed.Acknowledgement);
    }
}

public class HcdeConnectInfoTests
{
    [Fact]
    public void RoundTrip()
    {
        Span<byte> buffer = stackalloc byte[HcdeConnectInfo.EncodedSize];
        Assert.Equal(HcdeConnectInfo.EncodedSize, HcdeConnectInfo.Write(buffer, PregameConstants.ConnectProtocolVersion, HcdeConnectFlags.ServerAuthority));
        Assert.True(HcdeConnectInfo.TryRead(buffer, out var version, out var flags));
        Assert.Equal(PregameConstants.ConnectProtocolVersion, version);
        Assert.Equal(HcdeConnectFlags.ServerAuthority, flags);
    }
}

public class ServerQueryCodecTests
{
    [Fact]
    public void LauncherChallengeRequestUsesBigEndianMarker()
    {
        var packet = ServerQueryCodec.CreateLauncherChallengeRequest();
        Assert.Equal([0x00, 0x0B, 0xDB, 0xA3], packet);
    }

    [Fact]
    public void ResponseRoundTrip()
    {
        var original = new ServerQuerySnapshot
        {
            HostName = "HCDE Test",
            MapName = "MAP01",
            SessionState = "waiting",
            Version = "1.0",
            GitHash = "abc123",
            PlayerCount = 1,
            MaxPlayers = 8,
            Skill = 3,
            Deathmatch = true,
            GameName = "Doom 2",
            GameMode = 2,
            GameModeName = "Co-op",
        };
        original.Players.Add(new ServerQueryPlayer { Name = "Player1", Ping = 12, Frags = 5 });

        var buffer = new byte[NetConstants.MaxMessageLength];
        Assert.True(ServerQueryCodec.TryWriteResponse(original, 0, buffer, out var length));
        Assert.True(ServerQueryCodec.TryReadResponse(buffer.AsSpan(0, length), out var parsed, out var error), error);
        Assert.Equal(original.HostName, parsed.HostName);
        Assert.Equal(original.MapName, parsed.MapName);
        Assert.Equal(original.Players[0].Name, parsed.Players[0].Name);
        Assert.Equal(original.GameModeName, parsed.GameModeName);
    }
}

public class ServerQueryIntegrationTests
{
    [Fact]
    public async Task ClientReceivesLoopbackQueryResponse()
    {
        using var server = new UdpTransport();
        server.Bind(0);
        server.SetNonBlocking(true);
        var serverEndpoint = new NetworkEndpoint(IPAddress.Loopback, server.BoundPort);

        var expected = new ServerQuerySnapshot
        {
            HostName = "Loopback Server",
            MapName = "START",
            SessionState = "running",
            Version = "test",
            GitHash = "deadbeef",
            MaxPlayers = 4,
        };

        var clientTask = Task.Run(async () =>
        {
            await Task.Delay(50);
            var client = new ServerQueryClient();
            return client.Query(new ServerQueryClientOptions
            {
                Address = serverEndpoint.ToString(),
                Timeout = TimeSpan.FromSeconds(2),
            });
        });

        var buffer = new byte[NetConstants.MaxMessageLength];
        while (true)
        {
            if (server.TryReceive(buffer, out var received, out var remote, TimeSpan.FromSeconds(2)))
            {
                Assert.Equal(ServerQueryCodec.CreateLauncherChallengeRequest(), buffer.AsSpan(0, received).ToArray());
                var response = new byte[NetConstants.MaxMessageLength];
                Assert.True(ServerQueryCodec.TryWriteResponse(expected, 0, response, out var length));
                server.Send(response.AsSpan(0, length), remote);
                break;
            }
        }

        var snapshot = await clientTask;
        Assert.Equal(expected.HostName, snapshot.HostName);
        Assert.Equal(expected.MapName, snapshot.MapName);
    }
}
