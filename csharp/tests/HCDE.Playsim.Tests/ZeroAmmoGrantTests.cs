namespace HCDE.Playsim.Tests;

public class ZeroAmmoGrantTests
{
    [Theory]
    [InlineData(AmmoKind.Bullets)]
    [InlineData(AmmoKind.Shells)]
    [InlineData(AmmoKind.Rockets)]
    [InlineData(AmmoKind.Cells)]
    public void ZeroGrantSucceedsWhenThereIsRoomWithoutChangingAmount(AmmoKind kind)
    {
        var inventory = new PlayerInventory();
        var before = inventory.Ammo(kind);
        Assert.True(inventory.TryAddAmmo(kind, 0));
        Assert.Equal(before, inventory.Ammo(kind));
        Assert.False(inventory.TryAddAmmo(kind, -1));
        Assert.Equal(before, inventory.Ammo(kind));
    }

    [Fact]
    public void ZeroGrantAtCapacityFails()
    {
        var inventory = new PlayerInventory { Bullets = 200 };
        Assert.False(inventory.TryAddAmmo(AmmoKind.Bullets, 0));
        Assert.Equal(200, inventory.Bullets);
    }
}
