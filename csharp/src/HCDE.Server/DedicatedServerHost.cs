using System.Net;
using HCDE.Net.Core;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;
using HCDE.Protocol;

namespace HCDE.Server;

public sealed class DedicatedServerOptions
{
    public int Port { get; set; } = 10666;
    public string BindAddress { get; set; } = "0.0.0.0";
    public byte[] IwadBytes { get; set; } = Array.Empty<byte>();
    public bool ReplicateSectorMetadata { get; set; } = true;
    public PregameHostOptions Pregame { get; set; } = new();
    public bool EnableServerQuery { get; set; } = true;
    public bool EnableMasterAdvertise { get; set; }
    public string MasterHost { get; set; } = MasterProtocol.DefaultMasterHost;
    public ushort MasterPort { get; set; } = MasterProtocol.DefaultMasterPort;
    public string ServerName { get; set; } = "HCDE Server";
    public string VersionLabel { get; set; } = "hcdeserv-csharp";
    public string GitHash { get; set; } = "";
    public byte Skill { get; set; } = 3;
    public byte GameMode { get; set; }
    public string GameModeName { get; set; } = "Co-op";
    public bool Deathmatch { get; set; }
    public bool Teamplay { get; set; }
}

public sealed class DedicatedServerHost : IDisposable
{
    private readonly DedicatedServerOptions _options;
    private readonly UdpTransport _transport;
    private readonly PregameHost _pregameHost;
    private readonly DedicatedServerQueryResponder? _queryResponder;
    private readonly DedicatedServerAdvertiser? _advertiser;
    private LiveAuthoritySession? _liveSession;

    public DedicatedServerHost(DedicatedServerOptions options)
    {
        _options = options;
        _transport = new UdpTransport();
        _transport.Bind(options.Port);
        _transport.SetNonBlocking(true);

        if (options.EnableServerQuery)
        {
            _queryResponder = new DedicatedServerQueryResponder(_transport, BuildQuerySnapshot);
            options.Pregame.InboundInterceptor = _queryResponder;
        }

        _pregameHost = new PregameHost(_transport, options.Pregame);

        if (options.EnableMasterAdvertise)
        {
            var masterEndpoint = new NetworkEndpoint(IPAddress.Parse(options.MasterHost), options.MasterPort);
            _advertiser = new DedicatedServerAdvertiser(_transport, masterEndpoint, (ushort)_transport.BoundPort);
        }
    }

    public int BoundPort => _transport.BoundPort;

    public PregameHost PregameHost => _pregameHost;

    public LiveAuthoritySession? LiveSession => _liveSession;

    public void Pump(ulong nowMilliseconds)
    {
        _advertiser?.Pump(nowMilliseconds);
        _pregameHost.Pump(nowMilliseconds);
        if (_liveSession is null
            && _pregameHost.TryCreateBootstrappedLiveAuthoritySession(
                _options.IwadBytes,
                out var session,
                out _,
                _options.ReplicateSectorMetadata))
        {
            _liveSession = session;
        }

        if (_liveSession is not null)
            _pregameHost.PumpLiveClients(nowMilliseconds, _liveSession);
    }

    private ServerQuerySnapshot BuildQuerySnapshot()
    {
        var connectedClients = _pregameHost.Clients
            .Where(client => client.Status is ConnectionStatus.Connecting
                or ConnectionStatus.Waiting
                or ConnectionStatus.Ready)
            .ToArray();

        var snapshot = new ServerQuerySnapshot
        {
            HostName = _options.ServerName,
            MapName = _options.Pregame.Session.MapLoad.MapName,
            SessionState = _liveSession is not null ? "running" : "waiting",
            Version = _options.VersionLabel,
            GitHash = _options.GitHash,
            PlayerCount = (byte)Math.Min(connectedClients.Length, byte.MaxValue),
            MaxPlayers = (byte)Math.Min(_options.Pregame.MaxClients, byte.MaxValue),
            Skill = _options.Skill,
            Deathmatch = _options.Deathmatch,
            Teamplay = _options.Teamplay,
            GameName = "HCDE",
            GameMode = _options.GameMode,
            GameModeName = _options.GameModeName,
        };

        foreach (var client in connectedClients)
        {
            var name = string.IsNullOrWhiteSpace(client.UserInfo)
                ? $"Player{client.ClientSlot + 1}"
                : client.UserInfo;
            snapshot.Players.Add(new ServerQueryPlayer { Name = name });
        }

        return snapshot;
    }

    public void Dispose() => _transport.Dispose();
}
