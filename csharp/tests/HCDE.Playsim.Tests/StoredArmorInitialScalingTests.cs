using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StoredArmorInitialScalingTests
{
    [Theory]
    [InlineData(0.5, 40)]
    [InlineData(1.555, 124)]
    [InlineData(2, 160)]
    public void EmptyInventoryEquipsScaledPickupAndMetadata(double factor, int expected)
    {
        var player = Player(factor);
        Assert.True(player.Inventory.TryKeepArmorPickup(80, 50, 20, 10, "Reserve"));
        Assert.Equal(expected, player.Inventory.Armor);
        Assert.Equal(expected, player.Inventory.ArmorMaximum);
        Assert.Equal(expected, player.Inventory.ArmorActualSaveAmount);
        Assert.Equal("Reserve", player.Inventory.ArmorType);
        Assert.Equal(20, player.Inventory.MaxAbsorb);
        Assert.Equal(10, player.Inventory.MaxFullAbsorb);
        Assert.Empty(player.Inventory.SpareArmor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.001)]
    public void NonpositiveTruncatedAmountKeepsPickupWithoutEquipping(double factor)
    {
        var player = Player(factor);
        Assert.True(player.Inventory.TryKeepArmorPickup(80, 50, armorType: "Reserve"));
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal("None", player.Inventory.ArmorType);
        Assert.Equal(1, player.Inventory.ArmorMaximum);
        Assert.Equal(80, player.Inventory.SpareArmor.Single().SaveAmount);
    }

    [Fact]
    public void WornArmorKeepsUnscaledReserveForLaterActivation()
    {
        var player = Player(2);
        player.Inventory.Armor = 1;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "Reserve");
        Assert.Equal(1, player.Inventory.Armor);
        Assert.Equal(80, player.Inventory.SpareArmor.Single().SaveAmount);
        Assert.Equal(1, AcsPlayerInventory.Use(player, "Reserve"));
        Assert.Equal(160, player.Inventory.Armor);
    }

    [Fact]
    public void StandaloneInventoryRetainsDefaultFactor()
    {
        var inventory = new PlayerInventory();
        inventory.TryKeepArmorPickup(80, 50);
        Assert.Equal(80, inventory.Armor);
        Assert.Empty(inventory.SpareArmor);
    }

    private static PlayerPawn Player(double factor) => AuthoritySimulation.Start(new PlayLevel
    {
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(ArmorFactor: factor)).Players.Single();
}
