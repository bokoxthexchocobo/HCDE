namespace HCDE.Playsim.Tests;

public class AcsInventoryOwnershipQueryTests
{
    [Theory]
    [InlineData("Fist")]
    [InlineData("Pistol")]
    public void StartingWeaponCountsFollowRemovalAndGrant(string name)
    {
        var player = new PlayerPawn();
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, false));
        AcsPlayerInventory.Take(player, name, 1);
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, true));
        AcsPlayerInventory.Give(player, name, 1);
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, false));
    }

    [Theory]
    [InlineData("Fist")]
    [InlineData("Pistol")]
    [InlineData("Chainsaw")]
    [InlineData("Shotgun")]
    [InlineData("SuperShotgun")]
    [InlineData("Chaingun")]
    [InlineData("RocketLauncher")]
    [InlineData("PlasmaRifle")]
    [InlineData("BFG9000")]
    public void MaximumWeaponCountDoesNotRequireOwnership(string name)
    {
        var player = new PlayerPawn();
        player.Inventory.Weapons = 0;
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, true));
        AcsPlayerInventory.Give(player, name, 1);
        Assert.Equal(1, AcsPlayerInventory.Count(player, name.ToLowerInvariant(), false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, true));
    }

    [Fact]
    public void BackpackCountTracksAcquisitionAndRemoval()
    {
        var player = new PlayerPawn();
        Assert.Equal(0, AcsPlayerInventory.Count(player, "Backpack", false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "Backpack", true));
        AcsPlayerInventory.Give(player, "Backpack", 1);
        Assert.Equal(1, AcsPlayerInventory.Count(player, "backpack", false));
        AcsPlayerInventory.Take(player, "Backpack", 1);
        Assert.Equal(0, AcsPlayerInventory.Count(player, "Backpack", false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "Backpack", true));
    }
}
