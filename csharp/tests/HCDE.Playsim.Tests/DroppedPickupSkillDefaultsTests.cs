using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedPickupSkillDefaultsTests
{
    [Theory]
    [InlineData(2, false, false)]
    [InlineData(2, false, true)]
    [InlineData(2, true, false)]
    [InlineData(2, true, true)]
    [InlineData(4, false, false)]
    [InlineData(4, false, true)]
    [InlineData(4, true, false)]
    [InlineData(4, true, true)]
    public void SkillReactionAndFastSpeedAreIndependentAndReconstruct(int skill, bool monster, bool fast)
    {
        var patch = DehackedPatch.Apply($"Thing 64\nBits = SPECIAL{(monster ? "+COUNTKILL" : "")}\nReaction time = 17\nFast speed = 9\n");
        Assert.Empty(patch.Errors); var sim = Room(patch, skill, fast);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); var reaction = monster && skill == 4 ? 0 : 17;
        Assert.Equal(reaction, drop.ReactionTime); Assert.Equal(reaction, drop.SpawnReactionTime);
        Assert.Equal(Fixed.FromInt(fast ? 9 : 0), drop.MovementSpeed);
        var baseline = SimSavegame.Write(sim, true); drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, baseline);
        Assert.Equal(baseline, SimSavegame.Write(sim, true));
        var fresh = Room(patch, skill, fast); SimSavegame.Apply(fresh, baseline);
        Assert.Equal(baseline, SimSavegame.Write(fresh, true));
        Drop(fresh).ReactionTime = 41; var changed = SimSavegame.Write(fresh, true);
        SimSavegame.Apply(fresh, changed); Assert.Equal(41, Drop(fresh).ReactionTime);
        SimSavegame.Apply(fresh, baseline); Assert.Equal(reaction, Drop(fresh).ReactionTime);
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(DehackedPatchResult patch, int skill, bool fast) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] },
        dehacked: patch, spawnOptions: new SpawnOptions(Skill: skill, FastMonsters: fast), compat: CompatSurface.NoTossDrops);
}
