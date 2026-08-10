using HCDE.Protocol;

namespace HCDE.Protocol.Tests;

public class Nms1PacketTests
{
    private static byte[] Buffer() => new byte[MasterProtocol.Nms1MaxPacketSize];

    [Fact]
    public void ChallengeRequestGoldenVector()
    {
        var buffer = Buffer();
        Assert.True(Nms1Packets.TryWriteChallengeRequest(0x12345678, Nms1ChallengePurpose.Registration, buffer, out var length));

        var expected = new byte[]
        {
            0x4E, 0x4D, 0x53, 0x31, 0x01, 0x01, 0x00, 0x00,
            0x12, 0x34, 0x56, 0x78, 0x00, 0x05, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x01, 0x01,
        };

        Assert.Equal(expected.Length, length);
        Assert.Equal(expected, buffer.AsSpan(0, length).ToArray());
    }

    [Fact]
    public void ChallengeResponseRoundTrip()
    {
        var requestId = 42u;
        var challenge = new Nms1ChallengeToken { IssuedUnix = 1_700_000_000 };
        for (var i = 0; i < challenge.Token.Length; i++)
            challenge.Token[i] = (byte)(i + 1);

        var buffer = Buffer();
        Assert.True(Nms1Packets.TryWriteChallengeResponse(requestId, challenge, 180, buffer, out var length));

        var result = Nms1Packets.TryReadChallengeResponse(buffer.AsSpan(0, length), requestId, new Nms1ChallengeToken(), out var ttl, null);
        Assert.Equal(Nms1ParseResult.Ok, result);
        Assert.Equal(180, ttl);
    }

    [Fact]
    public void RegisterRequestAndAckRoundTrip()
    {
        var requestId = 99u;
        var request = new Nms1RegisterRequest
        {
            ProtocolFamily = "raw",
            GamePort = 10666,
            QueryPort = 10667,
            CurrentPlayers = 2,
            MaxPlayers = 8,
            ServerFlags = 1,
            DisplayName = "Test Server",
            MapName = "MAP01",
        };
        request.Challenge.IssuedUnix = 1_700_000_001;
        request.Challenge.Token[0] = 0xAB;

        var writeBuffer = Buffer();
        Assert.True(Nms1Packets.TryWriteRegisterRequest(requestId, request, writeBuffer, out _));

        var entry = new Nms1EntryToken();
        for (var i = 0; i < entry.Token.Length; i++)
            entry.Token[i] = (byte)i;

        var ackBuffer = Buffer();
        Assert.True(Nms1Packets.TryWriteRegisterAck(requestId, entry, 120, ackBuffer, out var ackLength));

        var parsedEntry = new Nms1EntryToken();
        var result = Nms1Packets.TryReadRegisterAck(ackBuffer.AsSpan(0, ackLength), requestId, parsedEntry, out var ttl, null);
        Assert.Equal(Nms1ParseResult.Ok, result);
        Assert.Equal(120, ttl);
        Assert.Equal(entry.Token, parsedEntry.Token);
    }

    [Fact]
    public void HeartbeatAndUnregisterRoundTrip()
    {
        var requestId = 7u;
        var heartbeat = new Nms1HeartbeatRequest
        {
            ProtocolFamily = "raw",
            GamePort = 10666,
            CurrentPlayers = 1,
            MaxPlayers = 4,
            ServerFlags = 0,
        };
        heartbeat.Entry.Token[0] = 0x42;

        var writeBuffer = Buffer();
        Assert.True(Nms1Packets.TryWriteHeartbeatRequest(requestId, heartbeat, writeBuffer, out _));

        var ackBuffer = Buffer();
        Assert.True(Nms1Packets.TryWriteHeartbeatAck(requestId, 90, ackBuffer, out var ackLength));
        var result = Nms1Packets.TryReadHeartbeatAck(ackBuffer.AsSpan(0, ackLength), requestId, out var ttl, null);
        Assert.Equal(Nms1ParseResult.Ok, result);
        Assert.Equal(90, ttl);

        var unregister = new Nms1UnregisterRequest
        {
            ProtocolFamily = "raw",
            GamePort = 10666,
        };
        unregister.Entry.Token[0] = 0x42;
        Assert.True(Nms1Packets.TryWriteUnregisterRequest(requestId, unregister, writeBuffer, out _));

        var unregisterAck = Buffer();
        Assert.True(Nms1Packets.TryWriteUnregisterAck(requestId, unregisterAck, out var unregisterLength));
        Assert.Equal(Nms1ParseResult.Ok, Nms1Packets.TryReadUnregisterAck(unregisterAck.AsSpan(0, unregisterLength), requestId, null));
    }

    [Theory]
    [InlineData("raw")]
    [InlineData("hcde.v1")]
    [InlineData("test_family-1")]
    public void ValidProtocolFamilies(string family) => Assert.True(Nms1Packets.IsValidProtocolFamily(family));

    [Theory]
    [InlineData("")]
    [InlineData("BAD FAMILY")]
    [InlineData("UPPER")]
    public void InvalidProtocolFamilies(string family) => Assert.False(Nms1Packets.IsValidProtocolFamily(family));

    [Fact]
    public void ErrorResponseRoundTrip()
    {
        var requestId = 5u;
        var buffer = Buffer();
        Assert.True(Nms1Packets.TryWriteErrorResponse(requestId, (ushort)Nms1ErrorCode.ChallengeRequired, "need challenge", buffer, out var length));

        var error = new Nms1ErrorResponse();
        var result = Nms1Packets.TryReadErrorResponse(buffer.AsSpan(0, length), requestId, error);
        Assert.Equal(Nms1ParseResult.ErrorResponse, result);
        Assert.Equal((ushort)Nms1ErrorCode.ChallengeRequired, error.Code);
        Assert.Equal("need challenge", error.Text);
    }
}
