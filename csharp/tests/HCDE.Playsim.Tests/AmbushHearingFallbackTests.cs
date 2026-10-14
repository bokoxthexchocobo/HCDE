using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AmbushHearingFallbackTests
{
    [Fact]
    public void HiddenNoiseCanResumeLastEnemy()
    {
        var (sim, actor, noise, other) = Room(); actor.Brain!.RememberHateTarget(other.Id);
        actor.Brain.Hear(sim, actor, noise); Assert.Equal(other.Id, actor.TargetId); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void HiddenNoiseCanResumeGoal()
    {
        var (sim, actor, noise, other) = Room(); actor.GoalId = other.Id; actor.ReactionTime = 0;
        actor.Brain!.Hear(sim, actor, noise); Assert.Equal(other.Id, actor.TargetId); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void FallbackGoalStillHonorsWait()
    {
        var (sim, actor, noise, other) = Room(); actor.GoalId = other.Id; actor.ReactionTime = 100;
        actor.Brain!.Hear(sim, actor, noise); Assert.Null(actor.TargetId); Assert.Equal(0, actor.States.Current);
    }

    [Fact]
    public void FailedFallbackStaysIdleAndResetsThreshold()
    {
        var (sim, actor, noise, _) = Room(); actor.Brain!.SetChaseThreshold(100, false);
        actor.Brain.Hear(sim, actor, noise); Assert.Null(actor.TargetId); Assert.Equal(0, actor.States.Current);
        Assert.Equal(0, actor.Brain.Threshold);
    }

    [Fact]
    public void VisiblePlayerWinsFallbackBeforeGoal()
    {
        var (sim, actor, noise, other) = Room(true); actor.GoalId = other.Id;
        actor.Brain!.Hear(sim, actor, noise); Assert.Equal(sim.Players.Single().Id, actor.TargetId);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Noise, Actor Other) Room(bool player = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Lines = [new LevelLine { X1 = 100, Y1 = -256, X2 = 100, Y2 = 256, SideBack = -1 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 300 },
                new LevelThing { Type = 3002, Y = 300 }, .. (player ? new[] { new LevelThing { Type = 1, Y = 400 } } : Array.Empty<LevelThing>())] });
        var actor = sim.Actors[0]; actor.Ambush = true; actor.PainState = -1; actor.SeeState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1)], 0);
        Assert.False(CombatTrace.HasLineOfSight(sim, actor, sim.Actors[1]));
        return (sim, actor, sim.Actors[1], sim.Actors[2]);
    }
}
