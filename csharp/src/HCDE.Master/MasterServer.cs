using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using HCDE.Protocol;

namespace HCDE.Master;

public sealed class MasterServerOptions
{
    public string BindAddress { get; set; } = "0.0.0.0";
    public ushort Port { get; set; } = MasterProtocol.DefaultMasterPort;
    public int TtlSeconds { get; set; } = MasterProtocol.DefaultEntryTtlSeconds;
    public int MaxPackets { get; set; }
    public bool Quiet { get; set; }
}

internal sealed record ServerEntry(IPAddress Address, ushort Port, DateTimeOffset LastSeen);

public sealed class MasterServer
{
    private readonly MasterServerOptions _options;
    private readonly ConcurrentDictionary<string, ServerEntry> _servers = new();
    private readonly CancellationTokenSource _shutdown = new();

    public int BoundPort { get; private set; }

    public MasterServer(MasterServerOptions options)
    {
        _options = options;
    }

    public void RequestStop() => _shutdown.Cancel();

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdown.Token);
        var token = linked.Token;

        using var client = new UdpClient(new IPEndPoint(ParseBindAddress(_options.BindAddress), _options.Port));
        BoundPort = ((IPEndPoint)client.Client.LocalEndPoint!).Port;

        if (!_options.Quiet)
        {
            Console.WriteLine(
                "HCDE master listening on {0}:{1} (ttl={2}s)",
                _options.BindAddress,
                BoundPort,
                _options.TtlSeconds);
        }

        var handledPackets = 0;
        var lastPrune = DateTimeOffset.UtcNow;

        while (!token.IsCancellationRequested)
        {
            if (_options.MaxPackets > 0 && handledPackets >= _options.MaxPackets)
                break;

            if (DateTimeOffset.UtcNow - lastPrune >= TimeSpan.FromSeconds(60))
            {
                PruneExpired();
                lastPrune = DateTimeOffset.UtcNow;
            }

            try
            {
                var receiveTask = client.ReceiveAsync(token).AsTask();
                var completed = await Task.WhenAny(receiveTask, Task.Delay(1000, token)).ConfigureAwait(false);
                if (completed != receiveTask)
                    continue;

                var result = await receiveTask.ConfigureAwait(false);
                var packet = result.Buffer;
                if (packet.Length < 4)
                    continue;

                var remote = result.RemoteEndPoint;
                if (TryReadServerHeartbeat(packet, remote.Address, out var key))
                {
                    handledPackets++;
                    if (!_options.Quiet)
                        Console.WriteLine("heartbeat {0} ({1} active)", key, _servers.Count);
                }
                else if (MasterPackets.TryReadLauncherListQuery(packet))
                {
                    PruneExpired();
                    lastPrune = DateTimeOffset.UtcNow;
                    var response = MasterPackets.CreateMasterListResponse(GetActiveServers());
                    await client.SendAsync(response, response.Length, remote).ConfigureAwait(false);
                    handledPackets++;
                    if (!_options.Quiet)
                        Console.WriteLine("query {0} -> {1} server(s)", remote.Address, _servers.Count);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (SocketException ex) when (token.IsCancellationRequested)
            {
                Console.Error.WriteLine("socket error during shutdown: {0}", ex.Message);
                break;
            }
        }
    }

    private bool TryReadServerHeartbeat(ReadOnlySpan<byte> packet, IPAddress address, out string key)
    {
        key = string.Empty;
        if (!MasterPackets.TryReadServerHeartbeat(packet, out var gamePort))
            return false;

        key = ServerKey(address, gamePort);
        _servers[key] = new ServerEntry(address, gamePort, DateTimeOffset.UtcNow);
        return true;
    }

    private void PruneExpired()
    {
        var cutoff = DateTimeOffset.UtcNow.AddSeconds(-_options.TtlSeconds);
        foreach (var pair in _servers)
        {
            if (pair.Value.LastSeen < cutoff)
                _servers.TryRemove(pair.Key, out _);
        }
    }

    private IReadOnlyList<MasterServerEntry> GetActiveServers()
    {
        return _servers.Values
            .Select(entry => new MasterServerEntry(entry.Address, entry.Port))
            .ToList();
    }

    private static string ServerKey(IPAddress address, ushort port) => $"{address}:{port}";

    private static IPAddress ParseBindAddress(string bindAddress)
    {
        if (bindAddress == "0.0.0.0")
            return IPAddress.Any;

        return IPAddress.Parse(bindAddress);
    }
}
