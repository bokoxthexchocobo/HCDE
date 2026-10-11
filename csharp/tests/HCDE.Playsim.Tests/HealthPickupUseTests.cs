using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealthPickupUseTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ManualUseConsumesOneItemWithSkillScalingAndPickupLimit(int mode)
    {
        var player = Player(.5); player.Health = 40; player.MaxPickupHealth = 45;
        player.Inventory.AddHealthPickup(20, 2, mode);
        Assert.True(player.Inventory.UseHealthPickup(0)); Assert.Equal(45, player.Health);
        Assert.Equal(1, Assert.Single(player.Inventory.HealthPickups).Amount);
        Assert.False(player.Inventory.UseHealthPickup(0));
        Assert.Equal(1, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Fact]
    public void LastSuccessfulUseRemovesStackAndDeadOwnerRetainsItems()
    {
        var player = Player(); player.Health = 40; player.Inventory.AddHealthPickup(20, 1, 3);
        Assert.True(player.Inventory.UseHealthPickup(0)); Assert.Empty(player.Inventory.HealthPickups);
        player.Inventory.AddHealthPickup(20, 1, 3); player.Health = 0;
        Assert.False(player.Inventory.UseHealthPickup(0)); Assert.Single(player.Inventory.HealthPickups);
    }

    [Theory]
    [InlineData(false, 45)]
    [InlineData(true, 25)]
    public void StrifeAutomaticUseHonorsPickupMaximumForBothDamagePaths(bool poison, int expected)
    {
        var player = Player(); player.Health = 60; player.MaxPickupHealth = 45;
        player.Inventory.AddHealthPickup(20, 3, 3);
        if (poison) ActorDamage.ApplyPoisonDamage(player, 20);
        else ActorDamage.Apply(player, 20);
        Assert.Equal(expected, player.Health);
        Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Fact]
    public void OwnerlessInventoryCannotConsumeAndInvalidIndexIsReported()
    {
        var inventory = new PlayerInventory(); inventory.AddHealthPickup(20, 1, 3);
        Assert.False(inventory.UseHealthPickup(0)); Assert.Single(inventory.HealthPickups);
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.UseHealthPickup(-1));
    }

    private static PlayerPawn Player(double factor = 1) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(HealthFactor: factor)).Players.Single();
}
