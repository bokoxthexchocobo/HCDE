using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorCheckReloadTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(39, true)]
    [InlineData(40, false)]
    public void ReloadChecksActualFiringCostAndPreservesAmmo(int cells, bool switches)
    {
        var player = Room().Players.Single(); player.Inventory.Weapons |= WeaponKind.Bfg;
        player.Inventory.Selected = WeaponKind.Bfg; player.Inventory.Cells = cells;
        ActorInventoryActions.CheckReload(player);
        Assert.Equal(switches, player.WeaponLowering);
        Assert.Equal(switches ? WeaponKind.Pistol : (WeaponKind?)null, player.Inventory.Pending);
        Assert.Equal(cells, player.Inventory.Cells);
    }

    [Fact]
    public void RailAliasStateCallbackCompletesSwitchAndSavePreservesLowering()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun; player.Inventory.Shells = 0;
        player.States.Configure(player, [new(-1, 0), new(-1, 0,
            Action: ActorInventoryActions.CheckRailReload)], 0);
        player.States.Enter(player, 1); Assert.True(player.WeaponLowering);
        var saved = SimSavegame.Write(sim); player.WeaponLowering = false; SimSavegame.Apply(sim, saved);
        Assert.True(player.WeaponLowering);
        for (var i = 0; i < 40; i++) sim.Tick();
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected); Assert.True(player.WeaponReady);
        Assert.Equal(50, player.Inventory.Bullets);
    }

    [Fact]
    public void MeleeMissingReadyAndNoUsableReplacementPreserveSelection()
    {
        var player = Room().Players.Single(); player.Inventory.Pending = WeaponKind.Chainsaw;
        player.Inventory.Selected = WeaponKind.Fist; ActorInventoryActions.CheckReload(player);
        Assert.Equal(WeaponKind.Chainsaw, player.Inventory.Pending);
        player.Inventory.Selected = 0; ActorInventoryActions.CheckReload(player);
        Assert.Equal(WeaponKind.Chainsaw, player.Inventory.Pending);
        player.Inventory.Weapons = WeaponKind.Shotgun; player.Inventory.Selected = WeaponKind.Shotgun;
        ActorInventoryActions.CheckReload(player); Assert.False(player.WeaponLowering);
        Assert.Equal(WeaponKind.Chainsaw, player.Inventory.Pending);
        player.Destroy(); ActorInventoryActions.CheckRailReload(player);
        ActorInventoryActions.CheckReload(new Actor());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
