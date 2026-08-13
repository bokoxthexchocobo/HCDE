using HCDE.Net.Transport;

namespace HCDE.Net.Core.Tests;

public class ServerSnapshotApplySessionTests
{
    private sealed class RecordingSnapshotSink : IServerSnapshotCommandSink
    {
        public List<int> Sequences { get; } = new();

        public bool ApplyCommand(byte playerNum, int sequence, UserCmd command, ReadOnlyMemory<byte> eventRecords)
        {
            Sequences.Add(sequence);
            return true;
        }
    }

    [Fact]
    public void Apply_AdvancesSequenceAndInvokesSink()
    {
        var registry = new LivePeerNetRegistry(maxClients: 4);
        var peerSlots = new LivePeerSlotTracker(4);
        var sink = new RecordingSnapshotSink();
        var routing = new LivePeerRoutingState(
            consolePlayer: 1,
            maxClients: 4,
            authoritySlot: 0,
            isLocalAuthority: false,
            usesHcdeService: true);

        var command = new UserCmd(1, 0, 90, 0, 0, 0, 0);
        var players = new[]
        {
            new ServerSnapshotPlayerRecord
            {
                PlayerNum = 1,
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

        var header = new ServerSnapshotHeader(
            controlFlags: 0,
            routingByte: 0,
            playerCount: 1,
            sequenceAck: 1,
            consistencyAck: 0,
            quitterBytes: 0,
            baseSequence: 1,
            baseConsistency: 0,
            commandTics: 1,
            consistencyTics: 0,
            stabilityBuffer: (byte)NetConstants.StabilityTics,
            bodyBytes: 1);

        Assert.True(ServerSnapshotApplySession.TryApply(
            header,
            ReadOnlySpan<byte>.Empty,
            players,
            remoteGameTic: 10,
            recipientClientSlot: 1,
            routing,
            registry,
            peerSlots,
            sink,
            nowMs: 0,
            out var result,
            out _));

        Assert.False(result.Idempotent);
        Assert.Equal(1, result.CommandsApplied);
        Assert.Equal(new[] { 1 }, sink.Sequences);
        Assert.Equal(1, registry[1].CurrentSequence);
    }

    [Fact]
    public void Apply_DuplicateGameTic_IsIdempotent()
    {
        var registry = new LivePeerNetRegistry(maxClients: 4);
        var routing = new LivePeerRoutingState(1, 4, 0, isLocalAuthority: false, usesHcdeService: true);
        registry[1].LastAppliedSnapshotGameTic = 10;

        var header = new ServerSnapshotHeader(
            0, 0, 1, sequenceAck: 9, consistencyAck: 0, quitterBytes: 0,
            baseSequence: 20, baseConsistency: 0, commandTics: 1, consistencyTics: 0,
            stabilityBuffer: (byte)NetConstants.StabilityTics, bodyBytes: 1);

        Assert.True(ServerSnapshotApplySession.TryApply(
            header,
            ReadOnlySpan<byte>.Empty,
            Array.Empty<ServerSnapshotPlayerRecord>(),
            remoteGameTic: 10,
            recipientClientSlot: 1,
            routing,
            registry,
            peerSlots: null,
            sink: null,
            nowMs: 0,
            out var result,
            out _));

        Assert.True(result.Idempotent);
        Assert.Equal(9, registry[1].SequenceAck);
    }

    [Fact]
    public void Apply_QuitterPrefix_MarksPeerDisconnected()
    {
        var registry = new LivePeerNetRegistry(maxClients: 4);
        var peerSlots = new LivePeerSlotTracker(4);
        var routing = new LivePeerRoutingState(1, 4, 0, isLocalAuthority: false, usesHcdeService: true);
        var header = new ServerSnapshotHeader(
            controlFlags: (byte)NetCommandFlags.Quitters,
            routingByte: 0,
            playerCount: 0,
            sequenceAck: 0,
            consistencyAck: 0,
            quitterBytes: 2,
            baseSequence: 0,
            baseConsistency: 0,
            commandTics: 0,
            consistencyTics: 0,
            stabilityBuffer: (byte)NetConstants.StabilityTics,
            bodyBytes: 0);

        Assert.True(ServerSnapshotApplySession.TryApply(
            header,
            new byte[] { 2 },
            Array.Empty<ServerSnapshotPlayerRecord>(),
            remoteGameTic: 1,
            recipientClientSlot: 1,
            routing,
            registry,
            peerSlots,
            sink: null,
            nowMs: 0,
            out _,
            out _));

        Assert.False(peerSlots.IsConnected(2));
    }
}

public class ClientInputApplySessionTests
{
    private sealed class RecordingInputSink : IClientInputCommandSink
    {
        public List<int> Sequences { get; } = new();

        public bool ApplyCommand(int clientSlot, byte playerNum, int sequence, UserCmd command, ReadOnlyMemory<byte> eventRecords)
        {
            Sequences.Add(sequence);
            return true;
        }
    }

    [Fact]
    public void Apply_AdvancesClientSequence()
    {
        var registry = new LivePeerNetRegistry(maxClients: 4);
        var sink = new RecordingInputSink();
        var routing = new LivePeerRoutingState(0, 4, 0, isLocalAuthority: true, usesHcdeService: true);
        var players = new[]
        {
            new ClientInputPlayerRecord
            {
                PlayerNum = 1,
                Commands = new[]
                {
                    new ClientInputCommandRecord
                    {
                        CommandOffset = 0,
                        Command = new UserCmd(1, 0, 90, 0, 0, 0, 0),
                    },
                },
            },
        };

        var header = new ClientInputHeader(
            controlFlags: 0,
            routingByte: 0,
            playerCount: 1,
            sequenceAck: 0,
            consistencyAck: 0,
            baseSequence: 1,
            baseConsistency: 0,
            commandTics: 1,
            consistencyTics: 0,
            stabilityBuffer: (byte)NetConstants.StabilityTics,
            bodyBytes: 1);

        Assert.True(ClientInputApplySession.TryApply(
            header,
            players,
            clientSlot: 1,
            routing,
            registry,
            sink,
            gameTic: 100,
            out var result,
            out _));

        Assert.Equal(1, result.CommandsApplied);
        Assert.Equal(new[] { 1 }, sink.Sequences);
        Assert.Equal(1, registry[1].CurrentSequence);
    }

    [Fact]
    public void Apply_RejectsUnauthorizedPlayerRecord()
    {
        var registry = new LivePeerNetRegistry(maxClients: 4);
        var routing = new LivePeerRoutingState(0, 4, 0, isLocalAuthority: true, usesHcdeService: true);
        var players = new[]
        {
            new ClientInputPlayerRecord
            {
                PlayerNum = 2,
                Commands = Array.Empty<ClientInputCommandRecord>(),
            },
        };

        var header = new ClientInputHeader(
            0, 0, playerCount: 1, sequenceAck: 0, consistencyAck: 0,
            baseSequence: 1, baseConsistency: 0, commandTics: 0, consistencyTics: 0,
            stabilityBuffer: (byte)NetConstants.StabilityTics, bodyBytes: 1);

        Assert.False(ClientInputApplySession.TryApply(
            header,
            players,
            clientSlot: 1,
            routing,
            registry,
            sink: null,
            gameTic: 1,
            out _,
            out var rejectReason));

        Assert.Equal("client-input-unauthorized-player-record", rejectReason);
    }
}

public class LivePeerNetRegistryTests
{
    [Fact]
    public void ResetClient_ClearsSequenceState()
    {
        var registry = new LivePeerNetRegistry(4);
        registry[2].CurrentSequence = 99;
        registry.ResetClient(2);
        Assert.Equal(0, registry[2].CurrentSequence);
    }
}
