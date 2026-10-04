using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponRaiseWithoutReadyTests
{
    [Fact]
    public void FirstWeaponStartsRaisingWithoutEmptyLoweringCycle()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Clear(player);
        Assert.False(player.WeaponReady);
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending);
        Assert.False(player.WeaponLowering);
        Assert.Equal(PlayerPawn.WeaponBottom - PlayerPawn.WeaponMoveSpeed, player.WeaponOffsetY);
        Assert.False(player.WeaponReady);
        for (var i = 1; i < 16; i++) sim.Tick();
        Assert.Equal(PlayerPawn.WeaponTop, player.WeaponOffsetY);
        Assert.True(player.WeaponReady);
    }

    [Fact]
    public void InstantSwitchBringsFirstWeaponReadyImmediately()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.InstantWeaponSwitch = true;
        AcsPlayerInventory.Clear(player);
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Selected);
        Assert.True(player.WeaponReady);
        Assert.Null(player.Inventory.Pending);
    }

    [Fact]
    public void EmptyInventoryCannotBecomeAttackReady()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Clear(player);
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        Assert.False(player.WeaponReady);
        Assert.False(player.AttackPressed);
        Assert.Equal("None", AcsPlayerInventory.ReadyWeaponClassName(player));
    }

    [Fact]
    public void ExistingReadyWeaponStillLowersBeforeHandoff()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 8;
        AcsPlayerInventory.SetWeapon(player, "Shotgun");
        sim.Tick();
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.True(player.WeaponLowering);
        Assert.Equal(PlayerPawn.WeaponTop + PlayerPawn.WeaponMoveSpeed, player.WeaponOffsetY);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
