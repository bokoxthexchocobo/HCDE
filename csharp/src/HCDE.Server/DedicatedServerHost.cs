using System.Net;
using HCDE.MapLoader;
using HCDE.Net.Core;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;
using HCDE.Playsim;
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
    public string RconPassword { get; set; } = "";
    public int RconPort { get; set; }
}

public sealed class DedicatedServerHost : IDisposable
{
    private readonly DedicatedServerOptions _options;
    private readonly UdpTransport _transport;
    private readonly PregameHost _pregameHost;
    private readonly DedicatedServerQueryResponder? _queryResponder;
    private readonly DedicatedServerAdvertiser? _advertiser;
    private readonly InEngineRconServer? _rcon;
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

        var mapName = options.Pregame.Session.MapLoad.MapName;
        if (string.IsNullOrWhiteSpace(mapName))
            mapName = MapLoaderConstants.DefaultMapName;
        if (options.IwadBytes.Length > 0
            && LevelBuilder.TryFromWad(options.IwadBytes, mapName, out var level, out _))
        {
            Simulation = AuthoritySimulation.Start(level, options.Pregame.Session.MapLoad.RngSeed);
        }

        if (Simulation != null && !string.IsNullOrEmpty(options.RconPassword))
            _rcon = new InEngineRconServer(options.RconPassword, Simulation, options.RconPort);
    }

    public int RconPort => _rcon?.Port ?? 0;

    public int BoundPort => _transport.BoundPort;

    public PregameHost PregameHost => _pregameHost;

    public LiveAuthoritySession? LiveSession => _liveSession;

    public AuthoritySimulation? Simulation { get; private set; }

    public void Pump(ulong nowMilliseconds)
    {
        _advertiser?.Pump(nowMilliseconds);
        _pregameHost.Pump(nowMilliseconds);
        if (_liveSession is null
            && _pregameHost.TryCreateBootstrappedLiveAuthoritySession(
                _options.IwadBytes,
                out var session,
                out _,
                _options.ReplicateSectorMetadata)
            && session is not null)
        {
            _liveSession = session;
            SyncLiveClients(_liveSession);
            if (Simulation != null)
                _liveSession.SetClientInputSink(new SimulationCommandSink(Simulation));
        }

        if (Simulation != null && _liveSession is not null)
        {
            foreach (var client in _liveSession.Clients.Clients)
                _liveSession.TryReceiveClientInput(client.Endpoint, out _, out _);
        }

        Simulation?.Tick();
        if (Simulation != null && _liveSession?.AuthorityWorldState is { } store)
            SimSnapshotPublisher.Publish(Simulation, store);

        if (_liveSession is not null)
            _liveSession.Pump(nowMilliseconds);
    }

    private void SyncLiveClients(LiveAuthoritySession session)
    {
        foreach (var client in _pregameHost.Clients)
        {
            if (client.HasStartGameAck)
                session.TrackClient(client.Address, client.ClientSlot);
        }
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

    public void Dispose()
    {
        if (_rcon != null)
            _rcon.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _transport.Dispose();
    }
}
