namespace HCDE.Playsim.Tests;

public class WeaponRemovalRaiseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReplacementRaisesDuringRemovalWithInstantPreference(bool instant)
    {
        var player = new PlayerPawn { InstantWeaponSwitch = instant };
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 8;
        player.WeaponLowering = true;
        AcsPlayerInventory.Take(player, "Pistol", 1);
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending);
        Assert.False(player.WeaponLowering);
        Assert.Equal(instant ? PlayerPawn.WeaponTop : PlayerPawn.WeaponBottom - PlayerPawn.WeaponMoveSpeed,
            player.WeaponOffsetY);
        Assert.Equal(instant, player.WeaponReady);
    }

    [Fact]
    public void RemovedReadyAndPendingWeaponStillTriggersImmediateReplacement()
    {
        var player = new PlayerPawn(); player.Inventory.Pending = WeaponKind.Pistol;
        AcsPlayerInventory.Take(player, "Pistol", 1);
        Assert.Equal(WeaponKind.Fist, player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending);
    }

    [Fact]
    public void ExistingDifferentPendingSelectionIsNotRaisedDuringRemoval()
    {
        var player = new PlayerPawn(); player.Inventory.Pending = WeaponKind.Fist;
        AcsPlayerInventory.Take(player, "Pistol", 1);
        Assert.Equal(default(WeaponKind), player.Inventory.Selected);
        Assert.Equal(WeaponKind.Fist, player.Inventory.Pending);
        Assert.Equal(PlayerPawn.WeaponTop, player.WeaponOffsetY);
    }
}
