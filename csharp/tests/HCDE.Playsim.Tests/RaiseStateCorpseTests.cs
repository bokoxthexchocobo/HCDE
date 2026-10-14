using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseStateCorpseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitRaiseStateMakesNonMonsterDeathACorpse(bool damage)
    {
        var actor = new Actor { RaiseState = ActorStateMachine.Spawn };
        if (damage) ActorDamage.Apply(actor, 1000); else actor.Health = 0;
        Assert.True(actor.Corpse); Assert.False(actor.IsMonster); Assert.Equal(0, actor.RaiseDuration);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void MissingOrInvalidRaiseStateDoesNotGrantCorpseFlag(int state)
    {
        var actor = new Actor { RaiseState = state };
        ActorDamage.Apply(actor, 1000); Assert.False(actor.Corpse);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DontCorpsePreservesExistingFlagWithoutAddingIt(bool alreadyCorpse)
    {
        var actor = new Actor { RaiseState = ActorStateMachine.Spawn, DontCorpse = true, Corpse = alreadyCorpse };
        ActorDamage.Apply(actor, 1000); Assert.Equal(alreadyCorpse, actor.Corpse);
    }

    [Fact]
    public void ExplicitRaiseCanReviveNonMonsterWithoutLegacyBrainOrDuration()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0);
        actor.IsMonster = false; actor.Brain = null; actor.RaiseDuration = 0; actor.RaiseState = ActorStateMachine.Spawn;
        var livingHeight = actor.Height;
        ActorDamage.Apply(actor, 1000); actor.States.Enter(actor, ActorStateMachine.Corpse);
        Assert.True(ActorRaise.CanRaise(sim, actor)); Assert.True(ActorRaiseActions.RaiseSelf(actor));
        Assert.False(actor.IsDead); Assert.False(actor.Corpse); Assert.Equal(livingHeight, actor.Height);
        Assert.Equal(ActorStateMachine.Spawn, actor.States.Current);
    }

    [Fact]
    public void DeathActionSeesNewCorpseFlag()
    {
        var actor = new Actor { RaiseState = 0, DeathState = 1 }; bool? observed = null;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, a => observed = a.Corpse)], 0);
        ActorDamage.Apply(actor, 1000); Assert.True(observed);
    }

    [Fact]
    public void SaveRestoresNonMonsterCorpseFlag()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0);
        actor.IsMonster = false; actor.Brain = null; actor.RaiseDuration = 0; actor.RaiseState = ActorStateMachine.Spawn;
        ActorDamage.Apply(actor, 1000); var saved = SimSavegame.Write(sim);
        actor.Corpse = false; SimSavegame.Apply(sim, saved);
        Assert.True(actor.Corpse); Assert.Equal(saved, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
