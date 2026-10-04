namespace HCDE.Playsim.Tests;

public class BackpackInventoryUseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BackpackUseReturnsFalseWithoutRefillingOrChangingOwnership(bool owned)
    {
        var player = new PlayerPawn();
        if (owned) player.Inventory.GiveBackpack(depleted: true);
        player.Inventory.Bullets = 0;
        player.Inventory.Pending = WeaponKind.Shotgun;
        for (var i = 0; i < 3; i++)
            Assert.Equal(0, AcsPlayerInventory.Use(player, "Backpack"));
        Assert.Equal(owned, player.Inventory.HasBackpack);
        Assert.Equal(owned ? 400 : 200, player.Inventory.MaxBullets);
        Assert.Equal(0, player.Inventory.Bullets);
        Assert.Equal(0, player.Inventory.Shells);
        Assert.Equal(0, player.Inventory.Rockets);
        Assert.Equal(0, player.Inventory.Cells);
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }

    [Fact]
    public void GiveInventoryStillGrantsBackpackAndAmmo()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "Backpack", 1);
        Assert.True(player.Inventory.HasBackpack);
        Assert.Equal(60, player.Inventory.Bullets);
        Assert.Equal(400, player.Inventory.MaxBullets);
    }
}
