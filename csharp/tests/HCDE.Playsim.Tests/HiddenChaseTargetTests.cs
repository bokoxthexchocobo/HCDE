using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HiddenChaseTargetTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HiddenEnemyClearsPursuitAndThreshold(bool invisible)
    {
        var (sim, actor, enemy) = Room(); actor.TidToHate = 99;
        enemy.Invisible = invisible; enemy.MinVisible = !invisible; actor.Brain!.SetChaseThreshold(10, false);
        actor.Brain.Tick(sim, actor, useMelee: false, useMissile: false);
        Assert.Null(actor.TargetId); Assert.Equal(0, actor.Brain.Threshold);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GoalIsExemptFromHiddenTargetClearing(bool invisible)
    {
        var (sim, actor, enemy) = Room(); actor.GoalId = enemy.Id;
        enemy.Invisible = invisible; enemy.MinVisible = !invisible;
        actor.Brain!.Tick(sim, actor, useMelee: false, useMissile: false);
        Assert.Equal(enemy.Id, actor.TargetId); Assert.True(actor.VelocityX.Raw > 0);
    }

    [Fact]
    public void ShadowAloneDoesNotClearExistingTarget()
    {
        var (sim, actor, enemy) = Room(); actor.TidToHate = 99; enemy.Shadow = true;
        actor.Brain!.Tick(sim, actor, useMelee: false, useMissile: false);
        Assert.Equal(enemy.Id, actor.TargetId); Assert.True(actor.VelocityX.Raw > 0);
    }

    [Fact]
    public void ReacquiredPlayerDoesNotAttackOnClearingCall()
    {
        var (sim, actor, enemy) = Room(); enemy.MinVisible = true; actor.ReactionTime = 0;
        actor.Brain!.Tick(sim, actor);
        Assert.Equal(enemy.Id, actor.TargetId); Assert.Equal(0, actor.Brain.WindupTics); Assert.Equal(0, actor.VelocityX.Raw);
    }

    [Fact]
    public void ClearingPreservesLastHeardMemory()
    {
        var (sim, actor, enemy) = Room(); enemy.MinVisible = true; actor.LastHeardTargetId = enemy.Id;
        actor.Brain!.Tick(sim, actor);
        Assert.Equal(enemy.Id, actor.LastHeardTargetId); Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void SavedVisibilityReplaysClearing()
    {
        var (sim, actor, enemy) = Room(); actor.TidToHate = 99; enemy.MinVisible = true;
        var saved = SimSavegame.Write(sim, true); actor.Brain!.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor, PlayerPawn Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 300 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004); var enemy = sim.Players.Single();
        actor.SetTargetPointer(enemy); return (sim, actor, enemy);
    }
}
