namespace HCDE.Playsim.Tests;

public class AmmoGrantOverflowTests
{
    [Theory]
    [InlineData(AmmoKind.Bullets, 200)]
    [InlineData(AmmoKind.Shells, 50)]
    [InlineData(AmmoKind.Rockets, 50)]
    [InlineData(AmmoKind.Cells, 300)]
    public void LargeGrantCapsWithoutWrapping(AmmoKind kind, int expected)
    {
        var inventory = new PlayerInventory { Bullets = 1, Shells = 1, Rockets = 1, Cells = 1 };
        Assert.True(inventory.TryAddAmmo(kind, int.MaxValue));
        Assert.Equal(expected, inventory.Ammo(kind));
    }

    [Fact]
    public void LargestCapacityStillSaturatesWithoutWrapping()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.SetAmmoCapacity(player, "Clip", int.MaxValue);
        player.Inventory.Bullets = int.MaxValue - 1;
        Assert.True(player.Inventory.TryAddAmmo(AmmoKind.Bullets, 10));
        Assert.Equal(int.MaxValue, player.Inventory.Bullets);
        Assert.False(player.Inventory.TryAddAmmo(AmmoKind.Bullets, 1));
    }

    [Fact]
    public void AcsLargeAmmoGrantUsesOverflowSafeAddition()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "Clip", int.MaxValue);
        Assert.Equal(200, player.Inventory.Bullets);
    }
}
