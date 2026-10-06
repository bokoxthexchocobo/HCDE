using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDieActionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForcedDeathBypassesOrdinaryProtectionWithoutConsumingArmor(bool protectedActor)
    {
        var actor = new Actor { Health = 100, Invulnerable = protectedActor, Buddha = protectedActor,
            Armor = 100, ArmorSavePercent = 100 };
        ActorHealthActions.Die(actor);
        Assert.Equal(0, actor.Health); Assert.True(actor.Killed); Assert.Equal(100, actor.Armor);
    }

    [Fact]
    public void NonShootableActorStillRequiresDamageEligibility()
    {
        var actor = new Actor { Shootable = false, NonShootable = true, Health = 100 };
        ActorHealthActions.Die(actor);
        Assert.Equal(100, actor.Health); Assert.False(actor.Killed);
    }

    [Fact]
    public void FrameDeathRunsAssignedSpecialAndRoundTripsKilledState()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetSpecial(actor, 112, 7, 35);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, Action: self => ActorHealthActions.Die(self)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.True(actor.Killed); Assert.Equal(35, sim.LightOf(0));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Killed = false; actor.Health = 100; sim.RestoreState(state);
        Assert.True(actor.Killed); Assert.Equal(0, actor.Health); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void CustomDamageTypeIsForwardedToDeathPipeline()
    {
        var actor = new Actor { Health = 100 };
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2)], 0);
        actor.SetTypedDeath("Fire", 2);
        ActorHealthActions.Die(actor, "Fire");
        Assert.Equal(0, actor.Health); Assert.Equal("Fire", actor.DamageType);
        Assert.Equal(2, actor.States.Current);
    }

    [Fact]
    public void AlreadyDeadActorDoesNotRepeatDeathSpecial()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorHealthActions.Die(actor);
        ActorPropertyActions.SetSpecial(actor, 112, 7, 35);
        ActorHealthActions.Die(actor);
        Assert.Equal(160, sim.LightOf(0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7, LightLevel = 160 }] });
}
