using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissingDeathStateTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void MissingDeathStateDestroysActorAndPublishesDestructionOnce(int deathState)
    {
        var sim = Room(); var source = sim.Players.Single(); var actor = sim.AddBot(100, 0);
        actor.Brain = null; actor.DeathState = deathState; var events = 0;
        sim.ActorDestroyed += removed => { if (removed.Id == actor.Id) events++; };
        Assert.True(ActorDamage.Apply(actor, 1000, source).Killed);
        Assert.True(actor.Destroyed); Assert.Equal(1, actor.DeathCount); Assert.True(actor.Killed);
        Assert.Equal(source.Id, actor.LastDamageSourceId); Assert.Equal(1, events);
        actor.Destroy(); Assert.Equal(1, events);
        sim.Tick(); Assert.DoesNotContain(actor, sim.Actors); Assert.Equal(1, events);
    }

    [Fact]
    public void PlayerWithMissingDeathStateRemainsAvailableForRespawn()
    {
        var sim = Room(); var player = sim.Players.Single(); player.DeathState = -1;
        ActorDamage.Apply(player, 1000);
        Assert.True(player.IsDead); Assert.False(player.Destroyed); Assert.Equal(1, player.DeathCount);
        sim.Tick(); Assert.Contains(player, sim.Actors);
    }

    [Fact]
    public void DirectHealthDeathUsesSameMissingStateCleanup()
    {
        var actor = new Actor { DeathState = -1 }; actor.Health = 0;
        Assert.True(actor.Destroyed); Assert.Equal(1, actor.DeathCount);
    }

    [Fact]
    public void LivingActorWithMissingDeathStateIsUnaffected()
    {
        var actor = new Actor { DeathState = -1, Health = 100 };
        ActorDamage.Apply(actor, 7); Assert.Equal(93, actor.Health); Assert.False(actor.Destroyed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidTypedDeathWinsBeforeMissingNormalState(bool extreme)
    {
        var actor = new Actor { Health = 100, DeathState = -1, ExtremeDeathState = -1 };
        if (extreme) actor.SetTypedDeath("Fire", ActorStateMachine.Corpse, extreme: true);
        else actor.SetTypedDeath("Fire", ActorStateMachine.Corpse);
        ActorDamage.Apply(actor, 1000, damageType: "Fire");
        Assert.False(actor.Destroyed); Assert.Equal(ActorStateMachine.Corpse, actor.States.Current);
    }

    [Fact]
    public void ValidExtremeDeathWinsBeforeMissingNormalState()
    {
        var actor = new Actor { Health = 100, DeathState = -1, ExtremeDeathState = ActorStateMachine.Corpse };
        ActorDamage.Apply(actor, 1000, damageType: "Extreme"); Assert.False(actor.Destroyed); Assert.Equal(ActorStateMachine.Corpse, actor.States.Current);
    }

    [Fact]
    public void UnsupportedTypedDeathFallsBackToDestruction()
    {
        var actor = new Actor { Health = 100, DeathState = -1, ExtremeDeathState = -1 };
        ActorDamage.Apply(actor, 1000, damageType: "Fire"); Assert.True(actor.Destroyed); Assert.Null(actor.DamageType);
    }

    [Fact]
    public void DestructionClearsLiveActorReferences()
    {
        var sim = Room(); var source = sim.Players.Single(); var actor = sim.AddBot(100, 0);
        actor.Brain = null; actor.DeathState = -1; source.MasterId = source.TracerId = actor.Id;
        ActorDamage.Apply(actor, 1000, source);
        Assert.True(actor.Destroyed); Assert.Null(source.MasterId); Assert.Null(source.TracerId);
    }

    [Fact]
    public void PendingSpawnDeathRemovesActorWithNoDeathStateOnFirstTick()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004, Health = 0 }]
        });
        var actor = sim.Actors.Single(); actor.DeathState = -1;
        Assert.True(actor.PendingSpawnDeath); Assert.False(actor.Destroyed);
        sim.Tick(); Assert.True(actor.Destroyed); Assert.Equal(1, actor.DeathCount); Assert.Empty(sim.Actors);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
