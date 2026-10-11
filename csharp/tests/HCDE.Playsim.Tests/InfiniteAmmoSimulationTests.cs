using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InfiniteAmmoSimulationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptyPistolFiresOnlyWithInfiniteAmmo(bool enabled)
    {
        var sim = Room(enabled); var player = sim.Players.Single();
        player.Inventory.Bullets = 0;
        Assert.Equal(enabled, HitscanCombat.Fire(sim, player));
        Assert.Equal(0, player.Inventory.Bullets);
    }

    [Fact]
    public void ProtectedTakeFailsButOrdinaryRemovalStillConsumes()
    {
        var player = Room(true).Players.Single();
        Assert.False(ActorInventoryActions.TakeInventory(player, "Clip", 7, flags: 1));
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.True(ActorInventoryActions.TakeInventory(player, "Clip", 7));
        Assert.Equal(43, player.Inventory.Bullets);
        Assert.True(player.Inventory.TrySpendAmmo(AmmoKind.Bullets, 10));
        Assert.Equal(43, player.Inventory.Bullets);
    }

    [Fact]
    public void SelectionAndReloadTreatEmptyWeaponAsUsable()
    {
        var player = Room(true).Players.Single();
        ActorInventoryActions.GiveInventory(player, "BFG9000");
        player.Inventory.Cells = 0; player.Inventory.Pending = null;
        Assert.Equal(1, AcsPlayerInventory.SetWeapon(player, "BFG9000"));
        Assert.Equal(WeaponKind.Bfg, player.Inventory.Pending);
        player.Inventory.Selected = WeaponKind.Bfg; player.Inventory.Pending = null;
        ActorInventoryActions.CheckReload(player);
        Assert.Null(player.Inventory.Pending);
        Assert.False(player.WeaponLowering);
        ActorInventoryActions.SelectWeapon(player, null, flags: 1);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Pending);
    }

    [Fact]
    public void ProtectionFlagDoesNotProtectNonAmmoInventory()
    {
        var player = Room(true).Players.Single();
        ActorInventoryActions.SetInventory(player, "MedPatch", 2);
        Assert.True(ActorInventoryActions.TakeInventory(player, "MedPatch", 1, flags: 1));
        Assert.Equal(1, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void SettingChangesChecksumAndRemainsActiveAfterRestore()
    {
        Assert.NotEqual(Room(false).Checksum, Room(true).Checksum);
        var sim = Room(true); var player = sim.Players.Single();
        player.Inventory.Bullets = 0;
        var bytes = SimSavegame.Write(sim);
        player.Inventory.Bullets = 20; SimSavegame.Apply(sim, bytes);
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(0, player.Inventory.Bullets);
    }

    private static AuthoritySimulation Room(bool enabled) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(InfiniteAmmo: enabled));
}
