using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkillFrameTimingTests
{
    [Theory]
    [InlineData(2, null, false, false, 5)]
    [InlineData(4, null, false, true, 3)]
    [InlineData(2, true, false, true, 3)]
    [InlineData(4, false, false, false, 5)]
    [InlineData(2, false, true, false, 10)]
    [InlineData(2, true, true, true, 3)]
    public void ConfiguredSkillPropertiesDriveFramesAndMatchingSaveContinuation(int skill,
        bool? fast, bool slow, bool expectedFast, int tics)
    {
        var options = new SpawnOptions(Skill: skill, FastMonsters: fast, SlowMonsters: slow);
        var sim = Room(options); var actor = sim.Actors[0];
        Assert.Equal(expectedFast, actor.IsFast()); Assert.Equal(slow, actor.IsSlow());
        actor.States.Enter(actor, 1); Assert.Equal(tics, actor.States.RemainingTics);
        actor.States.Tick(actor);
        var saved = SimSavegame.Write(sim); var restored = Room(options);
        SimSavegame.Apply(restored, saved);
        Assert.Equal(tics - 1, restored.Actors[0].States.RemainingTics);
        Assert.Equal(saved, SimSavegame.Write(restored));
        restored.Actors[0].States.Tick(restored.Actors[0]);
        Assert.Equal(tics - 2, restored.Actors[0].States.RemainingTics);
    }

    [Fact]
    public void NeverFastAllowsSlowScalingAndAlwaysFastTakesPrecedence()
    {
        var sim = Room(new SpawnOptions(FastMonsters: true, SlowMonsters: true));
        var actor = sim.Actors[0]; actor.NeverFast = true;
        actor.States.Enter(actor, 1); Assert.Equal(10, actor.States.RemainingTics);
        actor.AlwaysFast = true; actor.States.Enter(actor, 1);
        Assert.Equal(3, actor.States.RemainingTics);
    }

    private static AuthoritySimulation Room(SpawnOptions options)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] },
            spawnOptions: options);
        var actor = sim.Actors[0]; actor.Brain = null;
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, Fast: true, Slow: true)], 0);
        return sim;
    }
}
