using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponGrantWithoutReadyTests
{
    [Theory]
    [InlineData("Fist", WeaponKind.Fist)]
    [InlineData("Pistol", WeaponKind.Pistol)]
    [InlineData("Shotgun", WeaponKind.Shotgun)]
    [InlineData("BFG9000", WeaponKind.Bfg)]
    public void NewWeaponQueuesWhenClearRemovedReadyWeapon(string name, WeaponKind kind)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Clear(player);
        AcsPlayerInventory.Give(player, name, 1);
        Assert.True(player.Inventory.Owns(kind));
        Assert.Equal(kind, player.Inventory.Pending);
        Assert.Equal(default(WeaponKind), player.Inventory.Selected);
    }

    [Fact]
    public void NeverSwitchPreferenceStillPreventsGrantSelection()
    {
        var player = new PlayerPawn(); player.Inventory.NeverAutoSwitch = true;
        AcsPlayerInventory.Clear(player);
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.Null(player.Inventory.Pending);
    }

    [Fact]
    public void ExistingReadyWeaponPreservesPendingSelection()
    {
        var player = new PlayerPawn(); player.Inventory.Pending = WeaponKind.Fist;
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(WeaponKind.Fist, player.Inventory.Pending);
    }

    [Fact]
    public void DuplicateOwnedWeaponDoesNotReplacePendingSelection()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Clear(player);
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        player.Inventory.Pending = WeaponKind.Fist;
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        Assert.Equal(WeaponKind.Fist, player.Inventory.Pending);
    }

    [Fact]
    public void GrantedWeaponCanBecomeReadyAfterClear()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var player = sim.Players.Single();
        AcsPlayerInventory.Clear(player);
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        for (var i = 0; i < 40; i++) sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Selected);
        Assert.True(player.WeaponReady);
        Assert.Null(player.Inventory.Pending);
    }
}
