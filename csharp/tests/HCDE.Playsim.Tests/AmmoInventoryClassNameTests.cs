namespace HCDE.Playsim.Tests;

public class AmmoInventoryClassNameTests
{
    [Theory]
    [InlineData("Bullet")]
    [InlineData("AmmoClip")]
    [InlineData("Shells")]
    [InlineData("Rockets")]
    [InlineData("Cells")]
    public void QueryDoesNotTreatConvenienceNamesAsAmmoClasses(string name)
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = player.Inventory.Shells = player.Inventory.Rockets = player.Inventory.Cells = 10;
        foreach (Actor actor in new Actor[] { player, new Actor() })
        {
            Assert.Equal(0, AcsPlayerInventory.Count(actor, name, false));
            Assert.Equal(0, AcsPlayerInventory.Count(actor, name, true));
            Assert.Equal(0, AcsPlayerInventory.Count(actor, [name], 0, true));
        }
    }

    [Theory]
    [InlineData("clip", AmmoKind.Bullets)]
    [InlineData("shell", AmmoKind.Shells)]
    [InlineData("rocketammo", AmmoKind.Rockets)]
    [InlineData("cell", AmmoKind.Cells)]
    public void NativeClassQueriesUseCurrentAmmoAndInstanceMaximum(string name, AmmoKind kind)
    {
        var player = new PlayerPawn();
        player.Inventory.TryAddAmmo(kind, 3);
        var expected = player.Inventory.Ammo(kind);
        AcsPlayerInventory.SetAmmoCapacity(player, name, -7);
        Assert.Equal(expected, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(-7, AcsPlayerInventory.Count(player, [name], 0, true));
    }
}
