using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsStaminaPropertyTests
{
    [Theory]
    [InlineData(false, -1)]
    [InlineData(false, int.MinValue)]
    [InlineData(false, int.MaxValue)]
    [InlineData(true, -1)]
    [InlineData(true, int.MinValue)]
    [InlineData(true, int.MaxValue)]
    public void SignedStaminaPropertyPersistsOnEveryActorType(bool playerActor, int value)
    {
        var sim = Room(); var actor = playerActor ? (Actor)sim.Players.Single() : sim.Actors.Single(a => a.DoomEdNum == 3004);
        var old = SimSavegame.Write(sim, true);
        AcsActorProperties.Set(sim, actor, 0, 34, value);
        Assert.Equal(value, actor.Stamina); Assert.Equal(value, AcsActorProperties.Get(sim, actor, 0, 34));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, 34, value));
        Assert.False(AcsActorProperties.Check(sim, actor, 0, 34, 0));
        var saved = SimSavegame.Write(sim, true); sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.Stamina = 0; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
        SimSavegame.Apply(sim, saved); Assert.Equal(value, actor.Stamina); Assert.Equal(checksum, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.Equal(0, actor.Stamina);
    }

    [Fact]
    public void TidAssignmentUpdatesAllMatchesWithoutChangingUnrelatedActor()
    {
        var sim = Room(); var actors = sim.Actors.Where(a => a is not PlayerPawn).ToArray();
        foreach (var actor in actors) actor.ThingId = 7;
        AcsActorProperties.Set(sim, null, 7, 34, 21);
        Assert.All(actors, actor => Assert.Equal(21, actor.Stamina));
        Assert.Equal(0, sim.Players.Single().Stamina);
        Assert.Equal(21, AcsActorProperties.Get(sim, null, 7, 34));
        Assert.Equal(0, AcsActorProperties.Get(sim, null, 99, 34));
        Assert.False(AcsActorProperties.Check(sim, null, 99, 34, 0));
    }

    [Fact]
    public void PlayerStaminaPropertyFeedsExistingUpgradeAndPercentageHealing()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 20;
        AcsActorProperties.Set(sim, player, 0, 34, 20);
        Assert.Equal(120, player.GetMaxHealth(true));
        Assert.True(ActorPickupActions.UpgradeStamina(player));
        Assert.Equal(30, AcsActorProperties.Get(sim, player, 0, 34)); Assert.Equal(130, player.Health);
        player.Health = 20; Assert.True(ActorPickupActions.HealthFillup(player)); Assert.Equal(65, player.Health);
    }

    [Fact]
    public void MonsterStaminaDoesNotIncreaseItsHealingMaximumOrAllowPlayerToken()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.DoomEdNum == 3004);
        actor.Stamina = 30; actor.Health = 1;
        Assert.False(ActorPickupActions.UpgradeStamina(actor));
        Assert.True(actor.GiveBody(-100)); Assert.Equal(actor.ResurrectionHealth, actor.Health);
        Assert.Equal(30, actor.Stamina);
    }

    [Fact]
    public void StaminaSaveRetainsExistingSpecialArguments()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.DoomEdNum == 3004);
        ActorPropertyActions.SetSpecial(actor, 112, 7, 35); actor.Stamina = 33;
        var saved = SimSavegame.Write(sim, true);
        actor.Stamina = 0; ActorPropertyActions.SetSpecial(actor, 0);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(33, actor.Stamina); Assert.Equal(112, actor.Special);
        Assert.Equal(7, actor.SpecialArgs[0]); Assert.Equal(35, actor.SpecialArgs[1]);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }, new LevelThing { Type = 3001, X = 200 }]
    }, spawnOptions: new SpawnOptions(Skill: 2));
}
