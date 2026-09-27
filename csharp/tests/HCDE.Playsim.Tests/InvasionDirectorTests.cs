using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InvasionDirectorTests
{
    [Fact]
    public void LivingEnemiesPreventTimerFromStartingAnotherWave()
    {
        var sim = Start();
        sim.Tick();
        for (var i = 0; i < 200; i++) sim.Tick();
        Assert.Equal(1, sim.Invasion.Wave);
        Assert.Equal(1, sim.Invasion.ActiveMonsters);
        Assert.Equal(0, sim.Invasion.Cleared);
        Assert.Equal(0, sim.Invasion.Cooldown);
        Assert.Single(sim.Actors.OfType<BotPawn>());
    }

    [Fact]
    public void LastKillStartsExactlyOneIntermissionThenNextWave()
    {
        var sim = Start();
        sim.Tick();
        var bot = Assert.Single(sim.Actors.OfType<BotPawn>());
        // Verify these enemies can actually be killed through the combat path.
        for (var i = 0; i < 6 && !bot.IsDead; i++)
        {
            while (sim.Players.Single().WeaponCooldown > 1) sim.Tick();
            sim.QueueCommand(0, new PlayerCommand { Attack = true });
            sim.Tick();
        }
        Assert.True(bot.IsDead);
        Assert.Equal(InvasionPhase.Intermission, sim.Invasion.Phase);
        Assert.Equal(1, sim.Invasion.Cleared);
        Assert.Equal(0, sim.Invasion.ActiveMonsters);
        Assert.Equal(35, sim.Invasion.Cooldown);
        for (var i = 0; i < 34; i++) sim.Tick();
        Assert.Equal(1, sim.Invasion.Wave);
        Assert.Equal(1, sim.Invasion.Cooldown);
        sim.Tick();
        Assert.Equal(2, sim.Invasion.Wave);
        Assert.Equal(0, sim.Invasion.Cooldown);
        Assert.Equal(1, sim.Invasion.Spawned);
        Assert.Equal(1, sim.Invasion.ActiveMonsters);
        Assert.Equal(0, sim.Invasion.Cleared);
    }

    [Fact]
    public void OnlyCurrentWaveActorsAffectClearCount()
    {
        var sim = Start(twoSpots: true);
        var unrelated = sim.AddBot(600, 600);
        sim.Tick();
        var wave = sim.Actors.OfType<BotPawn>().Where(b => b != unrelated).ToArray();
        Assert.Equal(2, sim.Invasion.Spawned);
        wave[0].Health = 0;
        sim.Tick();
        Assert.Equal(1, sim.Invasion.Cleared);
        Assert.Equal(1, sim.Invasion.ActiveMonsters);
        Assert.Equal(InvasionPhase.Wave, sim.Invasion.Phase);
        wave[1].Destroy();
        sim.Tick();
        Assert.Equal(InvasionPhase.Intermission, sim.Invasion.Phase);
        Assert.Equal(2, sim.Invasion.Cleared);
        Assert.False(unrelated.IsDead);
    }

    [Fact]
    public void ZombiemanIsAnEnemyAndMapSpotIsOnlyAMarker()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = new[]
            {
                new LevelThing { Type = 3004, X = 200, Y = 64 },
                new LevelThing { Type = 9001, X = 300, Y = 64 },
            },
        });
        Assert.True(sim.Actors[0].BlocksActors);
        Assert.False(sim.Actors[1].BlocksActors);
        sim.Invasion.Enabled = true;
        sim.Tick();
        var bot = Assert.Single(sim.Actors.OfType<BotPawn>());
        Assert.Equal(300, bot.X.ToDouble());
        Assert.True(bot.BlocksActors);
    }

    [Fact]
    public void DisabledAndExitedSimulationsDoNotAdvanceInvasion()
    {
        var sim = Start();
        sim.Invasion.Enabled = false;
        sim.Tick();
        Assert.Equal(0, sim.Invasion.Wave);
        sim.Invasion.Enabled = true;
        sim.Tick();
        sim.Actors.OfType<BotPawn>().Single().Health = 0;
        sim.Tick();
        sim.Invasion.Enabled = false;
        sim.Tick();
        Assert.Equal(35, sim.Invasion.Cooldown);
        sim.Invasion.Enabled = true;
        sim.MarkExited(false);
        sim.Tick();
        Assert.Equal(35, sim.Invasion.Cooldown);
    }

    private static AuthoritySimulation Start(bool twoSpots = false)
    {
        var things = new List<LevelThing>
        {
            new() { Type = 1, X = 32, Y = 64 },
            new() { Type = 9001, X = 200, Y = 64 },
        };
        if (twoSpots) things.Add(new LevelThing { Type = 9001, X = 400, Y = 200 });
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = things });
        sim.Invasion.Enabled = true;
        return sim;
    }
}
