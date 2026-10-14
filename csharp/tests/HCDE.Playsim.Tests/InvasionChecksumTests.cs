using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InvasionChecksumTests
{
    [Fact]
    public void ExplicitDefaultDirectorStatePreservesExistingChecksum()
    {
        var sim = Room(); var state = sim.CaptureState(includeBotIdentities: true);
        sim.RestoreState(state); var original = sim.Checksum;
        state.Invasion = new(false, InvasionPhase.Waiting, 0, 0, 0, 0, 0, 0,
            InvasionDirector.WaveGapTics, 0, Array.Empty<uint>());
        sim.RestoreState(state);
        Assert.Null(sim.Invasion.CaptureState()); Assert.Equal(original, sim.Checksum);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ConfigurationAndEnabledStateAffectChecksumWithoutActorChanges(int field)
    {
        var sim = Room(); var state = sim.CaptureState(includeBotIdentities: true);
        sim.RestoreState(state); var original = sim.Checksum;
        state.Invasion = field switch
        {
            0 => new(true, InvasionPhase.Waiting, 0, 0, 0, 0, 0, 0, 35, 0, Array.Empty<uint>()),
            1 => new(false, InvasionPhase.Waiting, 0, 0, 0, 0, 3, 0, 35, 0, Array.Empty<uint>()),
            2 => new(false, InvasionPhase.Waiting, 0, 0, 0, 0, 0, 5, 35, 0, Array.Empty<uint>()),
            _ => new(false, InvasionPhase.Waiting, 0, 0, 0, 0, 0, 0, 7, 0, Array.Empty<uint>())
        };
        sim.RestoreState(state);
        Assert.NotEqual(original, sim.Checksum);
        state.Invasion = null; sim.RestoreState(state); Assert.Equal(original, sim.Checksum);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void WaveCountersAndMemberIdsAffectChecksumWithoutActorChanges(int field)
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Configure(3, 0, 4); sim.Tick();
        var first = Assert.Single(sim.Actors.OfType<BotPawn>()); first.Brain!.Enabled = false;
        var second = sim.AddBot(200, 0); second.Brain!.Enabled = false;
        var state = sim.CaptureState(includeBotIdentities: true);
        sim.RestoreState(state); var original = sim.Checksum; var invasion = state.Invasion!;
        state.Invasion = field switch
        {
            0 => invasion with { Wave = 2 },
            1 => invasion with { Spawned = 2, WaveActors = new[] { first.Id, second.Id } },
            2 => invasion with { ActiveMonsters = 0 },
            3 => invasion with { SpawnSpotCount = 1 },
            _ => invasion with { WaveActors = new[] { second.Id } }
        };
        sim.RestoreState(state); Assert.NotEqual(original, sim.Checksum);
        state.Invasion = invasion; sim.RestoreState(state); Assert.Equal(original, sim.Checksum);
    }

    [Fact]
    public void CountdownPhaseAndRemainingTimeParticipateInChecksum()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Configure(2, 5, 3);
        var waiting = sim.CaptureState(includeBotIdentities: true);
        sim.RestoreState(waiting); var initial = sim.Checksum;
        waiting.Invasion = waiting.Invasion! with { Phase = InvasionPhase.Countdown, Cooldown = 5 };
        sim.RestoreState(waiting); var countdown = sim.Checksum;
        Assert.NotEqual(initial, countdown);
        waiting.Invasion = waiting.Invasion with { Cooldown = 4 }; sim.RestoreState(waiting);
        Assert.NotEqual(countdown, sim.Checksum);
    }

    [Fact]
    public void PhaseAloneAffectsChecksumWithIdenticalCounters()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Configure(2, 0, 0); sim.Tick();
        var state = sim.CaptureState(includeBotIdentities: true);
        state.Invasion = state.Invasion! with { ActiveMonsters = 0 };
        sim.RestoreState(state); var wave = sim.Checksum;
        state.Invasion = state.Invasion with { Phase = InvasionPhase.Intermission };
        sim.RestoreState(state); Assert.NotEqual(wave, sim.Checksum);
    }

    [Fact]
    public void MembershipInsertionOrderDoesNotChangeChecksum()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Configure(2, 0, 3); sim.Tick();
        var parent = Assert.Single(sim.Actors.OfType<BotPawn>()); parent.Brain!.Enabled = false;
        var child = sim.SpawnLostSoul(parent, null, 0)!;
        var state = sim.CaptureState(includeBotIdentities: true);
        sim.RestoreState(state); var original = sim.Checksum;
        state.Invasion = state.Invasion! with { WaveActors = new[] { child.Id, parent.Id } };
        sim.RestoreState(state); Assert.Equal(original, sim.Checksum);
    }

    [Fact]
    public void SaveRestoreReplaysInvasionChecksumsAcrossWaves()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Configure(2, 2, 3);
        var bytes = SimSavegame.Write(sim, includeBotIdentities: true);
        var expected = Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray());
    }

    private static uint Step(AuthoritySimulation sim)
    {
        foreach (var bot in sim.Actors.OfType<BotPawn>())
        { bot.Brain!.Enabled = false; if (!bot.IsDead) bot.Health = 0; }
        sim.Tick(); return sim.Checksum;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
