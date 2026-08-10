using System.Net;
using System.Net.Sockets;
using HCDE.Protocol;
using HCDE.Rcon;

namespace HCDE.Rcon.Tests;

/// <summary>
/// Minimal loopback RCON server that mirrors <c>src/d_net_rcon.cpp</c> framing and auth.
/// </summary>
public sealed class RconLoopbackServer : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly string _password;
    private readonly CancellationTokenSource _shutdown = new();
    private Task? _runTask;

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public RconLoopbackServer(string password)
    {
        _password = password;
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        _runTask = Task.Run(RunAsync);
    }

    public async ValueTask DisposeAsync()
    {
        _shutdown.Cancel();
        _listener.Stop();
        if (_runTask is not null)
            await _runTask.ConfigureAwait(false);
        _shutdown.Dispose();
    }

    private async Task RunAsync()
    {
        while (!_shutdown.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(_shutdown.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            _ = Task.Run(() => HandleClientAsync(client), _shutdown.Token);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            await using var stream = client.GetStream();

        var nonce = Guid.NewGuid().ToString("N")[..8];
        await RconProtocol.SendFrameAsync(stream, $"nonce {nonce}").ConfigureAwait(false);

        var authFrame = await RconProtocol.ReceiveFrameAsync(stream).ConfigureAwait(false);
        var expected = "auth " + RconProtocol.FormatAuthHash(RconProtocol.Fnv1aHash($"{nonce}:{_password}"));
        if (authFrame != expected)
        {
            await RconProtocol.SendFrameAsync(stream, "ERR auth failed").ConfigureAwait(false);
            return;
        }

        await RconProtocol.SendFrameAsync(stream, "OK authenticated").ConfigureAwait(false);

        var command = await RconProtocol.ReceiveFrameAsync(stream).ConfigureAwait(false);
        var response = command switch
        {
            "ping" => "OK pong",
            "status" => "OK rcon state=listening port=0 clients=1",
            _ => "ERR command not allowed",
        };
        await RconProtocol.SendFrameAsync(stream, response).ConfigureAwait(false);
        }
        finally
        {
            client.Dispose();
        }
    }
}

public class RconClientIntegrationTests
{
    [Fact]
    public async Task PingAgainstLoopbackServer()
    {
        await using var server = new RconLoopbackServer("secret");
        var client = new RconClient();
        var response = await client.ExecuteAsync(new RconClientOptions
        {
            Host = "127.0.0.1",
            Port = server.Port,
            Password = "secret",
            Command = "ping",
        });

        Assert.Equal("OK pong", response);
    }

    [Fact]
    public async Task StatusAgainstLoopbackServer()
    {
        await using var server = new RconLoopbackServer("secret");
        var client = new RconClient();
        var response = await client.ExecuteAsync(new RconClientOptions
        {
            Host = "127.0.0.1",
            Port = server.Port,
            Password = "secret",
            Command = "status",
        });

        Assert.StartsWith("OK rcon state=", response);
    }

    [Fact]
    public async Task BadPasswordFailsAuth()
    {
        await using var server = new RconLoopbackServer("secret");
        var client = new RconClient();

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ExecuteAsync(new RconClientOptions
        {
            Host = "127.0.0.1",
            Port = server.Port,
            Password = "wrong",
            Command = "ping",
        }));
    }
}
