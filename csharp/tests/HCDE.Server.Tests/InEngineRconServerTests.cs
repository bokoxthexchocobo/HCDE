using System.Net;
using System.Net.Sockets;
using HCDE.MapLoader;
using HCDE.Playsim;
using HCDE.Protocol;

namespace HCDE.Server.Tests;

public class InEngineRconServerTests
{
    [Fact]
    public async Task Allowlist_AcceptsPingStatusAndMap_DeniesExec()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP07",
            Things = new[] { new LevelThing { Type = 1, X = 1, Y = 2 } },
        });
        sim.Tick();
        await using var server = new InEngineRconServer("secret", sim);

        Assert.Equal("OK pong", await Command(server.Port, "ping"));
        var status = await Command(server.Port, "status");
        Assert.Contains("map=MAP07", status, StringComparison.Ordinal);
        Assert.StartsWith("OK ", status, StringComparison.Ordinal);
        Assert.Equal("OK map=MAP07", await Command(server.Port, "map"));
        Assert.Equal("ERR command not allowed", await Command(server.Port, "exec say hi"));
        Assert.Equal("ERR auth failed", await Command(server.Port, "ping", password: "wrong"));
    }

    private static async Task<string> Command(int port, string command, string password = "secret")
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        await using var stream = client.GetStream();
        var hello = await RconProtocol.ReceiveFrameAsync(stream);
        var nonce = hello["nonce ".Length..];
        var auth = RconProtocol.FormatAuthHash(RconProtocol.Fnv1aHash($"{nonce}:{password}"));
        await RconProtocol.SendFrameAsync(stream, $"auth {auth}");
        var authReply = await RconProtocol.ReceiveFrameAsync(stream);
        if (!authReply.StartsWith("OK", StringComparison.Ordinal))
            return authReply;
        await RconProtocol.SendFrameAsync(stream, command);
        return await RconProtocol.ReceiveFrameAsync(stream);
    }
}
