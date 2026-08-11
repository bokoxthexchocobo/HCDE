using System.Buffers.Binary;

namespace HCDE.Net.Core.Tests;

public class GameplayEnvelopeCodecTests
{
    [Fact]
    public void GameplayEnvelope_RoundTrip_MatchesCppLayout()
    {
        var envelope = new GameplayEnvelope(
            GameplayPayloadKind.ClientInputs,
            roomId: 2,
            GameplayEnvelopeFlags.ActorRepairRequest,
            gameTic: 9001);

        Span<byte> buffer = stackalloc byte[LiveConstants.GameplayHeaderSize + 4];
        Assert.Equal(LiveConstants.GameplayHeaderSize, GameplayEnvelope.Write(buffer, envelope));
        Assert.True(GameplayEnvelope.TryRead(buffer, out var parsed));
        Assert.Equal(envelope.Kind, parsed.Kind);
        Assert.Equal(envelope.RoomId, parsed.RoomId);
        Assert.Equal(envelope.Flags, parsed.Flags);
        Assert.Equal(envelope.GameTic, parsed.GameTic);

        Assert.True(buffer[..4].SequenceEqual("HGPL"u8));
        Assert.Equal((byte)GameplayPayloadKind.ClientInputs, buffer[5]);
        Assert.Equal(9001u, BinaryPrimitives.ReadUInt32BigEndian(buffer[8..]));
    }

    [Fact]
    public void GameplayEnvelope_Validate_RejectsStaleRoom()
    {
        var envelope = new GameplayEnvelope(GameplayPayloadKind.ServerSnapshot, 1, GameplayEnvelopeFlags.None, 10);
        Assert.False(GameplayEnvelope.Validate(envelope, GameplayPayloadKind.ServerSnapshot, currentRoomId: 2, out var reason));
        Assert.Contains("stale room", reason);
    }

    [Fact]
    public void GameplayEnvelope_Validate_AllowsActorRepairFlagOnClientInputsOnly()
    {
        var client = new GameplayEnvelope(
            GameplayPayloadKind.ClientInputs,
            0,
            GameplayEnvelopeFlags.ActorRepairRequest,
            1);
        Assert.True(GameplayEnvelope.Validate(client, GameplayPayloadKind.ClientInputs, 0, out _));

        var server = new GameplayEnvelope(
            GameplayPayloadKind.ServerSnapshot,
            0,
            GameplayEnvelopeFlags.ActorRepairRequest,
            1);
        Assert.False(GameplayEnvelope.Validate(server, GameplayPayloadKind.ServerSnapshot, 0, out var reason));
        Assert.Contains("flags", reason);
    }
}
