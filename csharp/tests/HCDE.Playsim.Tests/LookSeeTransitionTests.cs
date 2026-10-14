using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LookSeeTransitionTests
{
    [Fact]
    public void ExplicitSeeTransitionReturnsBeforeAttackThenChaseCanAttack()
    {
        var (sim, actor) = Room(); Configure(actor, _ => { });
        actor.Brain!.Tick(sim, actor);
        Assert.Equal(1, actor.States.Current); Assert.Equal(MonsterMode.Chase, actor.Brain.Mode);
        Assert.Equal(0, actor.Brain.WindupTics); Assert.Equal(0, actor.VelocityX.Raw);
        actor.Brain.Tick(sim, actor); Assert.Equal(MonsterMode.Windup, actor.Brain.Mode);
    }

    [Fact]
    public void SeeActionCanClearTargetWithoutStaleAttack()
    {
        var (sim, actor) = Room(); Configure(actor, a => a.Brain!.ClearActionTargets());
        actor.Brain!.Tick(sim, actor);
        Assert.Null(actor.TargetId); Assert.Equal(0, actor.Brain.WindupTics); Assert.False(actor.InCombat);
    }

    [Fact]
    public void SeeActionCanDestroyActorWithoutStartingAttack()
    {
        var (sim, actor) = Room(); Configure(actor, a => a.Destroy());
        actor.Brain!.Tick(sim, actor);
        Assert.True(actor.Destroyed); Assert.Equal(0, actor.Brain.WindupTics); Assert.False(actor.JustAttacked);
    }

    [Fact]
    public void SeeActionCanRedirectTargetWithoutUsingOldEnemy()
    {
        var (sim, actor) = Room(); var other = sim.AddBot(0, 300, 3001);
        Configure(actor, a => a.SetTargetPointer(other));
        actor.Brain!.Tick(sim, actor);
        Assert.Equal(other.Id, actor.TargetId); Assert.Equal(0, actor.Brain.WindupTics); Assert.Equal(0, actor.VelocityX.Raw);
    }

    private static void Configure(Actor actor, Action<Actor> action)
    {
        actor.PainState = -1; actor.SeeState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, action)], 0);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 300 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004); actor.ReactionTime = 0; return (sim, actor);
    }
}
