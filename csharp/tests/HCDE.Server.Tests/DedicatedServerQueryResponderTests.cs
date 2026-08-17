using System.Net;
using HCDE.Net.Transport;

namespace HCDE.Server.Tests;

public class DedicatedServerQueryResponderTests
{
    [Fact]
    public void TryHandle_RespondsToLauncherChallenge()
    {
        using var server = new UdpTransport();
        server.Bind(0);
        server.SetNonBlocking(true);
        var responder = new DedicatedServerQueryResponder(server, () => new ServerQuerySnapshot
        {
            HostName = "Query Responder",
            MapName = "MAP01",
            SessionState = "waiting",
            Version = "test",
            GitHash = "abc",
        });

        using var client = new UdpTransport();
        client.Bind(0);
        client.SetNonBlocking(true);
        var serverEndpoint = new NetworkEndpoint(IPAddress.Loopback, server.BoundPort);
        client.Send(ServerQueryCodec.CreateLauncherChallengeRequest(), serverEndpoint);

        var buffer = new byte[NetConstants.MaxMessageLength];
        Assert.True(server.TryReceive(buffer, out var received, out var remote, TimeSpan.FromSeconds(2)));
        Assert.True(responder.TryHandle(buffer.AsSpan(0, received), remote));
        Assert.True(client.TryReceive(buffer, out received, out _, TimeSpan.FromSeconds(2)));
        Assert.True(ServerQueryCodec.TryReadResponse(buffer.AsSpan(0, received), out var snapshot, out _));
        Assert.Equal("Query Responder", snapshot.HostName);
        Assert.Equal("MAP01", snapshot.MapName);
    }
}
