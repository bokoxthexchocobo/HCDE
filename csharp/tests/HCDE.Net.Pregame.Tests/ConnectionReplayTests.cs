using System.Net;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame.Tests;

public class ConnectionReplayTests
{
    [Theory]
    [InlineData(2, ConnectionStatus.Connecting)]
    [InlineData(8, ConnectionStatus.Connecting)]
    [InlineData(2, ConnectionStatus.Waiting)]
    [InlineData(8, ConnectionStatus.Waiting)]
    [InlineData(2, ConnectionStatus.Ready)]
    [InlineData(8, ConnectionStatus.Ready)]
    public void RepeatedConnectReusesSessionEvenWhenServerIsFull(int capacity, ConnectionStatus status)
    {
        using var server = Bound(); using var peer = Bound();
        var host = new PregameHost(server, new PregameHostOptions { MaxClients = capacity });
        var address = Endpoint(server); var buffer = new byte[2048];
        void SendConnect()
        {
            var length = ConnectPacketCodec.Write(buffer, new EngineInfoSnapshot(), "", HcdeConnectFlags.ServerAuthority);
            Assert.True(PregameWire.TrySend(peer, buffer.AsSpan(0, length), address));
        }
        ConnectAckPacket ReceiveAck()
        {
            var deadline = Environment.TickCount64 + 2000;
            while (Environment.TickCount64 < deadline)
            {
                host.Pump((ulong)Environment.TickCount64);
                while (PregameWire.TryReceive(peer, buffer, out var length, out _, TimeSpan.Zero) == SetupPacketDecodeStatus.Ok)
                    if (ConnectAckPacket.TryRead(buffer.AsSpan(0, length), out var ack)) return ack;
            }
            throw new TimeoutException("No ConnectAck received");
        }
        SendConnect(); var first = ReceiveAck();
        var client = Assert.Single(host.Clients, item => item.Status != ConnectionStatus.None);
        client.Status = status;
        var token = client.Connection.SessionToken;
        // Remove previously queued datagrams before the replay.
        while (peer.TryReceive(buffer, out _, out _, TimeSpan.Zero)) { }
        SendConnect(); var repeated = ReceiveAck();
        Assert.Single(host.Clients, item => item.Status != ConnectionStatus.None);
        Assert.Equal(first.ClientSlot, repeated.ClientSlot);
        Assert.Equal(token, repeated.SessionToken); Assert.Equal(token, client.Connection.SessionToken);
        Assert.Equal(status, client.Status);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GuestIgnoresSetupFromOtherEndpoint(bool alreadyConnected)
    {
        using var server = Bound(); using var other = Bound(); using var peer = Bound();
        var guest = new PregameGuest(peer, new PregameGuestOptions { ServerAddress = Endpoint(server) });
        if (alreadyConnected) guest.Connection.SessionToken = 123;
        var buffer = new byte[2048];
        var length = alreadyConnected
            ? HcdeServicePacket.Write(buffer, PregameServiceType.StartGame, 123, 1, 0, ReadOnlySpan<byte>.Empty)
            : ConnectAckPacket.Write(buffer, 1, 1, 8, 456, PreConnectAckFlags.HcdeService,
                PregameConstants.ConnectProtocolVersion, HcdeConnectFlags.ServerAuthority);
        Assert.True(PregameWire.TrySend(other, buffer.AsSpan(0, length), Endpoint(peer)));
        // Follow with an authoritative packet and wait for its processing.
        length = ConnectAckPacket.Write(buffer, 1, 1, 8, 123, PreConnectAckFlags.HcdeService,
            PregameConstants.ConnectProtocolVersion, HcdeConnectFlags.ServerAuthority);
        Assert.True(PregameWire.TrySend(server, buffer.AsSpan(0, length), Endpoint(peer)));
        length = HcdeServicePacket.Write(buffer, PregameServiceType.Heartbeat, 123, 2, 0, ReadOnlySpan<byte>.Empty);
        Assert.True(PregameWire.TrySend(server, buffer.AsSpan(0, length), Endpoint(peer)));
        var deadline = Environment.TickCount64 + 2000;
        while (guest.Connection.ServiceRxSeq != 2 && Environment.TickCount64 < deadline)
            guest.Pump((ulong)Environment.TickCount64);
        Assert.Equal(2u, guest.Connection.ServiceRxSeq);
        Assert.Equal(123u, guest.Connection.SessionToken);
        Assert.NotEqual(PregameGuestPhase.Starting, guest.Phase);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1000UL)]
    public void LostInitialConnectRetriesAtDeadlineAndStopsAfterAck(ulong start)
    {
        using var server = Bound(); using var peer = Bound();
        var guest = new PregameGuest(peer, new PregameGuestOptions { ServerAddress = Endpoint(server) });
        var buffer = new byte[2048];
        guest.Pump(start);
        Assert.Equal(SetupPacketDecodeStatus.Ok,
            PregameWire.TryReceive(server, buffer, out var length, out _, TimeSpan.FromSeconds(2)));
        Assert.True(ConnectPacketCodec.TryRead(buffer.AsSpan(0, length), out _));
        // Drop the initial request. Pumping before the retry deadline must stay quiet.
        var due = start + PregameConstants.RuntimeConnectAckResendMilliseconds;
        guest.Pump(due - 1);
        Assert.False(server.TryReceive(buffer, out _, out _, TimeSpan.Zero));
        guest.Pump(due);
        Assert.Equal(SetupPacketDecodeStatus.Ok,
            PregameWire.TryReceive(server, buffer, out length, out _, TimeSpan.FromSeconds(2)));
        Assert.True(ConnectPacketCodec.TryRead(buffer.AsSpan(0, length), out _));
        // Once acknowledged, subsequent retry deadlines must stay quiet.
        length = ConnectAckPacket.Write(buffer, 1, 1, 8, 123, PreConnectAckFlags.HcdeService,
            PregameConstants.ConnectProtocolVersion, HcdeConnectFlags.ServerAuthority);
        Assert.True(PregameWire.TrySend(server, buffer.AsSpan(0, length), Endpoint(peer)));
        var deadline = Environment.TickCount64 + 2000;
        while (guest.Connection.SessionToken == 0 && Environment.TickCount64 < deadline) guest.Pump(due);
        Assert.Equal(123u, guest.Connection.SessionToken);
        guest.Pump(due + PregameConstants.RuntimeConnectAckResendMilliseconds);
        Assert.False(server.TryReceive(buffer, out _, out _, TimeSpan.Zero));
    }

    [Fact]
    public void RejectedConnectDoesNotRetry()
    {
        using var server = Bound(); using var peer = Bound();
        var guest = new PregameGuest(peer, new PregameGuestOptions { ServerAddress = Endpoint(server) });
        var buffer = new byte[2048]; guest.Pump(1000);
        Assert.True(server.TryReceive(buffer, out _, out _, TimeSpan.FromSeconds(2)));
        Assert.True(PregameWire.TrySend(server,
            new byte[] { (byte)NetCommandFlags.Setup, (byte)PregameSetupType.Full }, Endpoint(peer)));
        var deadline = Environment.TickCount64 + 2000;
        while (guest.Phase != PregameGuestPhase.Rejected && Environment.TickCount64 < deadline) guest.Pump(1000);
        Assert.Equal(PregameGuestPhase.Rejected, guest.Phase);
        guest.Pump(1000UL + PregameConstants.RuntimeConnectAckResendMilliseconds * 2);
        Assert.False(server.TryReceive(buffer, out _, out _, TimeSpan.Zero));
    }

    private static UdpTransport Bound()
    {
        var transport = new UdpTransport(); transport.Bind(0); transport.SetNonBlocking(true); return transport;
    }
    private static NetworkEndpoint Endpoint(UdpTransport transport) => new(IPAddress.Loopback, transport.BoundPort);
}
