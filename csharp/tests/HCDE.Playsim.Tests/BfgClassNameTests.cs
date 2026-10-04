namespace HCDE.Playsim.Tests;

public class BfgClassNameTests
{
    [Theory]
    [InlineData("BFG")]
    [InlineData("bfg")]
    public void ShorthandCannotQuerySelectUseRemoveOrDropOwnedWeapon(string name)
    {
        var player = new PlayerPawn();
        player.Inventory.Weapons |= WeaponKind.Bfg;
        player.Inventory.Cells = 100;
        player.Inventory.Selected = WeaponKind.Bfg;
        player.Inventory.Pending = WeaponKind.Pistol;
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, true));
        Assert.Equal(0, AcsPlayerInventory.CheckWeapon(player, name));
        Assert.Equal(0, AcsPlayerInventory.SetWeapon(player, name));
        Assert.Equal(0, AcsPlayerInventory.Use(player, name));
        AcsPlayerInventory.Take(player, name, 1);
        Assert.False(AcsPlayerInventory.Drop(player, name));
        Assert.True(player.Inventory.Owns(WeaponKind.Bfg));
        Assert.Equal(100, player.Inventory.Cells);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Pending);
    }

    [Fact]
    public void ShorthandGrantDoesNotCreateWeaponOrAmmo()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, ["BFG"], 0, 1);
        Assert.False(player.Inventory.Owns(WeaponKind.Bfg));
        Assert.Equal(0, player.Inventory.Cells);
    }

    [Fact]
    public void NativeNameStillGrantsQueriesSelectsAndRemoves()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "bfg9000", 1);
        Assert.Equal(1, AcsPlayerInventory.Count(player, "BFG9000", false));
        Assert.Equal(40, player.Inventory.Cells);
        Assert.Equal(1, AcsPlayerInventory.SetWeapon(player, "bfg9000"));
        Assert.Equal(WeaponKind.Bfg, player.Inventory.Pending);
        player.Inventory.Selected = WeaponKind.Bfg;
        Assert.Equal(1, AcsPlayerInventory.CheckWeapon(player, "BFG9000"));
        AcsPlayerInventory.Take(player, "bfg9000", 1);
        Assert.False(player.Inventory.Owns(WeaponKind.Bfg));
    }
}
