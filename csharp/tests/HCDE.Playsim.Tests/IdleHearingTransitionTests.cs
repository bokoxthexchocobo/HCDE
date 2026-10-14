using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IdleHearingTransitionTests
{
    [Theory]
    [InlineData(100)]
    [InlineData(-1)]
    public void IdleFrameSoundWakeResetsThresholdAndEntersSee(int threshold)
    {
        var (sim, actor, enemy) = Room(); ConfigureIdle(actor);
        actor.Brain!.SetChaseThreshold(threshold, false); actor.Brain.Hear(sim, actor, enemy);
        Assert.Equal(0, actor.Brain.Threshold); Assert.Equal(enemy.Id, actor.TargetId); Assert.Equal(2, actor.States.Current);
    }

    [Fact]
    public void IdleFrameSoundWakePlaysSeeSound()
    {
        var (sim, actor, enemy) = Room(); ConfigureIdle(actor); sim.DrainSoundRequests();
        actor.Brain!.Hear(sim, actor, enemy);
        Assert.Contains(sim.DrainSoundRequests(), request => request.Sound == "test/sight");
    }

    [Fact]
    public void HeardGoalDoesNotPlayEnemySeeSound()
    {
        var (sim, actor, enemy) = Room(); ConfigureIdle(actor); actor.GoalId = enemy.Id; actor.ReactionTime = 0; sim.DrainSoundRequests();
        actor.Brain!.Hear(sim, actor, enemy);
        Assert.Equal(enemy.Id, actor.TargetId);
        Assert.DoesNotContain(sim.DrainSoundRequests(), request => request.Sound == "test/sight");
    }

    [Fact]
    public void NonIdleFrameWakeKeepsThresholdAndDoesNotPlaySeeSound()
    {
        var (sim, actor, enemy) = Room(); ConfigureIdle(actor); actor.States.Enter(actor, 3);
        actor.Brain!.SetChaseThreshold(100, false); sim.DrainSoundRequests(); actor.Brain.Hear(sim, actor, enemy);
        Assert.Equal(100, actor.Brain.Threshold); Assert.Equal(3, actor.States.Current);
        Assert.DoesNotContain(sim.DrainSoundRequests(), request => request.Sound == "test/sight");
    }

    private static void ConfigureIdle(Actor actor)
    {
        actor.PainState = -1; actor.IdleState = 1; actor.SeeState = 2;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.See] = "test/sight" };
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2), new(-1, 3)], 1);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 300 }] });
        return (sim, sim.Actors[0], sim.Actors[1]);
    }
}
