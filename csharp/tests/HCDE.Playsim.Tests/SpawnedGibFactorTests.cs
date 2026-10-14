using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnedGibFactorTests
{
    [Theory]
    [InlineData(3004, 10)]
    [InlineData(3001, 30)]
    [InlineData(3003, 500)]
    public void MappedAndDynamicMonstersUseSelectedFactorByDefault(int type, int magnitude)
    {
        var sim = Room(type); sim.GibFactor = 0.5;
        var mapped = sim.Actors.Single(a => a.DoomEdNum == type); var dynamicActor = sim.AddBot(400, 0, type);
        Assert.Equal(int.MinValue, mapped.GibHealth); Assert.Equal(int.MinValue, dynamicActor.GibHealth);
        Assert.Equal(-magnitude, mapped.GetGibHealth()); Assert.Equal(-magnitude, dynamicActor.GetGibHealth());
    }

    [Fact]
    public void PlayerUsesSelectedFactorWithoutManualSentinelAssignment()
    {
        var sim = Room(); sim.GibFactor = 0.5; var player = sim.Players.Single();
        Assert.Equal(-50, player.GetGibHealth());
    }

    [Theory]
    [InlineData(2.5, -25)]
    [InlineData(-37, -18)]
    public void MapHealthOverridesFeedSentinelCalculation(double health, int expected)
    {
        var sim = Room(3004, health); sim.GibFactor = 0.5;
        Assert.Equal(expected, sim.Actors.Single(a => a.IsMonster).GetGibHealth());
    }

    [Fact]
    public void SpawnedLostSoulUsesFactor()
    {
        var sim = Room(71); sim.GibFactor = 0.5;
        var parent = sim.Actors.Single(a => a.DoomEdNum == 71);
        var soul = sim.SpawnLostSoul(parent, sim.Players.Single(), 0)!;
        Assert.NotNull(soul); Assert.Equal(int.MinValue, soul.GibHealth);
        Assert.Equal(-soul.ResurrectionHealth / 2, soul.GetGibHealth());
    }

    [Fact]
    public void ReconstructedMonsterRetainsFactorDependentThreshold()
    {
        var sim = Room(3004); sim.GibFactor = 0.5; var actor = sim.Actors.Single(a => a.IsMonster);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        var restored = sim.Actors.Single(a => a.IsMonster);
        Assert.Equal(int.MinValue, restored.GibHealth); Assert.Equal(-10, restored.GetGibHealth());
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ExplicitThresholdStillOverridesFactorOnSpawnedActor()
    {
        var sim = Room(3004); sim.GibFactor = 0.5; var actor = sim.Actors.Single(a => a.IsMonster);
        actor.GibHealth = 37; Assert.Equal(-37, actor.GetGibHealth());
    }

    private static AuthoritySimulation Room(int type = 3004, double health = 1) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = -400 }, new LevelThing { Type = type, Health = health }]
    });
}
