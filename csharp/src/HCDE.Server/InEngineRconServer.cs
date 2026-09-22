using System.Net;
using System.Net.Sockets;
using HCDE.Playsim;
using HCDE.Protocol;

namespace HCDE.Server;

/// <summary>
/// TCP RCON listener on the dedicated server. Auth matches <c>d_net_rcon.cpp</c>.
/// The allowlist is ping, status, and map. Everything else, including exec, is denied.
/// </summary>
public sealed class InEngineRconServer : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly string _password;
    private readonly AuthoritySimulation _simulation;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task _runTask;

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public InEngineRconServer(string password, AuthoritySimulation simulation, int port = 0)
    {
        _password = password;
        _simulation = simulation;
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
        _runTask = Task.Run(RunAsync);
    }

    public async ValueTask DisposeAsync()
    {
        _shutdown.Cancel();
        _listener.Stop();
        try
        {
            await _runTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

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
            catch (ObjectDisposedException)
            {
                break;
            }

            _ = Task.Run(() => HandleClientAsync(client), _shutdown.Token);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
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
                await RconProtocol.SendFrameAsync(stream, Dispatch(command)).ConfigureAwait(false);
            }
            catch (Exception) when (_shutdown.IsCancellationRequested)
            {
            }
            catch (IOException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    private string Dispatch(string command)
    {
        var verb = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
        return verb switch
        {
            "ping" => "OK pong",
            "status" => _simulation.StatusLine,
            "map" => $"OK map={_simulation.Level.MapName}",
            _ => "ERR command not allowed",
        };
    }
}
