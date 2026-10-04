namespace HCDE.Playsim.Tests;

public class AcsMegasphereGrantTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void PositiveQuantityRunsPickupActionsOnce(int amount)
    {
        var player = new PlayerPawn { Health = 10 };
        player.Inventory.Pending = WeaponKind.Pistol;
        AcsPlayerInventory.Give(player, "MegaSphere", amount);
        Assert.Equal(200, player.Health);
        Assert.Equal(200, player.Inventory.Armor);
        Assert.Equal("BlueArmorForMegasphere", player.Inventory.ArmorType);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Pending);
        Assert.Equal(0, AcsPlayerInventory.Count(player, "Megasphere", false));
        player.Health = 10;
        Assert.Equal(0, AcsPlayerInventory.Use(player, "Megasphere"));
        Assert.Equal(10, player.Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonpositiveQuantityDoesNotGrant(int amount)
    {
        var player = new PlayerPawn { Health = 10 };
        AcsPlayerInventory.Give(player, "Megasphere", amount);
        Assert.Equal(10, player.Health);
        Assert.Equal(0, player.Inventory.Armor);
    }

    [Fact]
    public void StringTableGrantUsesCombinedPickup()
    {
        var player = new PlayerPawn { Health = 10 };
        AcsPlayerInventory.Give(player, ["Megasphere"], 0, 1);
        Assert.Equal(200, player.Health);
        Assert.Equal(200, player.Inventory.Armor);
    }

    [Fact]
    public void FullArmorDoesNotPreventHealthGrant()
    {
        var player = new PlayerPawn { Health = 10 };
        player.Inventory.Armor = 250;
        player.Inventory.ArmorType = "GreenArmor";
        AcsPlayerInventory.Give(player, "Megasphere", 1);
        Assert.Equal(200, player.Health);
        Assert.Equal(250, player.Inventory.Armor);
        Assert.Equal("GreenArmor", player.Inventory.ArmorType);
    }
}
