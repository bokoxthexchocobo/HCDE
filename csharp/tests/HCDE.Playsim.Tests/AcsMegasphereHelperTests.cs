namespace HCDE.Playsim.Tests;

public class AcsMegasphereHelperTests
{
    [Theory]
    [InlineData(10, 1, 11)]
    [InlineData(190, 25, 200)]
    [InlineData(250, 25, 250)]
    [InlineData(0, 25, 0)]
    public void HealthHelperUsesRequestedAmountAndExplicitLimit(int health, int amount, int expected)
    {
        var player = new PlayerPawn { Health = health, MaxHealth = 50 };
        AcsPlayerInventory.Give(player, "megaspherehealth", amount);
        Assert.Equal(expected, player.Health);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal(0, AcsPlayerInventory.Count(player, "MegasphereHealth", false));
        Assert.Equal(200, AcsPlayerInventory.Count(player, "MegasphereHealth", true));
    }

    [Theory]
    [InlineData(1, 200)]
    [InlineData(2, 400)]
    public void ArmorHelperScalesSaveAmountAndRetainsIdentity(int amount, int expected)
    {
        var player = new PlayerPawn { Health = 10 };
        AcsPlayerInventory.Give(player, "bluearmorformegasphere", amount);
        Assert.Equal(10, player.Health);
        Assert.Equal(expected, player.Inventory.Armor);
        Assert.Equal(expected, player.Inventory.ArmorMaximum);
        Assert.Equal(expected, player.Inventory.ArmorActualSaveAmount);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.Equal("BlueArmorForMegasphere", player.Inventory.ArmorType);
        Assert.Equal(0, AcsPlayerInventory.Count(player, "BlueArmorForMegasphere", false));
        Assert.Equal(0, AcsPlayerInventory.Count(player, "BlueArmorForMegasphere", true));
    }

    [Theory]
    [InlineData("MegasphereHealth")]
    [InlineData("BlueArmorForMegasphere")]
    public void NonpositiveGrantAndUnownedUseDoNotCreateEffects(string type)
    {
        var player = new PlayerPawn { Health = 10 };
        AcsPlayerInventory.Give(player, type, 0);
        AcsPlayerInventory.Give(player, type, -1);
        Assert.Equal(0, AcsPlayerInventory.Use(player, type));
        Assert.Equal(10, player.Health);
        Assert.Equal(0, player.Inventory.Armor);
    }

    [Fact]
    public void StringTableDispatchSupportsBothHelpers()
    {
        var player = new PlayerPawn { Health = 10 };
        string[] names = ["MegasphereHealth", "BlueArmorForMegasphere"];
        AcsPlayerInventory.Give(player, names, 0, 25);
        AcsPlayerInventory.Give(player, names, 1, 1);
        Assert.Equal(35, player.Health);
        Assert.Equal(200, player.Inventory.Armor);
        Assert.Equal(200, AcsPlayerInventory.Count(player, names, 0, true));
    }
}
