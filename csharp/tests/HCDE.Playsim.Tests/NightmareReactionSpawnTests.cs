using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class NightmareReactionSpawnTests
{
    [Theory]
    [InlineData(0, false, 8)]
    [InlineData(3, false, 8)]
    [InlineData(4, false, 0)]
    [InlineData(5, false, 0)]
    [InlineData(3, true, 17)]
    [InlineData(4, true, 0)]
    public void NightmareClearsMapAndDynamicCountersAfterPatchedDefaults(int skill, bool patched, int expected)
    {
        var patch = patched ? DehackedPatch.Apply("Thing 2\nReaction time = 17\n") : null;
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] },
            spawnOptions: new SpawnOptions(Skill: skill), dehacked: patch);
        var mapped = Assert.Single(sim.Actors); var dynamicActor = sim.AddBot(500, 0);
        Assert.Equal(expected, mapped.ReactionTime); Assert.Equal(expected, mapped.Brain!.ReactionTics);
        Assert.Equal(expected, dynamicActor.ReactionTime); Assert.Equal(expected, dynamicActor.Brain!.ReactionTics);
    }
    [Fact]
    public void ClearingMonsterClassificationPreservesExplicitReactionOnNightmare()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = 6\nReaction time = 17\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] },
            spawnOptions: new SpawnOptions(Skill: 4), dehacked: patch);
        var mapped = Assert.Single(sim.Actors); var dynamicActor = sim.AddBot(500, 0);
        Assert.False(mapped.IsMonster); Assert.False(dynamicActor.IsMonster);
        Assert.Equal(17, mapped.ReactionTime); Assert.Equal(17, dynamicActor.ReactionTime);
    }
    [Theory]
    [InlineData(3, 8)]
    [InlineData(4, 0)]
    public void SpawnedChargingSoulUsesSkillReactionOverride(int skill, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 71 }, new LevelThing { Type = 1, X = 800 }] },
            spawnOptions: new SpawnOptions(Skill: skill));
        var parent = sim.Actors.Single(a => a.DoomEdNum == 71);
        var soul = sim.SpawnLostSoul(parent, sim.Players.Single(), 0);
        Assert.NotNull(soul); Assert.True(soul.Brain!.Charging); Assert.Equal(expected, soul.ReactionTime);
    }
}