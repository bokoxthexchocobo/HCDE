using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseShootabilityTests
{
    [Fact]
    public void LostShootabilityRetainsTargetWhenSearchFindsNothing()
    {
        var (sim, actor, enemy) = Room(); enemy.Shootable = false; actor.TidToHate = 99;
        actor.Brain!.Tick(sim, actor); Assert.Equal(enemy.Id, actor.TargetId); Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void TemporaryTargetIsRememberedWhenGoalTakesOver()
    {
        var (sim, actor, enemy) = Room(); enemy.Shootable = false; enemy.NonShootable = true;
        actor.GoalId = sim.Actors.Single(a => a.ClassDoomEdNum == 9024).Id; actor.Brain!.SetChaseThreshold(10, false);
        actor.Brain.Tick(sim, actor);
        Assert.Equal(actor.GoalId, actor.TargetId); Assert.Equal(enemy.Id, actor.Brain.LastEnemyId);
        Assert.Equal(0, actor.Brain.Threshold);
    }

    [Fact]
    public void TemporaryTargetCanBeReacquiredButCannotAttackOnSearchCall()
    {
        var (sim, actor, enemy) = Room(); enemy.Shootable = false; enemy.NonShootable = true;
        actor.ReactionTime = 0; actor.Brain!.Tick(sim, actor);
        Assert.Equal(enemy.Id, actor.TargetId); Assert.Null(actor.Brain.LastEnemyId);
        Assert.Equal(0, actor.Brain.WindupTics); Assert.Equal(0, actor.VelocityX.Raw);
    }

    [Fact]
    public void NonShootableGoalRemainsPursuable()
    {
        var (sim, actor, enemy) = Room(); enemy.Shootable = false; actor.GoalId = enemy.Id;
        actor.Brain!.Tick(sim, actor); Assert.Equal(enemy.Id, actor.TargetId); Assert.True(actor.VelocityX.Raw > 0);
    }

    [Fact]
    public void VulnerableDoesNotForceExistingTargetRemoval()
    {
        var (sim, actor, enemy) = Room(); enemy.Shootable = false; enemy.Vulnerable = true; actor.TidToHate = 99;
        actor.Brain!.Tick(sim, actor, useMelee: false, useMissile: false); Assert.Equal(enemy.Id, actor.TargetId);
    }

    [Fact]
    public void SavedTemporaryTargetMemoryReplays()
    {
        var (sim, actor, enemy) = Room(); enemy.Shootable = false; enemy.NonShootable = true;
        actor.GoalId = sim.Actors.Single(a => a.ClassDoomEdNum == 9024).Id;
        actor.Brain!.Tick(sim, actor); var saved = SimSavegame.Write(sim, true);
        actor.Brain.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor, PlayerPawn Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 300 }, new LevelThing { Type = 9024, Y = 300 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004); var enemy = sim.Players.Single();
        actor.SetTargetPointer(enemy); return (sim, actor, enemy);
    }
}
