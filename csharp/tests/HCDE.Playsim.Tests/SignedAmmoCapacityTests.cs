namespace HCDE.Playsim.Tests;

public class SignedAmmoCapacityTests
{
    [Theory]
    [InlineData("Clip", AmmoKind.Bullets)]
    [InlineData("Shell", AmmoKind.Shells)]
    [InlineData("RocketAmmo", AmmoKind.Rockets)]
    [InlineData("Cell", AmmoKind.Cells)]
    public void NegativeCapacityIsStoredWithoutReducingAmmo(string name, AmmoKind kind)
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = player.Inventory.Shells = player.Inventory.Rockets = player.Inventory.Cells = 10;
        AcsPlayerInventory.SetAmmoCapacity(player, name, -7);
        Assert.Equal(-7, AcsPlayerInventory.AmmoCapacity(player, name));
        Assert.Equal(-7, AcsPlayerInventory.Count(player, name, true));
        Assert.Equal(10, player.Inventory.Ammo(kind));
        Assert.False(player.Inventory.TryAddAmmo(kind, 1));
        Assert.Equal(10, player.Inventory.Ammo(kind));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    public void LowerCapacityPreservesOverCapAmmoAndCanBeRaisedAgain(int capacity)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.SetAmmoCapacity(player, "Clip", capacity);
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.False(player.Inventory.TryAddAmmo(AmmoKind.Bullets, 1));
        AcsPlayerInventory.SetAmmoCapacity(player, "Clip", 100);
        Assert.True(player.Inventory.TryAddAmmo(AmmoKind.Bullets, 1));
        Assert.Equal(51, player.Inventory.Bullets);
    }
}
