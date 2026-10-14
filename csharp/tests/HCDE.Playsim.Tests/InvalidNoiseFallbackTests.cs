using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InvalidNoiseFallbackTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void InvalidNoiseFallsBackToGoalOrLastEnemy(bool dead, bool goal)
    {
        var (sim, actor, source, other) = Room();
        if (dead) source.Health = 0; else source.Shootable = false;
        if (goal) actor.GoalId = other.Id; else actor.Brain!.RememberHateTarget(other.Id);
        actor.ReactionTime = 0; actor.Brain!.SetChaseThreshold(100, false);
        actor.Brain.Hear(sim, actor, source);
        Assert.Equal(other.Id, actor.TargetId); Assert.Equal(0, actor.Brain.Threshold);
    }

    [Fact]
    public void InvalidSourceWithoutReplacementStillClearsIdleThreshold()
    {
        var (sim, actor, source, _) = Room(); source.Shootable = false;
        actor.Brain!.SetChaseThreshold(100, false); actor.Brain.Hear(sim, actor, source);
        Assert.Null(actor.TargetId); Assert.Equal(0, actor.Brain.Threshold);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Source, Actor Other) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 300 }, new LevelThing { Type = 3002, Y = 300 }] });
        return (sim, sim.Actors[0], sim.Actors[1], sim.Actors[2]);
    }
}
