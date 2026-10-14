using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PatrolIdleStateTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DelayedArrivalEntersIdleOrSpawnAndExecutesFrameAction(bool hasIdle)
    {
        var (sim, actor, goal, next) = Room(); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0, _ => calls++), new(-1, 1), new(-1, 2, _ => calls++)], 1);
        actor.SpawnState = 0; actor.SeeState = 1; actor.IdleState = hasIdle ? 2 : -1; calls = 0;
        actor.Brain!.Tick(sim, actor);
        Assert.Equal(hasIdle ? 2 : 0, actor.States.Current); Assert.Equal(1, calls);
        Assert.Equal(next.Id, actor.GoalId); Assert.True(actor.InCombat); Assert.True(actor.JustAttacked);
    }

    [Fact]
    public void IdleActionObservesNativeTransitionOrder()
    {
        var (sim, actor, goal, next) = Room(); uint? observedGoal = null; bool flags = false;
        actor.IdleState = 2; actor.SeeState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2, a =>
        { observedGoal = a.GoalId; flags = a.InCombat && a.JustAttacked && a.TargetId is null; })], 1);
        actor.Brain!.Tick(sim, actor);
        Assert.Equal(goal.Id, observedGoal); Assert.True(flags); Assert.Equal(next.Id, actor.GoalId);
    }

    [Fact]
    public void WaitExpiryLeavesIdleForSeeState()
    {
        var (sim, actor, _, next) = Room(); actor.IdleState = 2; actor.SeeState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2)], 1);
        actor.Brain!.Tick(sim, actor); Assert.Equal(2, actor.States.Current);
        for (var i = 0; i < 35; i++) sim.Thinkers.Clock.Advance();
        actor.Brain.Tick(sim, actor); Assert.Equal(next.Id, actor.TargetId); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void ZeroDelayDoesNotEnterIdle()
    {
        var (sim, actor, _, next) = Room(); next.SpecialArgs[1] = 0; actor.IdleState = 2; actor.SeeState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2)], 1);
        actor.Brain!.Tick(sim, actor); Assert.Equal(1, actor.States.Current);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Goal, Actor Next) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 },
            new LevelThing { Type = 9024, Id = 7, X = 40 }, new LevelThing { Type = 9024, Id = 8, X = 300 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004); var goal = sim.Actors.Single(a => a.ThingId == 7);
        var next = sim.Actors.Single(a => a.ThingId == 8); goal.SpecialArgs[0] = 8; next.SpecialArgs[1] = 1;
        actor.PainState = -1; actor.GoalId = goal.Id; actor.SetTargetPointer(goal); return (sim, actor, goal, next);
    }
}
