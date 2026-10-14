using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PatrolWaitTests
{
    [Fact]
    public void WaitUsesMapClockRatherThanNumberOfBrainCalls()
    {
        var (sim, actor, goal) = Room(); actor.ReactionTime = 3;
        for (var i = 0; i < 5; i++) actor.Brain!.Tick(sim, actor);
        Assert.Null(actor.TargetId); Assert.Equal(3, actor.ReactionTime); Assert.Equal(0, actor.VelocityX.Raw);
        for (var i = 0; i < 3; i++) sim.Thinkers.Clock.Advance();
        actor.Brain!.Tick(sim, actor); Assert.Equal(goal.Id, actor.TargetId); Assert.True(actor.VelocityX.Raw > 0);
    }

    [Fact]
    public void EnemyCanInterruptWait()
    {
        var (sim, actor, _) = Room(); actor.ReactionTime = 100; var enemy = sim.AddBot(0, 300, 3001);
        actor.SetTargetPointer(enemy); actor.Brain!.Tick(sim, actor, useMelee: false, useMissile: false);
        Assert.Equal(enemy.Id, actor.TargetId); Assert.Equal(99, actor.ReactionTime);
    }

    [Fact]
    public void WaitSurvivesRestore()
    {
        var (sim, actor, _) = Room(); actor.ReactionTime = 100; actor.Brain!.Tick(sim, actor);
        var saved = SimSavegame.Write(sim, true); actor.ReactionTime = 0; actor.Brain.Tick(sim, actor);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor);
        Assert.Null(actor.TargetId); Assert.Equal(100, actor.ReactionTime); Assert.Equal(MonsterMode.Idle, actor.Brain.Mode);
    }

    [Fact]
    public void MapGoalInitializationConsumesSpecialAndAddsCurrentTime()
    {
        var (sim, actor, goal) = Room(); actor.GoalId = null; actor.Special = 229;
        actor.SpecialArgs[1] = 7; actor.SpecialArgs[2] = 2; actor.SpecialArgs[3] = 1;
        sim.Thinkers.Clock.Advance(); actor.Brain!.Tick(sim, actor);
        Assert.Equal(0, actor.Special); Assert.Equal(goal.Id, actor.GoalId); Assert.True(actor.ChaseGoal);
        Assert.Equal(71, actor.ReactionTime); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void ZeroDelayStartsImmediately()
    {
        var (sim, actor, goal) = Room(); actor.ReactionTime = 0;
        actor.Brain!.Tick(sim, actor); Assert.Equal(goal.Id, actor.TargetId);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Goal) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 },
            new LevelThing { Type = 9024, Id = 7, X = 300 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004); var goal = sim.Actors.Single(a => a.ThingId == 7);
        actor.GoalId = goal.Id; actor.TidToHate = 99; return (sim, actor, goal);
    }
}
