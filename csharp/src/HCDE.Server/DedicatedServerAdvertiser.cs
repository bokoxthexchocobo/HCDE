using HCDE.Net.Transport;
using HCDE.Protocol;

namespace HCDE.Server;

public sealed class DedicatedServerAdvertiser
{
    private readonly UdpTransport _transport;
    private readonly NetworkEndpoint _masterEndpoint;
    private readonly ushort _gamePort;
    private readonly ulong _intervalMilliseconds;
    private ulong _lastHeartbeatMilliseconds;

    public DedicatedServerAdvertiser(
        UdpTransport transport,
        NetworkEndpoint masterEndpoint,
        ushort gamePort,
        int intervalSeconds = MasterProtocol.ServerHeartbeatIntervalSeconds)
    {
        _transport = transport;
        _masterEndpoint = masterEndpoint;
        _gamePort = gamePort;
        _intervalMilliseconds = (ulong)Math.Max(1, intervalSeconds) * 1000;
    }

    public void Pump(ulong nowMilliseconds)
    {
        if (_lastHeartbeatMilliseconds != 0
            && nowMilliseconds - _lastHeartbeatMilliseconds < _intervalMilliseconds)
        {
            return;
        }

        _lastHeartbeatMilliseconds = nowMilliseconds;
        _transport.Send(MasterPackets.CreateServerHeartbeat(_gamePort), _masterEndpoint);
    }
}
