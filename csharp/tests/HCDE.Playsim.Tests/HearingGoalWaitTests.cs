using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HearingGoalWaitTests
{
    [Theory]
    [InlineData(2, false)]
    [InlineData(1, true)]
    [InlineData(0, true)]
    public void HeardGoalHonorsAbsoluteWaitBoundary(int reaction, bool wakes)
    {
        var (sim, actor, goal) = Room(); actor.ReactionTime = reaction; actor.Brain!.SetChaseThreshold(100, false);
        sim.Thinkers.Clock.Advance(); actor.Brain.Hear(sim, actor, goal);
        Assert.Equal(wakes ? goal.Id : (uint?)null, actor.TargetId);
        Assert.Equal(wakes ? 2 : 1, actor.States.Current); Assert.Equal(reaction, actor.ReactionTime);
        Assert.Equal(0, actor.Brain.Threshold);
    }

    [Fact]
    public void DifferentEnemyInterruptsWaitWithoutChangingTimestamp()
    {
        var (sim, actor, _) = Room(); var enemy = sim.AddBot(0, 300, 3002); actor.ReactionTime = 100;
        actor.Brain!.Hear(sim, actor, enemy);
        Assert.Equal(enemy.Id, actor.TargetId); Assert.Equal(2, actor.States.Current); Assert.Equal(100, actor.ReactionTime);
    }

    [Fact]
    public void WaitingHeardGoalSurvivesSaveRestoreAndWakesAtExpiry()
    {
        var (sim, actor, goal) = Room(); actor.ReactionTime = 2;
        actor.Brain!.Hear(sim, actor, goal); var saved = SimSavegame.Write(sim, true);
        actor.ReactionTime = 0; SimSavegame.Apply(sim, saved);
        actor.Brain.Hear(sim, actor, goal); Assert.Null(actor.TargetId);
        sim.Thinkers.Clock.Advance(); sim.Thinkers.Clock.Advance(); actor.Brain.Hear(sim, actor, goal);
        Assert.Equal(goal.Id, actor.TargetId); Assert.Equal(2, actor.States.Current);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Goal) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 300 }] });
        var actor = sim.Actors[0]; var goal = sim.Actors[1]; actor.GoalId = goal.Id;
        actor.PainState = -1; actor.IdleState = 1; actor.SeeState = 2;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2)], 1);
        return (sim, actor, goal);
    }
}
