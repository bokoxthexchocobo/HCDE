using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MaxHealthBonusPickupTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void AllGrantPathsRaiseBonusAndHealUsingUpdatedMaximum(int path)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 90; player.BonusHealth = 49;
        Grant(player, path, 10);
        Assert.Equal(50, player.BonusHealth); Assert.Equal(100, player.Health);
        Assert.Equal(100, player.GetMaxHealth(false));
        Assert.Equal(150, player.GetMaxHealth(true));
        Assert.Equal(0, AcsPlayerInventory.Count(player, "MaxHealthBonus", false));
        Assert.Equal(50, AcsPlayerInventory.Count(player, "MaxHealthBonus", true));
        Assert.Empty(player.Inventory.HealthPickups);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void BonusCanIncreaseWithoutHealingAndRoundTrips(int path)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 150;
        var saved = SimSavegame.Write(sim, true);
        Grant(player, path, 10);
        Assert.Equal(10, player.BonusHealth); Assert.Equal(150, player.Health);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Grant(player, path, 10);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void FullBonusStillAllowsHealingButFullHealthAndBonusReject()
    {
        var player = Room().Players.Single(); player.BonusHealth = 50; player.Health = 90;
        Assert.True(ActorInventoryActions.GiveInventory(player, "MaxHealthBonus", 10));
        Assert.Equal(100, player.Health);
        Assert.False(ActorInventoryActions.GiveInventory(player, "MaxHealthBonus", 10));
        Assert.Equal(50, player.BonusHealth);
    }

    [Fact]
    public void SkillScalesHealingButNotBonusIncrease()
    {
        var player = Room(2).Players.Single(); player.Health = 40;
        Assert.True(ActorInventoryActions.GiveInventory(player, "maxhealthbonus", 2));
        Assert.Equal(2, player.BonusHealth); Assert.Equal(44, player.Health);
    }

    [Fact]
    public void SetZeroDoesNotGrantWhileStateActionNormalizesToOne()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "MaxHealthBonus", 0));
        Assert.Equal(0, player.BonusHealth);
        Assert.True(ActorInventoryActions.GiveInventory(player, "MaxHealthBonus", 0));
        Assert.Equal(1, player.BonusHealth);
        AcsPlayerInventory.Give(player, "MaxHealthBonus", 0);
        Assert.Equal(1, player.BonusHealth);
    }

    [Fact]
    public void NonPlayerReceivesOnlyOrdinaryHealing()
    {
        var actor = new Actor { Health = 10, ResurrectionHealth = 100 };
        Assert.True(ActorInventoryActions.GiveInventory(actor, "MaxHealthBonus", 5));
        Assert.Equal(15, actor.Health);
        AcsPlayerInventory.Give(actor, "MaxHealthBonus", 5);
        Assert.Equal(20, actor.Health);
    }

    private static void Grant(PlayerPawn player, int path, int amount)
    {
        if (path == 0) AcsPlayerInventory.Give(player, "MaxHealthBonus", amount);
        else if (path == 1) Assert.True(ActorInventoryActions.GiveInventory(player, "MaxHealthBonus", amount));
        else Assert.True(ActorInventoryActions.SetInventory(player, "MaxHealthBonus", amount));
    }

    private static AuthoritySimulation Room(double factor = 1) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(HealthFactor: factor));
}
