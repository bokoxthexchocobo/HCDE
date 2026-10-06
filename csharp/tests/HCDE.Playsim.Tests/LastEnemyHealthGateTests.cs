using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LastEnemyHealthGateTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LivingLastEnemyResumesPursuitRegardlessOfShootability(bool shootable)
    {
        var (sim, hunter, enemy) = Room(); enemy.Shootable = shootable;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(enemy.Id, hunter.Brain.TargetId);
        Assert.Null(hunter.Brain.LastEnemyId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DeadLastEnemyIsRetainedAndLivingFriendlyEnemyIsDiscarded(bool dead)
    {
        var (sim, hunter, enemy) = Room();
        if (dead) enemy.Health = 0;
        else hunter.Friendly = enemy.Friendly = true;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Null(hunter.Brain.TargetId);
        Assert.Equal(dead ? enemy.Id : (uint?)null, hunter.Brain.LastEnemyId);
        Assert.Equal(MonsterMode.Idle, hunter.Brain.Mode);
    }

    [Fact]
    public void SavedUnshootableLastEnemyResumesAfterLoad()
    {
        var (sim, hunter, enemy) = Room(); enemy.Shootable = false;
        var bytes = SimSavegame.Write(sim);
        hunter.Brain!.ClearActionTargets();
        SimSavegame.Apply(sim, bytes);
        hunter.Brain.Tick(sim, hunter);
        Assert.Equal(enemy.Id, hunter.Brain.TargetId);
        Assert.Null(hunter.Brain.LastEnemyId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RepeatedLookRetainsDeadMemoryEvenForFriendlyCorpse(bool friendly)
    {
        var (sim, hunter, enemy) = Room(); enemy.Health = 0;
        hunter.Friendly = enemy.Friendly = friendly;
        for (var i = 0; i < 3; i++) hunter.Brain!.Tick(sim, hunter);
        Assert.Null(hunter.Brain!.TargetId);
        Assert.Equal(enemy.Id, hunter.Brain.LastEnemyId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RememberedDeadEnemyResumesAfterRaiseIncludingAcrossSave(bool save)
    {
        var (sim, hunter, enemy) = Room();
        ActorDamage.Apply(enemy, 1000);
        enemy.States.Enter(enemy, ActorStateMachine.Corpse);
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(enemy.Id, hunter.Brain.LastEnemyId);
        if (save)
        {
            var bytes = SimSavegame.Write(sim);
            hunter.Brain.ClearActionTargets();
            SimSavegame.Apply(sim, bytes);
            Assert.Equal(enemy.Id, hunter.Brain.LastEnemyId);
        }
        Assert.True(ActorRaiseActions.RaiseSelf(enemy));
        hunter.Brain.Tick(sim, hunter);
        Assert.Equal(enemy.Id, hunter.Brain.TargetId);
        Assert.Null(hunter.Brain.LastEnemyId);
    }

    private static (AuthoritySimulation, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 }],
        });
        var hunter = sim.Actors[0]; var enemy = sim.Actors[1];
        hunter.ReactionTime = 0;
        hunter.Brain!.RestoreTargetMemory(new SimTargetMemory(null, enemy.Id, null));
        return (sim, hunter, enemy);
    }
}
