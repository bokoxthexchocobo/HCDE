using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FriendlyNoiseTransitionTests
{
    [Fact]
    public void FriendlyNoiseEntersSeeWithoutTargetingFriend()
    {
        var (sim, actor, friend, _) = Room(); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, _ => calls++)], 0);
        actor.Brain!.SetChaseThreshold(100, false); actor.Brain.Hear(sim, actor, friend);
        Assert.Equal(1, actor.States.Current); Assert.Equal(1, calls); Assert.Null(actor.TargetId);
        Assert.Equal(0, actor.Brain.Threshold); Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void FriendlyNoiseCanResumeLastEnemyInstead()
    {
        var (sim, actor, friend, enemy) = Room(); actor.Brain!.RememberHateTarget(enemy.Id);
        actor.Brain.Hear(sim, actor, friend); Assert.Equal(enemy.Id, actor.TargetId); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void FriendlyNoiseCanResumeGoalInstead()
    {
        var (sim, actor, friend, enemy) = Room(); actor.GoalId = enemy.Id; actor.ReactionTime = 0;
        actor.Brain!.Hear(sim, actor, friend); Assert.Equal(enemy.Id, actor.TargetId);
    }

    [Fact]
    public void NonIdleFriendlyNoiseDoesNotChangeFrame()
    {
        var (sim, actor, friend, _) = Room(); actor.States.Enter(actor, 1);
        actor.Brain!.Hear(sim, actor, friend); Assert.Equal(1, actor.States.Current); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void MissingSeeFrameDoesNotTargetFriend()
    {
        var (sim, actor, friend, _) = Room(); actor.SeeState = -1;
        actor.Brain!.Hear(sim, actor, friend); Assert.Equal(0, actor.States.Current); Assert.Null(actor.TargetId);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Friend, Actor Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 }, new LevelThing { Type = 3002, Y = 300 }] });
        var actor = sim.Actors[0]; var friend = sim.Actors[1]; actor.Friendly = friend.Friendly = true;
        actor.PainState = -1; actor.SeeState = 1; actor.States.Configure(actor, [new(-1, 0), new(-1, 1)], 0);
        return (sim, actor, friend, sim.Actors[2]);
    }
}
