using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UpgradeStaminaPickupTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NamedGrantRaisesStaminaThenHealsAndReplays(int path)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 20; player.BonusHealth = 5;
        var saved = SimSavegame.Write(sim, true);
        Grant(player, path, 7);
        Assert.Equal(7, player.Stamina); Assert.Equal(112, player.Health);
        Assert.Equal(0, AcsPlayerInventory.Count(player, "UpgradeStamina", false));
        Assert.Equal(100, AcsPlayerInventory.Count(player, "UpgradeStamina", true));
        Assert.Empty(player.Inventory.HealthPickups);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Grant(player, path, 7);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(95, 20, 100, 200)]
    [InlineData(100, 20, 100, 200)]
    [InlineData(100, 250, 100, 250)]
    public void DefaultPickupCapsStaminaAndStillAcceptsAtCap(int stamina, int health, int expectedStamina, int expectedHealth)
    {
        var player = Room().Players.Single(); player.Stamina = stamina; player.Health = health;
        Assert.True(ActorPickupActions.UpgradeStamina(player));
        Assert.Equal(expectedStamina, player.Stamina); Assert.Equal(expectedHealth, player.Health);
    }

    [Fact]
    public void PercentageHealIgnoresSkillFactorAndHonorsPickupMaximum()
    {
        var player = Room(.25).Players.Single(); player.Health = 20; player.MaxPickupHealth = 130;
        Assert.True(ActorPickupActions.UpgradeStamina(player));
        Assert.Equal(10, player.Stamina); Assert.Equal(130, player.Health);
    }

    [Fact]
    public void NonPlayerAndDestroyedPlayerReject()
    {
        var actor = new Actor { Health = 20 };
        Assert.False(ActorPickupActions.UpgradeStamina(actor));
        Assert.False(ActorInventoryActions.GiveInventory(actor, "UpgradeStamina", 10));
        Assert.Equal(20, actor.Health);
        var player = Room().Players.Single(); player.Destroy();
        Assert.False(ActorPickupActions.UpgradeStamina(player)); Assert.Equal(0, player.Stamina);
    }

    [Fact]
    public void NonpositiveGrantContractsRemainDistinct()
    {
        var player = Room().Players.Single();
        AcsPlayerInventory.Give(player, "UpgradeStamina", 0); Assert.Equal(0, player.Stamina);
        Assert.True(ActorInventoryActions.SetInventory(player, "UpgradeStamina", 0)); Assert.Equal(0, player.Stamina);
        Assert.True(ActorInventoryActions.GiveInventory(player, "upgradestamina", 0)); Assert.Equal(1, player.Stamina);
    }

    private static void Grant(PlayerPawn player, int path, int amount)
    {
        if (path == 0) AcsPlayerInventory.Give(player, "UpgradeStamina", amount);
        else if (path == 1) Assert.True(ActorInventoryActions.GiveInventory(player, "UpgradeStamina", amount));
        else Assert.True(ActorInventoryActions.SetInventory(player, "UpgradeStamina", amount));
    }

    private static AuthoritySimulation Room(double factor = 1) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(HealthFactor: factor));
}
