using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseIdleTransitionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LostTargetEntersIdleOrSpawnAndRunsAction(bool hasIdle)
    {
        var (sim, actor, enemy) = Room(); var calls = 0;
        actor.IdleState = hasIdle ? 2 : -1; actor.PainState = -1;
        actor.States.Configure(actor, [new(-1, 0, _ => calls++), new(-1, 1), new(-1, 2, _ => calls++)], 1);
        calls = 0; enemy.Health = 0; actor.Brain!.Tick(sim, actor);
        Assert.Equal(hasIdle ? 2 : 0, actor.States.Current); Assert.Equal(1, calls); Assert.Null(actor.TargetId);
        actor.Brain.Tick(sim, actor); Assert.Equal(1, calls);
    }

    [Fact]
    public void FriendlyTargetLossDoesNotForceIdleFrame()
    {
        var (sim, actor, enemy) = Room(); actor.Friendly = true; enemy.Health = 0; actor.PainState = -1; actor.IdleState = 2;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2)], 1);
        actor.Brain!.Tick(sim, actor); Assert.Equal(1, actor.States.Current); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void TargetLossIdleActionObservesClearedTarget()
    {
        var (sim, actor, enemy) = Room(); var observed = false; actor.PainState = -1; actor.IdleState = 2;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2, a => observed = a.TargetId is null)], 1);
        enemy.Health = 0; actor.Brain!.Tick(sim, actor); Assert.True(observed);
    }

    [Fact]
    public void InitiallyIdleSearchDoesNotRestartSpawnAction()
    {
        var (sim, actor, enemy) = Room(); actor.SetTargetPointer(null); enemy.Health = 0; var calls = 0;
        actor.States.Configure(actor, [new(-1, 0, _ => calls++)], 0); calls = 0;
        actor.Brain!.Tick(sim, actor); Assert.Equal(0, calls);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 300, Id = 7 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004); var enemy = sim.Actors.Single(a => a.ThingId == 7);
        actor.SetTargetPointer(enemy); return (sim, actor, enemy);
    }
}
