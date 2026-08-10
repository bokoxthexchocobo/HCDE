using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public static class PregameWire
{
    public static bool TrySend(UdpTransport transport, ReadOnlySpan<byte> netBuffer, NetworkEndpoint remote)
    {
        Span<byte> wire = stackalloc byte[SetupPacketCodec.CrcPrefixSize + netBuffer.Length];
        var length = SetupPacketCodec.Encode(netBuffer, wire);
        if (length == 0)
            return false;
        return transport.Send(wire[..length], remote) == length;
    }

    public static SetupPacketDecodeStatus TryReceive(
        UdpTransport transport,
        Span<byte> netBuffer,
        out int netLength,
        out NetworkEndpoint remote,
        TimeSpan? timeout = null)
    {
        netLength = 0;
        remote = default;
        Span<byte> wire = stackalloc byte[NetConstants.MaxTransmitSize];
        if (!transport.TryReceive(wire, out var received, out remote, timeout))
            return SetupPacketDecodeStatus.TooShort;

        return SetupPacketCodec.TryDecode(wire[..received], netBuffer, out netLength);
    }
}
