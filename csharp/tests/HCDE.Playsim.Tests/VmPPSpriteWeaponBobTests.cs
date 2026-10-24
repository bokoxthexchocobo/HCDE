namespace HCDE.Playsim.Tests;

public class VmPPSpriteWeaponBobTests
{
    [Fact]
    public void WeaponBobSmoothFrac_AppliesSmoothstepWhenEnabled()
    {
        Assert.Equal(0.5, VmPPSpriteWeaponBob.WeaponBobSmoothFrac(0.5, smoothEnabled: true), 3);
        Assert.Equal(0.5, VmPPSpriteWeaponBob.WeaponBobSmoothFrac(0.5, smoothEnabled: false));
    }

    [Fact]
    public void BringUpWeapon_InvokesVirtualHook()
    {
        object? vmArg = null;
        var player = new VmPPSpriteWeaponBob.PlayerModel
        {
            PlayerIndex = 2,
            Mo = new object(),
            ReadyWeapon = new object(),
            WeaponClassName = _ => "Pistol",
        };

        VmPPSpriteWeaponBob.BringUpWeapon(new VmPPSpriteWeaponBob.BringUpContext
        {
            Player = player,
            VmBringUpWeapon = mo => vmArg = mo,
        });

        Assert.Same(player.Mo, vmArg);
    }

    [Fact]
    public void BobWeapon_AppliesInventoryModifyChain()
    {
        var bobSlots = new VmPPSpriteWeaponBob.PlayerBobSlot[VmPPSpriteWeaponBob.MaxPlayers];
        for (var i = 0; i < bobSlots.Length; i++)
            bobSlots[i] = new VmPPSpriteWeaponBob.PlayerBobSlot();

        var player = new VmPPSpriteWeaponBob.PlayerModel
        {
            PlayerIndex = 0,
            Mo = new object(),
            BobTimer = 7,
        };

        VmPPSpriteWeaponBob.BobWeapon(new VmPPSpriteWeaponBob.BobWeaponContext
        {
            Player = player,
            PlayerBob = bobSlots,
            HasVirtualBobWeapon = _ => true,
            CallBobWeapon = (_, _) => (1, 2),
            FirstInventory = _ => new VmPPSpriteWeaponBob.InventoryModel
            {
                OverridesModifyBob = true,
                ModifyBob = (v, _) => (v.X + 10, v.Y + 20),
                Inventory = new VmPPSpriteWeaponBob.InventoryModel
                {
                    ObjectFlags = VmPPSpriteWeaponBob.OfEuthanizeMe,
                },
            },
        });

        Assert.Equal((11, 22), bobSlots[0].Bob2D);
    }
}
