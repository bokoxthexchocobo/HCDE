using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PatrolArrivalTests
{
    [Fact]
    public void ArrivalAdvancesAndClearsOldGoalTarget()
    {
        var (sim, actor, goal, next) = Room(); actor.SetTargetPointer(goal);
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(next.Id, actor.GoalId); Assert.Null(actor.TargetId); Assert.Equal(0, actor.VelocityX.Raw);
        Assert.True(actor.JustAttacked);
    }

    [Fact]
    public void ArrivalExecutesMatchingSpecialWithMonsterActivator()
    {
        var (sim, actor, goal, _) = Room(); actor.SetTargetPointer(goal);
        var special = sim.Actors.Single(a => a.ClassDoomEdNum == 9047);
        special.Special = 127; special.SpecialArgs[0] = 0; special.SpecialArgs[1] = 19;
        special.ActivationType = 32;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(19, actor.Special); Assert.Equal(127, special.Special);
    }

    [Fact]
    public void ChaseGoalArrivalPreservesEnemyAndUsesLastGoalAngle()
    {
        var (sim, actor, goal, next) = Room(); actor.ChaseGoal = true; actor.SetTargetPointer(sim.Players.Single());
        goal.Angle = BamAngle.FromDegrees(90); next.SpecialArgs[1] = 3;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(next.Id, actor.GoalId); Assert.Equal(sim.Players.Single().Id, actor.TargetId);
        Assert.Equal(goal.Angle, actor.Angle); Assert.Equal(8, actor.ReactionTime);
    }

    [Fact]
    public void EndOfRouteClearsGoalAndFacesLastPoint()
    {
        var (sim, actor, goal, _) = Room(); actor.SetTargetPointer(goal); goal.SpecialArgs[0] = 999;
        goal.Angle = BamAngle.FromDegrees(180);
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.Null(actor.GoalId); Assert.Null(actor.TargetId); Assert.Equal(goal.Angle, actor.Angle);
    }

    [Fact]
    public void EnemyPursuitWithoutFlagDoesNotAdvanceNearbyGoal()
    {
        var (sim, actor, goal, _) = Room(); actor.SetTargetPointer(sim.Players.Single());
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive, false, false);
        Assert.Equal(goal.Id, actor.GoalId);
    }

    [Fact]
    public void NextPointDelayIsStoredAsAbsoluteMapTime()
    {
        var (sim, actor, goal, next) = Room(); actor.SetTargetPointer(goal); next.SpecialArgs[1] = 3;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(105 + sim.Thinkers.Clock.Tic, actor.ReactionTime); Assert.Equal(MonsterMode.Idle, actor.Brain.Mode);
        var saved = SimSavegame.Write(sim, true); actor.GoalId = null; SimSavegame.Apply(sim, saved);
        Assert.Equal(next.Id, actor.GoalId); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void RedirectedGoalSuppliesFacingButOriginalRouteSuppliesNextPoint()
    {
        var (sim, actor, goal, next) = Room(); actor.ThingId = 5; actor.SetTargetPointer(goal);
        goal.Angle = BamAngle.FromDegrees(30); next.Angle = BamAngle.FromDegrees(120);
        var special = sim.Actors.Single(a => a.ClassDoomEdNum == 9047);
        special.Special = 229; special.SpecialArgs[0] = 5; special.SpecialArgs[1] = 8;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(next.Id, actor.GoalId); Assert.Null(actor.TargetId); Assert.Equal(next.Angle, actor.Angle);
        Assert.Equal(8, actor.ReactionTime);
    }

    [Fact]
    public void SpecialCanClearGoalWithoutCrashingArrival()
    {
        var (sim, actor, goal, next) = Room(); actor.ThingId = 5; actor.SetTargetPointer(goal);
        var special = sim.Actors.Single(a => a.ClassDoomEdNum == 9047);
        special.Special = 229; special.SpecialArgs[0] = 5; special.SpecialArgs[1] = 999;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(next.Id, actor.GoalId); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void RemovedPatrolSpecialDoesNotExecuteFromSnapshot()
    {
        var (sim, actor, goal, next) = Room(true); actor.SetTargetPointer(goal);
        var specials = sim.Actors.Where(a => a.ClassDoomEdNum == 9047).ToArray();
        specials[0].Special = 132; specials[0].SpecialArgs[0] = 7;
        specials[1].Special = 127; specials[1].SpecialArgs[0] = 0; specials[1].SpecialArgs[1] = 19;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.All(specials, marker => Assert.True(marker.Destroyed));
        Assert.Equal(0, actor.Special); Assert.Equal(next.Id, actor.GoalId);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Goal, Actor Next) Room(bool secondSpecial = false)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 },
            new LevelThing { Type = 9024, Id = 7, X = 40 }, new LevelThing { Type = 9024, Id = 8, X = 300 },
            new LevelThing { Type = 9047, Id = 7 }, new LevelThing { Type = 1, Y = 500 },
            .. (secondSpecial ? new[] { new LevelThing { Type = 9047, Id = 7 } } : Array.Empty<LevelThing>())] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004);
        var goal = sim.Actors.Single(a => a.ClassDoomEdNum == 9024 && a.ThingId == 7);
        var next = sim.Actors.Single(a => a.ThingId == 8); goal.SpecialArgs[0] = 8; actor.GoalId = goal.Id;
        return (sim, actor, goal, next);
    }
}
