using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InvasionRestoreTests
{
    [Theory]
    [InlineData(InvasionPhase.Waiting)]
    [InlineData(InvasionPhase.Countdown)]
    [InlineData(InvasionPhase.Wave)]
    [InlineData(InvasionPhase.Intermission)]
    [InlineData(InvasionPhase.Victory)]
    public void EveryPhaseReplaysWaveProgressionAndActorIds(InvasionPhase phase)
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Configure(2, 2, 3);
        for (var i = 0; i < 30 && sim.Invasion.Phase != phase; i++) Step(sim);
        Assert.Equal(phase, sim.Invasion.Phase);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        Assert.Equal(phase is InvasionPhase.Intermission or InvasionPhase.Victory ? 311 : 216,
            BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray());
    }

    [Fact]
    public void RewindingLaterChildRemovesItsWaveMembershipBeforeIdReuse()
    {
        var (sim, parent) = Wave(); var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var child = sim.SpawnLostSoul(parent, null, 0)!; Assert.Equal(2, sim.Invasion.Spawned); sim.Tick();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(1, sim.Invasion.Spawned); Assert.Equal(1, sim.Invasion.ActiveMonsters);
        Assert.DoesNotContain(sim.Actors, actor => actor.Id == child.Id);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(child.Id, sim.SpawnLostSoul(parent, null, 0)!.Id);
        Assert.Equal(2, sim.Invasion.Spawned); sim.Tick(); Assert.Equal(2, sim.Invasion.ActiveMonsters);
    }

    [Fact]
    public void ReconstructedChildRestoresMembershipWithoutDoubleRegistration()
    {
        var (sim, parent) = Wave(); var child = sim.SpawnLostSoul(parent, null, 0)!;
        Assert.Equal(2, sim.Invasion.Spawned); Assert.Equal(1, sim.Invasion.ActiveMonsters);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        child.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(2, sim.Invasion.Spawned); Assert.Equal(1, sim.Invasion.ActiveMonsters);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        sim.Tick(); Assert.Equal(2, sim.Invasion.Spawned); Assert.Equal(2, sim.Invasion.ActiveMonsters);
    }

    [Fact]
    public void DisabledWavePreservesConfigurationAndPausedCounters()
    {
        var (sim, parent) = Wave(); sim.Invasion.Enabled = false;
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        sim.Invasion.Enabled = true; parent.Health = 0; sim.Tick();
        SimSavegame.Apply(sim, bytes);
        Assert.False(sim.Invasion.Enabled); Assert.Equal(InvasionPhase.Wave, sim.Invasion.Phase);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(InvasionPhase.Wave, sim.Invasion.Phase); Assert.Equal(1, sim.Invasion.ActiveMonsters);
    }

    [Fact]
    public void DefaultIdentitySnapshotClearsLaterDirectorState()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        sim.Invasion.Enabled = true; sim.Invasion.Configure(2, 0, 3); sim.Tick();
        SimSavegame.Apply(sim, bytes);
        Assert.False(sim.Invasion.Enabled); Assert.Equal(InvasionPhase.Waiting, sim.Invasion.Phase);
        Assert.Equal(0, sim.Invasion.Spawned); Assert.Equal(InvasionDirector.WaveGapTics, sim.Invasion.IntermissionTics);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void DefaultArchivesKeepExistingDirectorBehavior()
    {
        var (sim, _) = Wave(); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Null(state.Invasion); Assert.False(state.IncludesBotIdentities);
        sim.Invasion.Enabled = false; SimSavegame.Apply(sim, bytes);
        Assert.False(sim.Invasion.Enabled); Assert.Equal(1, sim.Invasion.Wave);
        Assert.Equal(bytes, SimSavegame.Write(state));
    }

    [Fact]
    public void HistoricalRemovedMemberIdsSurviveIntermissionRestore()
    {
        var (sim, parent) = Wave(); parent.Destroy(); sim.Tick();
        Assert.Equal(InvasionPhase.Intermission, sim.Invasion.Phase);
        Assert.DoesNotContain(sim.Actors, actor => actor.Id == parent.Id);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var expected = Enumerable.Range(0, 6).Select(_ => Step(sim)).ToArray();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(new[] { parent.Id }, sim.Invasion.CaptureState()!.WaveActors);
        Assert.Equal(bytes, SimSavegame.Write(sim, includeBotIdentities: true));
        Assert.Equal(expected, Enumerable.Range(0, 6).Select(_ => Step(sim)).ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void InconsistentPhaseCountersAreRejectedBeforeMutation(int kind)
    {
        var (sim, _) = Wave(); var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true); var saved = state.Invasion!;
        state.Invasion = kind switch
        {
            0 => saved with { Phase = InvasionPhase.Waiting },
            1 => saved with { Phase = InvasionPhase.Countdown },
            2 => saved with { Phase = InvasionPhase.Victory },
            _ => saved with { Cooldown = 1 }
        };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Fact]
    public void InvasionArchiveRequiresIdentitySnapshot()
    {
        var (sim, _) = Wave(); var state = sim.CaptureState(); state.Invasion = sim.Invasion.CaptureState();
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void InvalidInvasionStateIsRejectedBeforeMutation(int kind)
    {
        var (sim, parent) = Wave(); var before = SimSavegame.Write(sim, includeBotIdentities: true);
        var state = sim.CaptureState(includeBotIdentities: true); var saved = state.Invasion!;
        state.Invasion = kind switch
        {
            0 => saved with { Phase = (InvasionPhase)99 },
            1 => saved with { Wave = -1 },
            2 => saved with { ActiveMonsters = 2 },
            3 => saved with { Spawned = 2, WaveActors = new[] { parent.Id, parent.Id } },
            4 => saved with { WaveActors = new uint[] { 0 } },
            5 => saved with { WaveActors = new[] { state.NextActorId!.Value } },
            _ => saved with { Spawned = 0 }
        };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, SimSavegame.Write(sim, includeBotIdentities: true));
    }

    [Theory]
    [InlineData(0, 215, "save-invasion-header")]
    [InlineData(4, 2, "save-invasion-value")]
    [InlineData(8, 99, "save-invasion-state")]
    [InlineData(20, -1, "save-invasion-state")]
    [InlineData(48, 0, "save-invasion-state")]
    public void InvalidInvasionArchiveFieldsAreRejected(int offset, int value, string expectedError)
    {
        var (sim, _) = Wave(); var bytes = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expectedError, error);
    }

    private static (InvasionPhase, int, int, int, int, int, string) Step(AuthoritySimulation sim)
    {
        foreach (var bot in sim.Actors.OfType<BotPawn>())
        { bot.Brain!.Enabled = false; if (!bot.IsDead) bot.Health = 0; }
        sim.Tick();
        return (sim.Invasion.Phase, sim.Invasion.Wave, sim.Invasion.Cooldown, sim.Invasion.Spawned,
            sim.Invasion.ActiveMonsters, sim.Invasion.Cleared,
            string.Join(',', sim.Actors.OfType<BotPawn>().Where(actor => !actor.IsDead).Select(actor => actor.Id)));
    }

    private static (AuthoritySimulation, BotPawn) Wave()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Configure(2, 0, 3); sim.Tick();
        var parent = Assert.Single(sim.Actors.OfType<BotPawn>()); parent.Brain!.Enabled = false;
        return (sim, parent);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
