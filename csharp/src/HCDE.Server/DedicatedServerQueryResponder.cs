using System.Buffers.Binary;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;

namespace HCDE.Server;

public sealed class DedicatedServerQueryResponder : IPregameInboundInterceptor
{
    private readonly UdpTransport _transport;
    private readonly Func<ServerQuerySnapshot> _snapshotFactory;
    private readonly byte[] _responseBuffer = new byte[NetConstants.MaxMessageLength];

    public DedicatedServerQueryResponder(UdpTransport transport, Func<ServerQuerySnapshot> snapshotFactory)
    {
        _transport = transport;
        _snapshotFactory = snapshotFactory;
    }

    public bool TryHandle(ReadOnlySpan<byte> packet, NetworkEndpoint remote)
    {
        if (!TryParseChallenge(packet, out var echoToken))
            return false;

        if (!ServerQueryCodec.TryWriteResponse(_snapshotFactory(), echoToken, _responseBuffer, out var length))
            return false;

        _transport.Send(_responseBuffer.AsSpan(0, length), remote);
        return true;
    }

    internal static bool TryParseChallenge(ReadOnlySpan<byte> packet, out uint echoToken)
    {
        echoToken = 0;
        if (packet.Length < 4)
            return false;

        var challenge = BinaryPrimitives.ReadUInt32BigEndian(packet);
        if (challenge == (uint)NetConstants.LauncherChallenge
            || challenge == unchecked((uint)NetConstants.ProtoChallenge))
        {
            if (packet.Length >= 8)
                echoToken = BinaryPrimitives.ReadUInt32BigEndian(packet[4..]);

            return true;
        }

        return ((challenge >> 20) & 0x0FFFu) == NetConstants.OdamexQueryTagId;
    }
}
