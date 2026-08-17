using HCDE.Net.Core;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;

namespace HCDE.Server;

public sealed class DedicatedServerOptions
{
    public int Port { get; set; } = 10666;
    public string BindAddress { get; set; } = "0.0.0.0";
    public byte[] IwadBytes { get; set; } = Array.Empty<byte>();
    public bool ReplicateSectorMetadata { get; set; } = true;
    public PregameHostOptions Pregame { get; set; } = new();
}

public sealed class DedicatedServerHost : IDisposable
{
    private readonly DedicatedServerOptions _options;
    private readonly UdpTransport _transport;
    private readonly PregameHost _pregameHost;
    private LiveAuthoritySession? _liveSession;

    public DedicatedServerHost(DedicatedServerOptions options)
    {
        _options = options;
        _transport = new UdpTransport();
        _transport.Bind(options.Port);
        _transport.SetNonBlocking(true);
        _pregameHost = new PregameHost(_transport, options.Pregame);
    }

    public int BoundPort => _transport.BoundPort;

    public PregameHost PregameHost => _pregameHost;

    public LiveAuthoritySession? LiveSession => _liveSession;

    public void Pump(ulong nowMilliseconds)
    {
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

    public void Dispose() => _transport.Dispose();
}
