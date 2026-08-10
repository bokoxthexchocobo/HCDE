using System.Buffers.Binary;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame.Tests;

public class Crc32Tests
{
    [Fact]
    public void EmptyInputMatchesZlib()
    {
        Assert.Equal(0u, Crc32.Calc(ReadOnlySpan<byte>.Empty));
    }

    [Fact]
    public void KnownVector()
    {
        var data = "123456789"u8.ToArray();
        Assert.Equal(0xCBF43926u, Crc32.Calc(data));
    }

    [Fact]
    public void AddChainsLikeZlib()
    {
        var left = "abc"u8.ToArray();
        var right = "def"u8.ToArray();
        var combined = "abcdef"u8.ToArray();
        var chained = Crc32.Add(Crc32.Calc(left), right);
        Assert.Equal(Crc32.Calc(combined), chained);
    }
}

public class SetupPacketCodecTests
{
    [Fact]
    public void RoundTripPreservesPayload()
    {
        Span<byte> netBuffer = stackalloc byte[32];
        netBuffer[0] = (byte)NetCommandFlags.Setup;
        netBuffer[1] = (byte)PregameSetupType.ConnectAck;
        netBuffer[2] = 1;

        var wire = new byte[SetupPacketCodec.CrcPrefixSize + 3];
        Assert.Equal(wire.Length, SetupPacketCodec.Encode(netBuffer[..3], wire));

        var decoded = new byte[32];
        var status = SetupPacketCodec.TryDecode(wire, decoded, out var length);
        Assert.Equal(SetupPacketDecodeStatus.Ok, status);
        Assert.Equal(3, length);
        Assert.Equal(netBuffer[..3].ToArray(), decoded.AsSpan(0, length).ToArray());
    }

    [Fact]
    public void RejectsBadCrc()
    {
        var wire = new byte[] { 0, 0, 0, 0, (byte)NetCommandFlags.Setup, 1, 2 };
        var decoded = new byte[16];
        Assert.Equal(SetupPacketDecodeStatus.BadCrc, SetupPacketCodec.TryDecode(wire, decoded, out _));
    }
}

public class HcdeServicePacketTests
{
    [Fact]
    public void HeaderLayoutMatchesCpp()
    {
        const uint token = 0x11223344;
        const uint sequence = 7;
        const uint acknowledgement = 3;
        var payload = new byte[] { 0xAA, 0xBB };

        var netBuffer = new byte[PregameConstants.ServiceHeaderSize + payload.Length];
        var length = HcdeServicePacket.Write(
            netBuffer,
            PregameServiceType.ConsolePlayer,
            token,
            sequence,
            acknowledgement,
            payload);

        Assert.Equal(PregameConstants.ServiceHeaderSize + payload.Length, length);
        Assert.Equal((byte)NetCommandFlags.Setup, netBuffer[0]);
        Assert.Equal((byte)PregameSetupType.HcdeService, netBuffer[1]);
        Assert.Equal((byte)PregameServiceType.ConsolePlayer, netBuffer[2]);
        Assert.Equal(token, BinaryPrimitives.ReadUInt32BigEndian(netBuffer.AsSpan(PregameConstants.SessionTokenOffset, 4)));
        Assert.Equal(sequence, BinaryPrimitives.ReadUInt32BigEndian(netBuffer.AsSpan(PregameConstants.ServiceSequenceOffset, 4)));
        Assert.Equal(acknowledgement, BinaryPrimitives.ReadUInt32BigEndian(netBuffer.AsSpan(PregameConstants.ServiceAckOffset, 4)));

        Assert.True(HcdeServicePacket.TryRead(netBuffer, out var parsed));
        Assert.Equal(PregameServiceType.ConsolePlayer, parsed.Service);
        Assert.Equal(token, parsed.SessionToken);
        Assert.Equal(sequence, parsed.Sequence);
        Assert.Equal(acknowledgement, parsed.Acknowledgement);
        Assert.Equal(payload, parsed.Payload.ToArray());
    }
}

public class PregameServiceReceiverTests
{
    [Fact]
    public void AcceptsNewSequenceAndPeerAck()
    {
        var connection = new PregameConnectionState { SessionToken = 99, ServiceTxSeq = 5 };
        var receiver = new PregameServiceReceiver();
        var netBuffer = new byte[PregameConstants.ServiceHeaderSize];
        HcdeServicePacket.WriteHeader(
            netBuffer,
            PregameServiceType.Heartbeat,
            99,
            sequence: 1,
            acknowledgement: 2);

        Assert.Equal(PregameServiceReceiveResult.Accepted, receiver.TryAccept(netBuffer, connection, 1000));
        Assert.Equal(1u, connection.ServiceRxSeq);
        Assert.Equal(2u, connection.ServicePeerAck);
    }

    [Fact]
    public void DuplicateSequenceIsBenign()
    {
        var connection = new PregameConnectionState { SessionToken = 1, ServiceRxSeq = 3, ServiceTxSeq = 4 };
        var receiver = new PregameServiceReceiver();
        var netBuffer = new byte[PregameConstants.ServiceHeaderSize];
        HcdeServicePacket.WriteHeader(netBuffer, PregameServiceType.Heartbeat, 1, 2, 4);

        Assert.Equal(PregameServiceReceiveResult.Duplicate, receiver.TryAccept(netBuffer, connection, 1000));
        Assert.Equal(3u, connection.ServiceRxSeq);
        Assert.Equal(0u, connection.ServiceMalformedStrikes);
    }
}

public class ConnectAckCodecTests
{
    [Fact]
    public void RoundTrip()
    {
        var netBuffer = new byte[ConnectAckPacket.WithConnectInfoSize];
        var length = ConnectAckPacket.Write(
            netBuffer,
            clientSlot: 1,
            connectedPlayers: 2,
            maxClients: 8,
            sessionToken: 0xDEADBEEF,
            PreConnectAckFlags.HcdeService | PreConnectAckFlags.ServerAuthority,
            PregameConstants.ConnectProtocolVersion,
            HcdeConnectFlags.ServerAuthority);

        Assert.True(ConnectAckPacket.TryRead(netBuffer.AsSpan(0, length), out var parsed));
        Assert.Equal(1, parsed.ClientSlot);
        Assert.Equal(2, parsed.ConnectedPlayers);
        Assert.Equal(8, parsed.MaxClients);
        Assert.Equal(0xDEADBEEFu, parsed.SessionToken);
        Assert.True(parsed.Flags.HasFlag(PreConnectAckFlags.HcdeService));
        Assert.Equal(PregameConstants.ConnectProtocolVersion, parsed.ConnectVersion);
        Assert.Equal(HcdeConnectFlags.ServerAuthority, parsed.ConnectFlags);
    }
}

public class ReliableServiceQueueTests
{
    [Fact]
    public void FlushUpdatesAckField()
    {
        var connection = new PregameConnectionState { SessionToken = 5, ServiceRxSeq = 9 };
        var sender = new PregameServiceSender();
        var payload = new byte[] { 1, 2, 3, (byte)HcdeConnectFlags.ServerAuthority };
        Assert.True(sender.TryQueueReliable(PregameServiceType.ConsolePlayer, connection, key: 1, payload));

        var netBuffer = new byte[NetConstants.MaxMessageLength];
        Assert.True(sender.TryFlush(connection, 1000, netBuffer, out var length, force: true));
        Assert.True(length > PregameConstants.ServiceHeaderSize);
        Assert.Equal(9u, BinaryPrimitives.ReadUInt32BigEndian(netBuffer.AsSpan(PregameConstants.ServiceAckOffset, 4)));
    }
}

public class PregameHandshakeIntegrationTests
{
    [Fact]
    public void ConnectAckAndConsolePlayerServiceRoundTripOnWire()
    {
        const uint token = 0xCAFEBABE;
        var host = new PregameConnectionState { SessionToken = token };
        var guest = new PregameConnectionState { SessionToken = token };
        var hostSender = new PregameServiceSender();
        var guestReceiver = new PregameServiceReceiver();

        var connectAck = new byte[ConnectAckPacket.WithConnectInfoSize];
        var ackLength = ConnectAckPacket.Write(
            connectAck,
            clientSlot: 1,
            connectedPlayers: 1,
            maxClients: 8,
            token,
            PreConnectAckFlags.HcdeService,
            PregameConstants.ConnectProtocolVersion,
            HcdeConnectFlags.ServerAuthority);

        var wire = new byte[SetupPacketCodec.CrcPrefixSize + ackLength];
        Assert.Equal(wire.Length, SetupPacketCodec.Encode(connectAck.AsSpan(0, ackLength), wire));

        var guestNet = new byte[NetConstants.MaxMessageLength];
        Assert.Equal(SetupPacketDecodeStatus.Ok, SetupPacketCodec.TryDecode(wire, guestNet, out var guestNetLength));
        Assert.True(ConnectAckPacket.TryRead(guestNet.AsSpan(0, guestNetLength), out var parsedAck));
        Assert.Equal(token, parsedAck.SessionToken);

        var consolePayload = new byte[] { 1, 1, 8, (byte)HcdeConnectFlags.ServerAuthority };
        Assert.True(hostSender.TryQueueReliable(PregameServiceType.ConsolePlayer, host, key: 1, consolePayload));

        var hostNet = new byte[NetConstants.MaxMessageLength];
        Assert.True(hostSender.TryFlush(host, 1000, hostNet, out var hostNetLength, force: true));

        var serviceWire = new byte[SetupPacketCodec.CrcPrefixSize + hostNetLength];
        Assert.Equal(serviceWire.Length, SetupPacketCodec.Encode(hostNet.AsSpan(0, hostNetLength), serviceWire));

        var guestServiceNet = new byte[NetConstants.MaxMessageLength];
        Assert.Equal(SetupPacketDecodeStatus.Ok, SetupPacketCodec.TryDecode(serviceWire, guestServiceNet, out var guestServiceLength));
        Assert.Equal(PregameServiceReceiveResult.Accepted, guestReceiver.TryAccept(guestServiceNet.AsSpan(0, guestServiceLength), guest, 2000));
        Assert.True(HcdeServicePacket.TryRead(guestServiceNet.AsSpan(0, guestServiceLength), out var service));
        Assert.Equal(PregameServiceType.ConsolePlayer, service.Service);
        Assert.Equal(consolePayload, service.Payload.ToArray());
    }
}
