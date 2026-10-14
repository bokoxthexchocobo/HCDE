using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FriendlyEnemySearchTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(int.MaxValue, true)]
    public void FriendlySeeBlocksControlsBlockRingSearch(int range, bool found)
    {
        var sim = Room(); var observer = sim.Actors[0]; var enemy = sim.Actors[1];
        observer.Friendly = true; observer.FriendlySeeBlocks = range;
        Assert.Equal(found ? enemy : null, FriendlyEnemySearch.Find(sim, observer));
    }

    [Theory]
    [InlineData("friendly")]
    [InlineData("dormant")]
    [InlineData("never")]
    [InlineData("invisible")]
    [InlineData("nonmonster")]
    [InlineData("unshootable")]
    [InlineData("noblockmap")]
    public void IneligibleEnemiesAreNotAcquired(string flag)
    {
        var sim = Room(); var observer = sim.Actors[0]; var enemy = sim.Actors[1]; observer.Friendly = true;
        switch (flag)
        {
            case "friendly": enemy.Friendly = true; break;
            case "dormant": enemy.Deactivate(null); break;
            case "never": enemy.NeverTarget = true; break;
            case "invisible": enemy.MVisBlocked = true; break;
            case "nonmonster": enemy.IsMonster = false; break;
            case "unshootable": enemy.Shootable = false; break;
            case "noblockmap": enemy.NoBlockmap = true; break;
        }
        Assert.Null(FriendlyEnemySearch.Find(sim, observer));
    }

    [Fact]
    public void SameCellIsSearchedEvenWithZeroRadiusAndBrainAcquiresEnemy()
    {
        var sim = Room(); var observer = sim.Actors[0]; var enemy = sim.Actors[1]; observer.Friendly = true;
        enemy.X = Fixed.FromInt(96); observer.FriendlySeeBlocks = 0;
        Assert.Same(enemy, FriendlyEnemySearch.Find(sim, observer));
        enemy.Brain!.Enabled = false;
        for (var i = 0; i < 20 && observer.TargetId is null; i++) sim.Tick();
        Assert.Equal(enemy.Id, observer.TargetId);
    }

    [Fact]
    public void SkipTargetStreamAndAcquisitionSequenceSurviveSavedContinuation()
    {
        var sim = Room(); var observer = sim.Actors[0]; var enemy = sim.Actors[1]; var ally = sim.Actors[2];
        observer.Friendly = ally.Friendly = true;
        enemy.SetTargetPointer(ally); ally.SetTargetPointer(enemy);
        var initial = SimSavegame.Write(sim, true);
        Assert.Null(sim.CaptureState().SkipTargetRandomState);
        FriendlyEnemySearch.IsValid(sim, observer, enemy);
        Assert.NotNull(sim.CaptureState().SkipTargetRandomState);
        var saved = SimSavegame.Write(sim, true);
        var expected = Enumerable.Range(0, 20).Select(_ => FriendlyEnemySearch.IsValid(sim, observer, enemy)).ToArray();
        SimSavegame.Apply(sim, saved);
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => FriendlyEnemySearch.IsValid(sim, observer, enemy)).ToArray());
        SimSavegame.Apply(sim, initial); Assert.Null(sim.CaptureState().SkipTargetRandomState);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []),
        Things = [new LevelThing { Type = 3004, X = 64, Y = 64 },
            new LevelThing { Type = 3001, X = 320, Y = 64 },
            new LevelThing { Type = 3004, X = 900, Y = 900, Friendly = true }]
    });
}
