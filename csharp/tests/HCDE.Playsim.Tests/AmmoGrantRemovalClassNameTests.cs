namespace HCDE.Playsim.Tests;

public class AmmoGrantRemovalClassNameTests
{
    [Theory]
    [InlineData("Bullet", AmmoKind.Bullets)]
    [InlineData("AmmoClip", AmmoKind.Bullets)]
    [InlineData("Shells", AmmoKind.Shells)]
    [InlineData("Rockets", AmmoKind.Rockets)]
    [InlineData("Cells", AmmoKind.Cells)]
    public void UnsupportedAmmoAliasCannotGrantOrRemoveAmmo(string alias, AmmoKind kind)
    {
        var player = new PlayerPawn();
        player.Inventory.TryAddAmmo(kind, 10);
        var original = player.Inventory.Ammo(kind);
        AcsPlayerInventory.Give(player, alias, 3);
        Assert.Equal(original, player.Inventory.Ammo(kind));
        AcsPlayerInventory.Give(player, [alias], 0, 3);
        Assert.Equal(original, player.Inventory.Ammo(kind));
        AcsPlayerInventory.Take(player, alias, 3);
        Assert.Equal(original, player.Inventory.Ammo(kind));
        Assert.False(AcsPlayerInventory.Drop(player, alias));
        Assert.Equal(original, player.Inventory.Ammo(kind));
    }

    [Theory]
    [InlineData("clip", AmmoKind.Bullets)]
    [InlineData("shell", AmmoKind.Shells)]
    [InlineData("rocketammo", AmmoKind.Rockets)]
    [InlineData("cell", AmmoKind.Cells)]
    public void NativeNamesGrantAndRemoveRequestedAmmo(string name, AmmoKind kind)
    {
        var player = new PlayerPawn();
        var original = player.Inventory.Ammo(kind);
        AcsPlayerInventory.Give(player, name, 3);
        Assert.Equal(original + 3, player.Inventory.Ammo(kind));
        AcsPlayerInventory.Take(player, name, 2);
        Assert.Equal(original + 1, player.Inventory.Ammo(kind));
    }
}
