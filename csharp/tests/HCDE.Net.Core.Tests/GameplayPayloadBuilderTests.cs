namespace HCDE.Net.Core.Tests;

public class GameplayPayloadBuilderTests
{
    [Fact]
    public void EmptyClientInput_HasValidHeaderAndHcirBody()
    {
        Span<byte> payload = stackalloc byte[LiveConstants.ClientInputHeaderSize + LiveConstants.ClientInputRecordsHeaderSize];
        Assert.Equal(LiveConstants.ClientInputHeaderSize + LiveConstants.ClientInputRecordsHeaderSize, GameplayPayloadBuilders.BuildEmptyClientInput(payload));
        Assert.True(ClientInputHeader.TryRead(payload, out var header));
        Assert.Equal((ushort)LiveConstants.ClientInputRecordsHeaderSize, header.BodyBytes);
        Assert.Equal((byte)0, header.PlayerCount);
        Assert.True(ClientInputRecordsHeader.TryRead(payload[LiveConstants.ClientInputHeaderSize..], out var records));
        Assert.Equal((byte)0, records.PlayerCount);
        Assert.True(ClientInputHeader.ValidateHeader(header, payload.Length, out _));
    }

    [Fact]
    public void EmptyServerSnapshot_HasValidHeaderAndHcsrBody()
    {
        Span<byte> payload = stackalloc byte[LiveConstants.ServerSnapshotHeaderSize + LiveConstants.ServerSnapshotRecordsHeaderSize];
        Assert.Equal(LiveConstants.ServerSnapshotHeaderSize + LiveConstants.ServerSnapshotRecordsHeaderSize, GameplayPayloadBuilders.BuildEmptyServerSnapshot(payload));
        Assert.True(ServerSnapshotHeader.TryRead(payload, out var header));
        Assert.Equal((ushort)LiveConstants.ServerSnapshotRecordsHeaderSize, header.BodyBytes);
        Assert.True(ServerSnapshotHeader.ValidateHeader(header, payload.Length, out _));
    }

    [Fact]
    public void LiveGameplayPacketBuilder_WrapsHcInInHgpl()
    {
        Span<byte> native = stackalloc byte[LiveConstants.ClientInputHeaderSize + LiveConstants.ClientInputRecordsHeaderSize];
        GameplayPayloadBuilders.BuildEmptyClientInput(native);
        var packet = LiveGameplayPacketBuilder.BuildWrapped(
            LiveMessageType.ClientCommands,
            GameplayPayloadKind.ClientInputs,
            txSequence: 1,
            acknowledgement: 0,
            roomId: 0,
            gameTic: 5,
            native);

        Assert.True(LiveGameplayPacketBuilder.TryUnwrap(
            packet,
            GameplayPayloadKind.ClientInputs,
            currentRoomId: 0,
            out var envelope,
            out var payload,
            out _));
        Assert.Equal(GameplayPayloadKind.ClientInputs, envelope.Kind);
        Assert.Equal(5u, envelope.GameTic);
        Assert.True(ClientInputHeader.LooksLikeHeader(payload.Span));
    }
}
