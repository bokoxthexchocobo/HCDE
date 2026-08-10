using System.Net;
using System.Net.Sockets;

namespace HCDE.Net.Transport;

public sealed class UdpTransport : IDisposable
{
    private readonly Socket _socket;
    private bool _disposed;

    public UdpTransport()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public int BoundPort => ((IPEndPoint)_socket.LocalEndPoint!).Port;

    public void Bind(NetworkEndpoint endpoint)
    {
        _socket.Bind(endpoint.ToEndPoint());
    }

    public void Bind(int port = 0) => Bind(new NetworkEndpoint(IPAddress.Any, port));

    public void SetNonBlocking(bool nonBlocking) => _socket.Blocking = !nonBlocking;

    public int Send(ReadOnlySpan<byte> data, NetworkEndpoint remote)
    {
        return _socket.SendTo(data, SocketFlags.None, remote.ToEndPoint());
    }

    public bool TryReceive(Span<byte> buffer, out int received, out NetworkEndpoint remote, TimeSpan? timeout = null)
    {
        received = 0;
        remote = default;

        if (timeout is { } wait && wait > TimeSpan.Zero)
        {
            if (!_socket.Poll((int)wait.TotalMilliseconds, SelectMode.SelectRead))
                return false;
        }

        EndPoint from = new IPEndPoint(IPAddress.Any, 0);
        received = _socket.ReceiveFrom(buffer, SocketFlags.None, ref from);
        var endpoint = (IPEndPoint)from;
        remote = new NetworkEndpoint(endpoint.Address, endpoint.Port);
        return received > 0;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _socket.Dispose();
    }
}
