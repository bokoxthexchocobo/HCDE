using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public static class GameplayPayloadBuilders
{
    public static int BuildEmptyClientInput(Span<byte> payload)
    {
        const ushort bodyBytes = LiveConstants.ClientInputRecordsHeaderSize;
        var header = new ClientInputHeader(
            controlFlags: 0,
            routingByte: 0,
            playerCount: 0,
            sequenceAck: 0,
            consistencyAck: 0,
            baseSequence: 0,
            baseConsistency: 0,
            commandTics: 0,
            consistencyTics: 0,
            stabilityBuffer: (byte)NetConstants.StabilityTics,
            bodyBytes: bodyBytes);

        if (payload.Length < LiveConstants.ClientInputHeaderSize + bodyBytes)
            return 0;

        var written = ClientInputHeader.Write(payload, header);
        if (written == 0)
            return 0;

        var bodyWritten = ClientInputRecordsHeader.Write(
            payload[LiveConstants.ClientInputHeaderSize..],
            new ClientInputRecordsHeader(playerCount: 0));
        return bodyWritten == 0 ? 0 : written + bodyWritten;
    }

    public static int BuildEmptyServerSnapshot(Span<byte> payload)
    {
        const ushort bodyBytes = LiveConstants.ServerSnapshotRecordsHeaderSize;
        var header = new ServerSnapshotHeader(
            controlFlags: 0,
            routingByte: 0,
            playerCount: 0,
            sequenceAck: 0,
            consistencyAck: 0,
            quitterBytes: 0,
            baseSequence: 0,
            baseConsistency: 0,
            commandTics: 0,
            consistencyTics: 0,
            stabilityBuffer: (byte)NetConstants.StabilityTics,
            bodyBytes: bodyBytes);

        if (payload.Length < LiveConstants.ServerSnapshotHeaderSize + bodyBytes)
            return 0;

        var written = ServerSnapshotHeader.Write(payload, header);
        if (written == 0)
            return 0;

        var bodyWritten = ServerSnapshotRecordsHeader.Write(
            payload[LiveConstants.ServerSnapshotHeaderSize..],
            new ServerSnapshotRecordsHeader(playerCount: 0));
        return bodyWritten == 0 ? 0 : written + bodyWritten;
    }

    public static int BuildClientInput(
        Span<byte> payload,
        byte playerCount,
        byte commandTics,
        byte consistencyTics,
        uint sequenceAck,
        uint consistencyAck,
        uint baseSequence,
        uint baseConsistency,
        IReadOnlyList<ClientInputPlayerRecord> players)
    {
        if (payload.Length < LiveConstants.ClientInputHeaderSize + LiveConstants.ClientInputRecordsHeaderSize)
            return 0;

        var bodyWritten = ClientInputBodyCodec.Write(payload[LiveConstants.ClientInputHeaderSize..], players);
        if (bodyWritten == 0)
            return 0;

        var header = new ClientInputHeader(
            controlFlags: 0,
            routingByte: 0,
            playerCount: playerCount,
            sequenceAck: sequenceAck,
            consistencyAck: consistencyAck,
            baseSequence: baseSequence,
            baseConsistency: baseConsistency,
            commandTics: commandTics,
            consistencyTics: consistencyTics,
            stabilityBuffer: (byte)NetConstants.StabilityTics,
            bodyBytes: (ushort)bodyWritten);

        if (ClientInputHeader.Write(payload, header) == 0)
            return 0;

        return LiveConstants.ClientInputHeaderSize + bodyWritten;
    }

    public static int BuildClientInputSinglePlayer(
        Span<byte> payload,
        byte playerNum,
        UserCmd command,
        byte commandTics = 1,
        byte consistencyTics = 0)
    {
        var players = new[]
        {
            new ClientInputPlayerRecord
            {
                PlayerNum = playerNum,
                Commands = new[]
                {
                    new ClientInputCommandRecord
                    {
                        CommandOffset = 0,
                        Command = command,
                    },
                },
            },
        };

        return BuildClientInput(
            payload,
            playerCount: 1,
            commandTics,
            consistencyTics,
            sequenceAck: 0,
            consistencyAck: 0,
            baseSequence: 1,
            baseConsistency: 0,
            players);
    }

    public static int BuildServerSnapshot(
        Span<byte> payload,
        byte playerCount,
        byte commandTics,
        byte consistencyTics,
        uint sequenceAck,
        uint consistencyAck,
        uint baseSequence,
        uint baseConsistency,
        IReadOnlyList<ServerSnapshotPlayerRecord> players)
    {
        if (payload.Length < LiveConstants.ServerSnapshotHeaderSize + LiveConstants.ServerSnapshotRecordsHeaderSize)
            return 0;

        var bodyWritten = ServerSnapshotBodyCodec.WritePlayerRecords(payload[LiveConstants.ServerSnapshotHeaderSize..], players);
        if (bodyWritten == 0)
            return 0;

        var header = new ServerSnapshotHeader(
            controlFlags: 0,
            routingByte: 0,
            playerCount: playerCount,
            sequenceAck: sequenceAck,
            consistencyAck: consistencyAck,
            quitterBytes: 0,
            baseSequence: baseSequence,
            baseConsistency: baseConsistency,
            commandTics: commandTics,
            consistencyTics: consistencyTics,
            stabilityBuffer: (byte)NetConstants.StabilityTics,
            bodyBytes: (ushort)bodyWritten);

        if (ServerSnapshotHeader.Write(payload, header) == 0)
            return 0;

        return LiveConstants.ServerSnapshotHeaderSize + bodyWritten;
    }

    public static int BuildServerSnapshotSinglePlayer(
        Span<byte> payload,
        byte playerNum,
        UserCmd command,
        ushort averageLatency = 0,
        byte commandTics = 1,
        byte consistencyTics = 0)
    {
        var players = new[]
        {
            new ServerSnapshotPlayerRecord
            {
                PlayerNum = playerNum,
                AverageLatency = averageLatency,
                Commands = new[]
                {
                    new ServerSnapshotCommandRecord
                    {
                        CommandOffset = 0,
                        Command = command,
                    },
                },
            },
        };

        return BuildServerSnapshot(
            payload,
            playerCount: 1,
            commandTics,
            consistencyTics,
            sequenceAck: 0,
            consistencyAck: 0,
            baseSequence: 1,
            baseConsistency: 0,
            players);
    }
}

public static class LiveGameplayPacketBuilder
{
    public static LivePacket BuildWrapped(
        LiveMessageType messageType,
        GameplayPayloadKind payloadKind,
        uint txSequence,
        uint acknowledgement,
        byte roomId,
        uint gameTic,
        ReadOnlySpan<byte> gameplayPayload)
    {
        var envelopeSize = LiveConstants.GameplayHeaderSize + gameplayPayload.Length;
        var envelopeBuffer = new byte[envelopeSize];
        GameplayEnvelope.Write(
            envelopeBuffer,
            new GameplayEnvelope(payloadKind, roomId, GameplayEnvelopeFlags.None, gameTic));
        gameplayPayload.CopyTo(envelopeBuffer.AsSpan(LiveConstants.GameplayHeaderSize));

        var header = new LiveHeader(messageType, txSequence, acknowledgement);
        return new LivePacket(header, envelopeBuffer);
    }

    public static bool TryUnwrap(
        LivePacket packet,
        GameplayPayloadKind expectedKind,
        byte currentRoomId,
        out GameplayEnvelope envelope,
        out ReadOnlyMemory<byte> gameplayPayload,
        out string? rejectReason)
    {
        envelope = default;
        gameplayPayload = default;
        rejectReason = null;

        if (!GameplayEnvelope.TryRead(packet.Payload.Span, out envelope))
        {
            rejectReason = "missing-gameplay-envelope";
            return false;
        }

        if (!GameplayEnvelope.Validate(envelope, expectedKind, currentRoomId, out rejectReason))
            return false;

        if (packet.Payload.Length < LiveConstants.GameplayHeaderSize)
        {
            rejectReason = "gameplay-payload-too-short";
            return false;
        }

        gameplayPayload = packet.Payload[LiveConstants.GameplayHeaderSize..];
        return true;
    }
}
