namespace HCDE.Playsim.Tests;

public class BackpackCapacityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FirstBackpackPreservesLargerAmmoLimits(bool depleted)
    {
        var inventory = new PlayerInventory
        {
            MaxBullets = 900, MaxShells = 250, MaxRockets = 200, MaxCells = 1200,
            Bullets = 800, Shells = 200, Rockets = 150, Cells = 1000,
        };
        Assert.True(inventory.GiveBackpack(depleted: depleted));
        Assert.Equal(900, inventory.MaxBullets);
        Assert.Equal(250, inventory.MaxShells);
        Assert.Equal(200, inventory.MaxRockets);
        Assert.Equal(1200, inventory.MaxCells);
        Assert.Equal(depleted ? 800 : 810, inventory.Bullets);
        Assert.Equal(depleted ? 200 : 204, inventory.Shells);
        Assert.Equal(depleted ? 150 : 151, inventory.Rockets);
        Assert.Equal(depleted ? 1000 : 1020, inventory.Cells);
    }

    [Fact]
    public void RemovalOnlyResetsLimitsEqualToBackpackMaximum()
    {
        var inventory = new PlayerInventory
        {
            HasBackpack = true,
            MaxBullets = 400, MaxShells = 250, MaxRockets = 75, MaxCells = 600,
            Bullets = 350, Shells = 200, Rockets = 70, Cells = 500,
        };
        Assert.True(inventory.RemoveBackpack());
        Assert.False(inventory.HasBackpack);
        Assert.Equal(200, inventory.MaxBullets);
        Assert.Equal(200, inventory.Bullets);
        Assert.Equal(250, inventory.MaxShells);
        Assert.Equal(200, inventory.Shells);
        Assert.Equal(75, inventory.MaxRockets);
        Assert.Equal(70, inventory.Rockets);
        Assert.Equal(300, inventory.MaxCells);
        Assert.Equal(300, inventory.Cells);
    }

    [Fact]
    public void RepeatedBackpackDoesNotRaiseChangedLimitsAgain()
    {
        var inventory = new PlayerInventory();
        inventory.GiveBackpack();
        inventory.MaxShells = 60;
        inventory.Shells = 59;
        inventory.GiveBackpack();
        Assert.Equal(60, inventory.MaxShells);
        Assert.Equal(60, inventory.Shells);
        inventory.RemoveBackpack();
        Assert.Equal(60, inventory.MaxShells);
        Assert.Equal(60, inventory.Shells);
    }

    [Fact]
    public void AcsRemovalPreservesLargerLimitsAndAmmo()
    {
        var player = new PlayerPawn();
        player.Inventory.MaxCells = 900;
        player.Inventory.Cells = 800;
        AcsPlayerInventory.Give(player, "Backpack", 1);
        AcsPlayerInventory.Take(player, "Backpack", 1);
        Assert.Equal(900, player.Inventory.MaxCells);
        Assert.Equal(820, player.Inventory.Cells);
        Assert.False(player.Inventory.HasBackpack);
    }
}
