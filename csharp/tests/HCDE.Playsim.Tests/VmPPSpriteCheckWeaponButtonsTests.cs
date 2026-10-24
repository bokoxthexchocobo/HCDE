namespace HCDE.Playsim.Tests;

public class VmPPSpriteCheckWeaponButtonsTests
{
    [Fact]
    public void CheckWeaponButtons_SetsPspriteOnFirstMatchingState()
    {
        object? weapon = new();
        object? chosenState = new();
        var player = new VmPPSpriteCheckWeaponButtons.PlayerModel
        {
            ReadyWeapon = weapon,
            WeaponState = VmPPSpriteCheckWeaponButtons.WfWeaponReloadOk,
            Buttons = VmPPSpriteCheckWeaponButtons.BtReload,
        };

        string? setLayer = null;
        VmPPSpriteCheckWeaponButtons.CheckWeaponButtons(new VmPPSpriteCheckWeaponButtons.CheckContext
        {
            Player = player,
            FindWeaponState = (w, name) => w == weapon && name == "Reload" ? chosenState : null,
            SetPsprite = (_, layer, state) =>
            {
                setLayer = $"{layer}:{state}";
            },
        });

        Assert.Equal($"{VmPPSpriteLayers.PspWeapon}:{chosenState}", setLayer);
    }

    [Fact]
    public void CheckWeaponButtons_SkipsWhenStateMissing()
    {
        var calls = 0;
        VmPPSpriteCheckWeaponButtons.CheckWeaponButtons(new VmPPSpriteCheckWeaponButtons.CheckContext
        {
            Player = new VmPPSpriteCheckWeaponButtons.PlayerModel
            {
                ReadyWeapon = new object(),
                WeaponState = VmPPSpriteCheckWeaponButtons.WfWeaponZoomOk,
                Buttons = VmPPSpriteCheckWeaponButtons.BtZoom,
            },
            FindWeaponState = (_, _) => null,
            SetPsprite = (_, _, _) => calls++,
        });

        Assert.Equal(0, calls);
    }
}
