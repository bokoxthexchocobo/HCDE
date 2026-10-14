using HCDE.MapLoader;
using HCDE.Net.Core;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class ClientViewAcknowledgmentTests
{
    [Theory]
    [InlineData(0u, 1, 0)]
    [InlineData(17u, 1, 17)]
    [InlineData(17u, 2, 34)]
    [InlineData(17u, 3, 51)]
    public void AcceptedInputRecordsAuthenticatedSlotAndScalesSequence(uint ack, int ticDup, int expected)
    {
        var sim = Room(); var registry = new LivePeerNetRegistry(4);
        var sink = new SimulationCommandSink(sim, ticDup);
        Assert.True(Apply(Header(ack), [Record()], 1, registry, sink, out var result));
        Assert.True(result.ViewTicRecorded); Assert.Equal(1, result.CommandsApplied);
        Assert.Equal(expected, sim.Rewind.GetClientViewTic(1));
        Assert.Equal(-1, sim.Rewind.GetClientViewTic(0));
        Assert.Equal(1, sim.Players.Single().BufferedCommandCount);
    }

    [Theory]
    [InlineData(-1, 1, true)]
    [InlineData(4, 1, true)]
    [InlineData(1, 2, true)]
    [InlineData(1, 1, false)]
    public void RejectedInputCannotChangePreviouslyRecordedView(int slot, byte player, bool authority)
    {
        var sim = Room(); Assert.True(sim.Rewind.TryRecordClientViewTic(1, 7));
        Assert.False(Apply(Header(99), [Record(player)], slot, new LivePeerNetRegistry(4),
            new SimulationCommandSink(sim), out var result, authority));
        Assert.False(result.ViewTicRecorded); Assert.Equal(7, sim.Rewind.GetClientViewTic(1));
        Assert.Equal(0, sim.Players.Single().BufferedCommandCount);
    }

    [Fact]
    public void MismatchedPlayerCountCannotChangeView()
    {
        var sim = Room(); var sink = new SimulationCommandSink(sim);
        Assert.False(Apply(Header(99), [], 1, new LivePeerNetRegistry(4), sink, out var result));
        Assert.False(result.ViewTicRecorded); Assert.Equal(-1, sim.Rewind.GetClientViewTic(1));
    }

    [Fact]
    public void AcceptedAcknowledgmentWithoutCommandsStillUpdatesView()
    {
        var sim = Room(); var header = new ClientInputHeader(0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0);
        Assert.True(Apply(header, [], 1, new LivePeerNetRegistry(4), new SimulationCommandSink(sim), out var result));
        Assert.True(result.ViewTicRecorded); Assert.Equal(0, result.CommandsApplied);
        Assert.Equal(12, sim.Rewind.GetClientViewTic(1));
    }

    [Theory]
    [InlineData(uint.MaxValue, 1)]
    [InlineData(1073741824u, 2)]
    public void UnrepresentableViewDoesNotRejectCommandsOrOverwritePriorObservation(uint ack, int ticDup)
    {
        var sim = Room(); Assert.True(sim.Rewind.TryRecordClientViewTic(1, 7));
        Assert.True(Apply(Header(ack), [Record()], 1, new LivePeerNetRegistry(4),
            new SimulationCommandSink(sim, ticDup), out var result));
        Assert.False(result.ViewTicRecorded); Assert.Equal(1, result.CommandsApplied);
        Assert.Equal(7, sim.Rewind.GetClientViewTic(1));
    }

    [Fact]
    public void FullCommandQueueDoesNotDiscardAnAcceptedViewAcknowledgment()
    {
        var sim = Room();
        for (var i = 0; i < PlayerPawn.CommandQueueCapacity; i++) Assert.True(sim.QueueCommand(1, default));
        Assert.True(Apply(Header(19), [Record()], 1, new LivePeerNetRegistry(4), new SimulationCommandSink(sim), out var result));
        Assert.Equal(0, result.CommandsApplied); Assert.True(result.ViewTicRecorded);
        Assert.Equal(19, sim.Rewind.GetClientViewTic(1));
    }

    [Fact]
    public void AcceptedAcknowledgmentFeedsHistoricalSelectionAndDisableInvalidatesIt()
    {
        var sim = Room(); sim.Rewind.Capture(sim);
        for (var i = 0; i < 4; i++) sim.Tick();
        sim.Rewind.Capture(sim); var expected = SimSavegame.Write(sim, true);
        for (var i = 0; i < 4; i++) sim.Tick();
        sim.Rewind.Capture(sim);
        Assert.True(Apply(Header(3), [Record()], 1, new LivePeerNetRegistry(4), new SimulationCommandSink(sim, 2), out var result));
        Assert.True(result.ViewTicRecorded);
        Assert.True(sim.Rewind.RestoreAtOrBefore(sim, sim.Rewind.GetClientViewTic(1)));
        Assert.Equal(expected, SimSavegame.Write(sim, true));
        sim.RewindEnabled = false; Assert.Equal(-1, sim.Rewind.GetClientViewTic(1));
    }

    [Fact]
    public void ExistingCommandOnlySinksRemainCompatible()
    {
        Assert.True(Apply(Header(12), [Record()], 1, new LivePeerNetRegistry(4), new CommandOnlySink(), out var result));
        Assert.Equal(1, result.CommandsApplied); Assert.False(result.ViewTicRecorded);
    }

    private sealed class CommandOnlySink : IClientInputCommandSink
    {
        public bool ApplyCommand(int clientSlot, byte playerNum, int sequence, UserCmd command, ReadOnlyMemory<byte> eventRecords) => true;
    }
    private static bool Apply(ClientInputHeader header, IReadOnlyList<ClientInputPlayerRecord> records, int slot,
        LivePeerNetRegistry registry, IClientInputCommandSink sink, out ClientInputApplyResult result, bool authority = true) =>
        ClientInputApplySession.TryApply(header, records, slot,
            new LivePeerRoutingState(0, 4, 0, isLocalAuthority: authority, usesHcdeService: true),
            registry, sink, 0, out result, out _);
    private static ClientInputHeader Header(uint ack) => new(0, 0, 1, ack, 0, 1, 0, 1, 0, 0, 1);
    private static ClientInputPlayerRecord Record(byte player = 1) => new()
    {
        PlayerNum = player,
        Commands = [new ClientInputCommandRecord { CommandOffset = 0, Command = default }]
    };
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 2, X = 800 }]
    });
}
