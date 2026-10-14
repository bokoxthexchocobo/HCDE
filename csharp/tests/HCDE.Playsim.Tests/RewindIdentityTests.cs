using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RewindIdentityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void KeyframeReconstructsBotsAndRewindsMembershipOrderAndAllocator(bool established)
    {
        var sim = Room(); var first = sim.AddBot(100, 0); var second = sim.AddBot(300, 0);
        first.Brain!.Enabled = second.Brain!.Enabled = false;
        if (established) sim.Tick();
        sim.Thinkers.ChangeStatNum(first, first.StatNum);
        sim.Rewind.Capture(sim); var before = Save(sim);
        second.Destroy(); sim.Tick();
        var later = sim.AddBot(500, 0); var next = later.Id;
        var notifications = new List<uint>(); sim.WorldThingSpawned += actor => notifications.Add(actor.Id);

        Assert.True(sim.Rewind.RestoreOldest(sim));

        Assert.True(later.Destroyed); Assert.DoesNotContain(later, sim.Actors);
        Assert.Equal(before, Save(sim)); Assert.Empty(notifications);
        sim.Tick();
        Assert.Equal(new[] { second.Id, first.Id }, sim.Thinkers.ThinkersIn(ThinkerStat.Default).Cast<Actor>().Select(actor => actor.Id));
        Assert.Equal(established ? Array.Empty<uint>() : new[] { sim.Players.Single().Id, second.Id, first.Id }, notifications);
        Assert.Equal(next, sim.AddBot(500, 0).Id);
    }

    [Fact]
    public void AutomaticCaptureReplaysActiveBrainsAndProjectilesAfterReconstruction()
    {
        var sim = Room(); var first = sim.AddBot(0, 200, 3004); var second = sim.AddBot(0, -200, 3001);
        sim.Thinkers.ChangeStatNum(first, first.StatNum);
        sim.Players.Single().Invulnerable = true;
        sim.SpawnProjectile(second, ProjectileKind.Plasma);
        sim.RewindEnabled = true; sim.Tick(); sim.Rewind.CaptureIntervalSeconds = 30;
        var captured = Save(sim);
        Assert.Equal(1, sim.Rewind.Count);
        var expected = Enumerable.Range(0, 40).Select(_ => Advance(sim)).ToArray();
        first.Destroy(); second.Destroy(); sim.Tick();

        Assert.True(sim.Rewind.RestoreOldest(sim));

        Assert.Equal(captured, Save(sim));
        Assert.Equal(expected, Enumerable.Range(0, 40).Select(_ => Advance(sim)).ToArray());
        Assert.Equal(1, sim.Rewind.Count);
    }

    [Fact]
    public void RewindRestoresDamageHistoryWithoutReplayingDeath()
    {
        var sim = Room(); var victim = sim.AddBot(100, 0, 3001); var source = sim.AddBot(300, 0, 3004);
        victim.Brain!.Enabled = source.Brain!.Enabled = false;
        ActorDamage.Apply(victim, 1000, source, inflictor: source);
        sim.Rewind.Capture(sim); var captured = Save(sim);
        ActorRaise.ReviveSupported(victim); ActorDamage.Apply(victim, 1000, source, inflictor: source);
        Assert.Equal(2, victim.DeathCount);

        Assert.True(sim.Rewind.RestoreOldest(sim));

        Assert.Equal(1, victim.DeathCount); Assert.Equal(source.Id, victim.LastDamageSourceId);
        Assert.Equal(captured, Save(sim)); sim.Tick(); Assert.Equal(1, victim.DeathCount);
    }

    [Fact]
    public void RewindRestoresInvasionMembershipAndRemovesLaterChildren()
    {
        var sim = Room(); sim.Invasion.Enabled = true; sim.Invasion.Configure(2, 0, 3); sim.Tick();
        var parent = Assert.Single(sim.Actors.OfType<BotPawn>()); parent.Brain!.Enabled = false;
        sim.Rewind.Capture(sim); var captured = Save(sim);
        var child = sim.SpawnLostSoul(parent, null, 0)!;
        Assert.NotNull(child); Assert.Equal(2, sim.Invasion.Spawned);
        parent.Destroy(); sim.Tick();

        Assert.True(sim.Rewind.RestoreOldest(sim));

        Assert.True(child.Destroyed); Assert.Equal(1, sim.Invasion.Spawned);
        Assert.Equal(new[] { parent.Id }, sim.Invasion.CaptureState()!.WaveActors);
        Assert.Equal(captured, Save(sim));
        var restoredParent = sim.Actors.Single(actor => actor.Id == parent.Id);
        Assert.Equal(child.Id, sim.SpawnLostSoul(restoredParent, null, 0)!.Id);
    }

    [Fact]
    public void RingEvictsOnlyTheOldestFrameAndRestoresRetainedIdentityState()
    {
        var sim = Room(); var bot = sim.AddBot(100, 0); bot.Brain!.Enabled = false;
        var rewind = new RewindBuffer { Capacity = 2 };
        rewind.Capture(sim);
        sim.Tick(); rewind.Capture(sim); var expected = Save(sim);
        sim.Tick(); rewind.Capture(sim); bot.Destroy(); sim.Tick();

        Assert.True(rewind.RestoreOldest(sim));

        Assert.Equal(2, rewind.Count); Assert.Equal(1, sim.Thinkers.Clock.Tic);
        Assert.Equal(expected, Save(sim));
        Assert.True(rewind.RestoreOldest(sim)); Assert.Equal(expected, Save(sim));
    }

    [Fact]
    public void UnsupportedCaptureDoesNotEvictAnExistingKeyframe()
    {
        var sim = Room(); var rewind = new RewindBuffer { Capacity = 1 };
        rewind.Capture(sim); var before = Save(sim);
        var unsupported = new Thinker(); sim.Thinkers.Add(unsupported);
        Assert.Throws<InvalidOperationException>(() => rewind.Capture(sim)); Assert.Equal(1, rewind.Count);
        sim.Thinkers.Remove(unsupported); sim.Tick();
        Assert.True(rewind.RestoreOldest(sim)); Assert.Equal(before, Save(sim));
    }

    [Fact]
    public void UnsupportedRestoreFailsBeforeMutationAndRetainsTheFrame()
    {
        var sim = Room(); sim.Rewind.Capture(sim);
        sim.Players.Single().Destroy(); sim.Tick(); var before = Save(sim);
        Assert.Throws<InvalidOperationException>(() => sim.Rewind.RestoreOldest(sim));
        Assert.Equal(before, Save(sim)); Assert.Equal(1, sim.Rewind.Count);
    }

    [Fact]
    public void EmptyRingReturnsFalseWithoutChangingSimulation()
    {
        var sim = Room(); var before = Save(sim);
        Assert.False(sim.Rewind.RestoreOldest(sim)); Assert.Equal(before, Save(sim));
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, includeBotIdentities: true);
    private static uint Advance(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
