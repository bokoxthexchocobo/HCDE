using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RewindBracketTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BracketRestoresLiveWorldQueuesAndSoundsWithoutPublishingHistoricalState(bool laterBot)
    {
        var sim = Room(); var bot = sim.AddBot(100, 0); bot.Brain!.Enabled = false;
        sim.Rewind.Capture(sim);
        for (var i = 0; i < 6; i++) sim.Tick();
        bot.Health = 7;
        if (laterBot) { var added = sim.AddBot(300, 0); added.Brain!.Enabled = false; }
        sim.Rewind.TryRecordClientViewTic(0, 0);
        sim.QueueCommand(0, new PlayerCommand { YawDelta = 16384 });
        sim.QueueCommand(0, new PlayerCommand { YawDelta = -8192 });
        sim.QueueSoundRequest(new(sim.Players.Single().Id, "pending", 1, 0, 1, false, 1, false));
        var sounds = sim.PendingSoundRequests.ToArray(); var before = Save(sim);
        var bytes = sim.Rewind.TotalBytes; var count = sim.Rewind.Count;
        var restored = 0; var destroyed = 0; var spawned = 0;
        sim.StateRestored += () => restored++; sim.ActorDestroyed += _ => destroyed++; sim.WorldThingSpawned += _ => spawned++;

        Assert.True(sim.Rewind.TestLagCompBracket(sim, 0));

        Assert.Equal(before, Save(sim)); Assert.Equal(sounds, sim.PendingSoundRequests);
        Assert.Equal(2, sim.Players.Single().BufferedCommandCount);
        Assert.Equal(0, restored); Assert.Equal(0, destroyed); Assert.Equal(0, spawned);
        Assert.Equal(bytes, sim.Rewind.TotalBytes); Assert.Equal(count, sim.Rewind.Count);
        Assert.Equal(0, sim.Rewind.GetClientViewTic(0));
        Assert.Equal(1, sim.Rewind.LagCompBracketTests); Assert.Equal(0, sim.Rewind.LagCompBracketFailures);
        sim.Tick(); Assert.Equal(90, sim.Players.Single().Angle.ToDegrees());
        sim.Tick(); Assert.Equal(45, sim.Players.Single().Angle.ToDegrees());
        sim.RestoreState(sim.CaptureState(true)); Assert.Equal(1, restored);
    }

    [Fact]
    public void BracketPreservesCaptureTimerRatherThanUsingHistoricalOrLiveTick()
    {
        var sim = Room(); sim.RewindEnabled = true; sim.Rewind.Capture(sim);
        for (var i = 0; i < 10; i++) sim.Tick();
        sim.Rewind.Capture(sim);
        for (var i = 0; i < 10; i++) sim.Tick();
        sim.Rewind.TryRecordClientViewTic(0, 0);
        Assert.True(sim.Rewind.TestLagCompBracket(sim, 0)); Assert.Equal(20, sim.Thinkers.Clock.Tic);
        sim.Thinkers.Clock.Restore(44); Assert.False(sim.Rewind.CaptureIfDue(sim));
        sim.Thinkers.Clock.Restore(45); Assert.True(sim.Rewind.CaptureIfDue(sim));
        Assert.Equal(3, sim.Rewind.Count);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void MissingViewOrHistoryRejectsWithoutMutation(bool view, bool history)
    {
        var sim = Room();
        if (view) sim.Rewind.TryRecordClientViewTic(0, 0);
        if (history) sim.Rewind.Capture(sim);
        var before = Save(sim);
        Assert.False(sim.Rewind.TestLagCompBracket(sim, 0)); Assert.Equal(before, Save(sim));
        Assert.Equal(1, sim.Rewind.LagCompBracketTests); Assert.Equal(1, sim.Rewind.LagCompBracketFailures);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void InvalidClientRejectsWithoutChangingWorld(int slot)
    {
        var sim = Room(); sim.Rewind.Capture(sim); var before = Save(sim);
        Assert.False(sim.Rewind.TestLagCompBracket(sim, slot)); Assert.Equal(before, Save(sim));
        Assert.Equal(1, sim.Rewind.LagCompBracketFailures);
    }

    [Fact]
    public void FailedCurrentCaptureSurfacesErrorAndAllowsRetry()
    {
        var sim = Room(); sim.Rewind.Capture(sim); sim.Rewind.TryRecordClientViewTic(0, 0);
        var before = Save(sim); var unsupported = new Thinker(); sim.Thinkers.Add(unsupported);
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        Assert.Throws<InvalidOperationException>(() => sim.Rewind.TestLagCompBracket(sim, 0));
        Assert.Equal(1, sim.Rewind.LagCompBracketFailures); Assert.Equal(1, sim.Players.Single().BufferedCommandCount);
        sim.Thinkers.Remove(unsupported);
        Assert.Equal(before, Save(sim)); Assert.True(sim.Rewind.TestLagCompBracket(sim, 0));
        Assert.Equal(2, sim.Rewind.LagCompBracketTests); Assert.Equal(1, sim.Rewind.LagCompBracketFailures);
    }

    [Fact]
    public void UnsupportedHistoricalRestoreRollsBackAndRestoresNotificationMode()
    {
        var sim = Room(); sim.Rewind.Capture(sim); sim.Rewind.TryRecordClientViewTic(0, 0);
        sim.Players.Single().Destroy(); sim.Tick(); var before = Save(sim);
        var notifications = 0; sim.StateRestored += () => notifications++;
        Assert.Throws<InvalidOperationException>(() => sim.Rewind.TestLagCompBracket(sim, 0));
        Assert.Equal(before, Save(sim)); Assert.Equal(0, notifications);
        Assert.Equal(1, sim.Rewind.LagCompBracketFailures); Assert.Equal(1, sim.Rewind.Count);
        sim.RestoreState(sim.CaptureState(true)); Assert.Equal(1, notifications);
        sim.Rewind.Reset(); sim.Rewind.Capture(sim); sim.Rewind.TryRecordClientViewTic(0, 1);
        Assert.True(sim.Rewind.TestLagCompBracket(sim, 0));
    }

    [Fact]
    public void NormalRestoreStillClearsTransientWorkAndNotifiesListeners()
    {
        var sim = Room(); var state = sim.CaptureState(true); var notifications = 0;
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.QueueSoundRequest(new(sim.Players.Single().Id, "pending", 1, 0, 1, false, 1, false));
        sim.StateRestored += () => notifications++;
        sim.RestoreState(state);
        Assert.Equal(0, sim.Players.Single().BufferedCommandCount); Assert.Empty(sim.PendingSoundRequests);
        Assert.Equal(1, notifications);
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, includeBotIdentities: true);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
