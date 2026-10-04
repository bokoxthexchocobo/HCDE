namespace HCDE.Playsim.Tests;

public class AmmoCapacityClassNameTests
{
    [Theory]
    [InlineData("Bullet", "Clip")]
    [InlineData("AmmoClip", "Clip")]
    [InlineData("Shells", "Shell")]
    [InlineData("Rockets", "RocketAmmo")]
    [InlineData("Cells", "Cell")]
    public void CapacityOperationsRejectNamesWithoutNativeAmmoClasses(string alias, string native)
    {
        var player = new PlayerPawn();
        var original = AcsPlayerInventory.AmmoCapacity(player, native);
        Assert.Equal(0, AcsPlayerInventory.AmmoCapacity(player, alias));
        Assert.Equal(0, AcsPlayerInventory.AmmoCapacity(new Actor(), alias));
        AcsPlayerInventory.SetAmmoCapacity(player, alias, -7);
        AcsPlayerInventory.SetAmmoCapacity(player, [alias], 0, 999);
        Assert.Equal(original, AcsPlayerInventory.AmmoCapacity(player, native));
    }

    [Theory]
    [InlineData("clip")]
    [InlineData("shell")]
    [InlineData("rocketammo")]
    [InlineData("cell")]
    public void NativeNamesRemainCaseInsensitiveAndSupportSignedCapacity(string name)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.SetAmmoCapacity(player, [name], 0, -7);
        Assert.Equal(-7, AcsPlayerInventory.AmmoCapacity(player, [name], 0));
        Assert.Equal(-7, AcsPlayerInventory.Count(player, name, true));
    }
}
