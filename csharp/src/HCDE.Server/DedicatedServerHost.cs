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
    public BinaryThingFlagFormat ThingFlagFormat { get; set; } = BinaryThingFlagFormat.Doom;
    public ModResources? Resources { get; set; }
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
    public double PlayerDamageFactor { get; set; } = 1;
    public double SkillDamageFactor { get; set; } = 1;
    public double MonsterDamageFactor { get; set; } = 1;
    public double FriendlyDamageFactor { get; set; } = 1;
    public bool NoMonsterPain { get; set; }
    public bool DisableAutoHealth { get; set; }
    public bool StrifeHealthPickups { get; set; }
    public bool RavenHealthPickups { get; set; }
    public bool UnlimitedHealthPickups { get; set; }
    public bool InfiniteHealthItemUse { get; set; }
    public bool InfiniteStoredInventoryUse { get; set; }
    public bool InfiniteAmmo { get; set; }
    public int DefaultInventoryMaximum { get; set; } = 25;
    public byte GameMode { get; set; }
    public string GameModeName { get; set; } = "Co-op";
    public bool Deathmatch { get; set; }
    /// <summary>Suppress damaging-floor and actor-triggered ordinary map exits in deathmatch.</summary>
    public bool NoExit { get; set; }
    public bool EnableRewind { get; set; }
    public float RewindIntervalSeconds { get; set; } = 1;
    public int RewindDepth { get; set; } = 10;
    public int RewindMaxMegabytes { get; set; } = 32;
    public bool EnableLagCompensation { get; set; }
    public int LagCompensationMaxAgeTics { get; set; } = 12;
    public bool Teamplay { get; set; }
    public CompatSurface Compatibility { get; set; } = CompatSurface.None;
    public string RconPassword { get; set; } = "";
    public int RconPort { get; set; }
    public int InvasionWaves { get; set; } = 8;
    public int InvasionCountdownTics { get; set; } = 30 * GameTicClock.TicRate;
    public int InvasionIntermissionTics { get; set; } = GameTicClock.TicRate;
}

public sealed class DedicatedServerHost : IDisposable, IPregameInboundInterceptor
{
    private readonly DedicatedServerOptions _options;
    private readonly UdpTransport _transport;
    private readonly PregameHost _pregameHost;
    private readonly DedicatedServerQueryResponder? _queryResponder;
    private readonly DedicatedServerAdvertiser? _advertiser;
    private readonly InEngineRconServer? _rcon;
    private readonly IPregameInboundInterceptor? _upstreamInterceptor;
    private LiveAuthoritySession? _liveSession;

    public DedicatedServerHost(DedicatedServerOptions options)
    {
        ValidateRewindOptions(options);
        _options = options;
        var mapName = string.IsNullOrWhiteSpace(options.Pregame.Session.MapLoad.MapName)
            ? MapLoaderConstants.DefaultMapName : options.Pregame.Session.MapLoad.MapName;
        PlayLevel? level = null;
        HCDE.Gamedata.DehackedPatchResult? patch = null;
        if (options.IwadBytes.Length > 0)
        {
            if (!LevelBuilder.TryFromWad(options.IwadBytes, mapName, out level, out var mapError, options.ThingFlagFormat))
                throw new InvalidDataException($"Cannot load {mapName}: {mapError}");
            WadArchiveReader.TryReadDirectory(options.IwadBytes, out var entries, out _);
            foreach (var entry in entries.Where(entry => entry.Name.Equals("DEHACKED", StringComparison.OrdinalIgnoreCase)))
            {
                if (!WadArchiveReader.TryReadLumpData(options.IwadBytes, entry, out var data, out var lumpError))
                    throw new InvalidDataException(lumpError);
                patch = HCDE.Gamedata.DehackedPatch.Apply(System.Text.Encoding.UTF8.GetString(data), patch);
                if (patch.Errors.Count > 0) throw new InvalidDataException(string.Join("; ", patch.Errors));
            }
        }
        _upstreamInterceptor = options.Pregame.InboundInterceptor;
        _transport = new UdpTransport();
        _transport.Bind(options.Port);
        _transport.SetNonBlocking(true);

        if (options.EnableServerQuery)
        {
            _queryResponder = new DedicatedServerQueryResponder(_transport, BuildQuerySnapshot);
        }

        options.Pregame.InboundInterceptor = this;
        _pregameHost = new PregameHost(_transport, options.Pregame);

        if (options.EnableMasterAdvertise)
        {
            var masterEndpoint = new NetworkEndpoint(IPAddress.Parse(options.MasterHost), options.MasterPort);
            _advertiser = new DedicatedServerAdvertiser(_transport, masterEndpoint, (ushort)_transport.BoundPort);
        }

        if (level != null)
        {
            Simulation = AuthoritySimulation.Start(level, options.Pregame.Session.MapLoad.RngSeed, patch,
                compat: options.Compatibility,
                spawnOptions: new SpawnOptions(Math.Clamp((int)options.Skill, 0, 4),
                    options.Deathmatch ? SpawnGameMode.Deathmatch : SpawnGameMode.Cooperative,
                    PlayerDamageFactor: options.PlayerDamageFactor, MonsterDamageFactor: options.MonsterDamageFactor,
                    FriendlyDamageFactor: options.FriendlyDamageFactor, NoMonsterPain: options.NoMonsterPain,
                    SkillDamageFactor: options.SkillDamageFactor, DisableAutoHealth: options.DisableAutoHealth,
                    StrifeHealthPickups: options.StrifeHealthPickups,
                    UnlimitedHealthPickups: options.UnlimitedHealthPickups,
                    DefaultInventoryMaximum: options.DefaultInventoryMaximum,
                    RavenHealthPickups: options.RavenHealthPickups,
                    InfiniteHealthItemUse: options.InfiniteHealthItemUse,
                    InfiniteStoredInventoryUse: options.InfiniteStoredInventoryUse,
                    InfiniteAmmo: options.InfiniteAmmo), noExit: options.NoExit);
            Simulation.Rewind.CaptureIntervalSeconds = options.RewindIntervalSeconds;
            Simulation.Rewind.Capacity = options.RewindDepth;
            Simulation.Rewind.MemoryLimitBytes = options.RewindMaxMegabytes * 1024L * 1024;
            Simulation.Rewind.LagCompensationEnabled = options.EnableLagCompensation;
            Simulation.Rewind.MaxLagCompensationAgeTics = options.LagCompensationMaxAgeTics;
            Simulation.RewindEnabled = options.EnableRewind;
            if (options.GameMode == 4)
                Simulation.Invasion.Configure(options.InvasionWaves, options.InvasionCountdownTics, options.InvasionIntermissionTics);
        }

        if (Simulation != null && !string.IsNullOrEmpty(options.RconPassword))
            _rcon = new InEngineRconServer(options.RconPassword, Simulation, options.RconPort);
    }

    private static void ValidateRewindOptions(DedicatedServerOptions options)
    {
        if (!float.IsFinite(options.RewindIntervalSeconds) || options.RewindIntervalSeconds is < 0.25f or > 30f)
            throw new ArgumentOutOfRangeException(nameof(options.RewindIntervalSeconds));
        if (options.RewindDepth is < 1 or > 64)
            throw new ArgumentOutOfRangeException(nameof(options.RewindDepth));
        if (options.RewindMaxMegabytes is < 1 or > 512)
            throw new ArgumentOutOfRangeException(nameof(options.RewindMaxMegabytes));
        if (options.LagCompensationMaxAgeTics is < 1 or > GameTicClock.TicRate * 2)
            throw new ArgumentOutOfRangeException(nameof(options.LagCompensationMaxAgeTics));
    }

    public int RconPort => _rcon?.Port ?? 0;

    public int BoundPort => _transport.BoundPort;

    public PregameHost PregameHost => _pregameHost;

    public LiveAuthoritySession? LiveSession => _liveSession;

    public AuthoritySimulation? Simulation { get; private set; }

    public void Pump(ulong nowMilliseconds)
    {
        _advertiser?.Pump(nowMilliseconds);
        if (_liveSession != null)
            SyncLiveClients(_liveSession);
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
                _liveSession.SetClientInputSink(new SimulationCommandSink(Simulation, _options.Pregame.Session.GameInfo.TicDup));
        }

        if (Simulation != null && _options.GameMode == 4 && _liveSession != null)
            Simulation.Invasion.Enabled = true;
        // Map actors must not attack player starts while everyone is still in the lobby.
        if (_liveSession != null) Simulation?.Tick();
        if (Simulation != null && _liveSession?.AuthorityWorldState is { } store)
        {
            SimSnapshotPublisher.Publish(Simulation, store);
            _liveSession.SetAuthorityInvasionSnapshot(_options.GameMode == 4
                ? InvasionSnapshotPublisher.Capture(Simulation.Invasion) : null);
        }

        if (_liveSession is not null)
            _liveSession.PumpAllClients(nowMilliseconds);
    }

    bool IPregameInboundInterceptor.TryHandle(ReadOnlySpan<byte> packet, NetworkEndpoint remote) =>
        (_queryResponder?.TryHandle(packet, remote) ?? false)
        || (_liveSession?.TryApplyClientInputPacket(packet, remote) ?? false)
        || (_upstreamInterceptor?.TryHandle(packet, remote) ?? false);

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
            GameModeName = _options.GameMode == 4 && _options.GameModeName == "Co-op" ? "Invasion" : _options.GameModeName,
        };

        foreach (var client in connectedClients)
        {
            var name = string.IsNullOrWhiteSpace(client.UserInfo)
                ? $"Player{client.ClientSlot + 1}"
                : client.UserInfo;
            snapshot.Players.Add(new ServerQueryPlayer { Name = name });
        }

        if (_options.GameMode == 4 && Simulation != null)
            InvasionSnapshotPublisher.ApplyToQuery(Simulation.Invasion, snapshot);

        return snapshot;
    }

    public void Dispose()
    {
        if (_rcon != null)
            _rcon.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _transport.Dispose();
    }
}
