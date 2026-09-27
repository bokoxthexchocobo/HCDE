using HCDE.MapLoader;
using HCDE.Net.Core;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class CommandBufferTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 2 }],
    });

    [Fact]
    public void BurstRetainsOrderAndExecutesOneCommandPerTic()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(sim.QueueCommand(1, new PlayerCommand { YawDelta = 16384, Attack = true }));
        Assert.True(sim.QueueCommand(1, new PlayerCommand { YawDelta = -16384 }));
        Assert.Equal(2, player.BufferedCommandCount);
        sim.Tick();
        Assert.Equal(90, player.Angle.ToDegrees()); Assert.Equal(49, player.Inventory.Bullets);
        Assert.Equal(1, player.BufferedCommandCount);
        sim.Tick();
        Assert.Equal(0, player.Angle.ToDegrees()); Assert.Equal(49, player.Inventory.Bullets);
        Assert.Equal(0, player.BufferedCommandCount);
    }

    [Fact]
    public void FullQueueDoesNotAcknowledgeRejectedCommandAndRetryIsIdempotent()
    {
        var sim = Room(); var player = sim.Players.Single();
        for (var i = 0; i < PlayerPawn.CommandQueueCapacity; i++) Assert.True(sim.QueueCommand(1, default));
        var registry = new LivePeerNetRegistry(4);
        var routing = new LivePeerRoutingState(0, 4, 0, isLocalAuthority: true, usesHcdeService: true);
        var header = new ClientInputHeader(0, 0, 1, sequenceAck: 0, consistencyAck: 0,
            baseSequence: 1, baseConsistency: 0, commandTics: 1, consistencyTics: 0, stabilityBuffer: 0, bodyBytes: 1);
        ClientInputPlayerRecord[] records = [new() { PlayerNum = 1, Commands = [new ClientInputCommandRecord {
            CommandOffset = 0, Command = new UserCmd(0, 0, 123, 0, 0, 0, 0),
        }] }];
        var sink = new SimulationCommandSink(sim);
        Assert.True(ClientInputApplySession.TryApply(header, records, 1, routing, registry, sink, 0, out var full, out _));
        Assert.Equal(0, full.CommandsApplied); Assert.Equal(0, registry[1].CurrentSequence);
        sim.Tick();
        Assert.True(ClientInputApplySession.TryApply(header, records, 1, routing, registry, sink, 1, out var accepted, out _));
        Assert.Equal(1, accepted.CommandsApplied); Assert.Equal(1, registry[1].CurrentSequence);
        Assert.True(ClientInputApplySession.TryApply(header, records, 1, routing, registry, sink, 1, out var duplicate, out _));
        Assert.Equal(0, duplicate.CommandsApplied);
        Assert.Equal(PlayerPawn.CommandQueueCapacity, player.BufferedCommandCount);
        for (var tick = 0; tick < PlayerPawn.CommandQueueCapacity; tick++) sim.Tick();
        Assert.Equal((uint)(123 << 16), player.Angle.Raw);
    }

    [Fact]
    public void RestoreAndDeathClearBufferedActions()
    {
        var sim = Room(); var player = sim.Players.Single(); var saved = sim.CaptureState();
        sim.QueueCommand(1, new PlayerCommand { Attack = true });
        sim.RestoreState(saved);
        Assert.Equal(0, player.BufferedCommandCount);
        Assert.True(sim.QueueCommand(1, new PlayerCommand { YawDelta = 16384 }));
        sim.Tick(); Assert.Equal(90, player.Angle.ToDegrees()); // No inserted empty command after restore.
        sim.QueueCommand(1, new PlayerCommand { Attack = true });
        player.Health = 0;
        Assert.Equal(0, player.BufferedCommandCount);
        Assert.True(sim.QueueCommand(1, new PlayerCommand { Attack = true }));
        player.Health = 100; sim.Tick();
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.False(sim.QueueCommand(3, default));
    }
}
