namespace HCDE.Playsim.Tests;

public class ScriptAmmoWithoutReadyTests
{
    [Theory]
    [InlineData("Shell")]
    [InlineData("ShellBox")]
    [InlineData("Shotgun")]
    public void ReplenishingEmptyAmmoSelectsOwnedWeaponWithoutReadyWeapon(string name)
    {
        var player = Prepare();
        AcsPlayerInventory.Give(player, name, 2);
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.Equal(default(WeaponKind), player.Inventory.Selected);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void PreferencesPendingSelectionAndNonemptyAmmoPreventSwitch(
        bool neverSwitch, bool pending, bool nonempty)
    {
        var player = Prepare();
        player.Inventory.NeverAutoSwitch = neverSwitch;
        player.Inventory.Pending = pending ? WeaponKind.Fist : null;
        player.Inventory.Shells = nonempty ? 1 : 0;
        AcsPlayerInventory.Give(player, "Shell", 2);
        Assert.Equal(pending ? WeaponKind.Fist : (WeaponKind?)null, player.Inventory.Pending);
    }

    private static PlayerPawn Prepare()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Clear(player);
        player.Inventory.Weapons = WeaponKind.Shotgun;
        return player;
    }
}
