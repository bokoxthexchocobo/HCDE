using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryClearActionTests
{
    [Fact]
    public void CallbackClearsHeldItemsAndPowersOnlyForItsOwner()
    {
        var sim = Room(); var players = sim.Players.ToArray(); var player = players[0];
        Prepare(player); Prepare(players[1]); player.Health = 30;
        player.States.Configure(player, [new(-1, 0), new(4, 0,
            Action: ActorInventoryActions.ClearInventory)], 0);
        player.States.Enter(player, 1);
        Assert.Equal(1, player.States.Current); Assert.Equal(30, player.Health);
        Assert.Equal(0, player.Inventory.Bullets); Assert.Equal(0, player.Inventory.Shells);
        Assert.Equal((WeaponKind)0, player.Inventory.Weapons); Assert.Equal((WeaponKind)0, player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending); Assert.False(player.Inventory.BlueKey);
        Assert.False(player.Inventory.HasBackpack);
        Assert.Empty(player.Inventory.HealthPickups); Assert.Empty(player.Inventory.SpareArmor);
        Assert.Equal(0, player.PowerDamageTics); Assert.Equal(0, player.PowerProtectionTics);
        Assert.Equal(0, player.PowerBuddhaTics); Assert.Equal(0, player.DrainStrength);
        Assert.True(players[1].Inventory.BlueKey); Assert.True(players[1].Inventory.HasBackpack);
        Assert.NotEmpty(players[1].Inventory.HealthPickups);
    }

    [Fact]
    public void DepletedArmorRetainsAbsorptionMetadataAndCustomAmmoCapacity()
    {
        var player = new PlayerPawn(); Prepare(player);
        player.Inventory.MaxBullets = 333;
        ActorInventoryActions.ClearInventory(player);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal("StoredSuit", player.Inventory.ArmorType);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.Equal(9, player.Inventory.AbsorbCount);
        Assert.Equal(333, player.Inventory.MaxBullets);
        Assert.Equal(50, player.Inventory.MaxShells);
    }

    [Fact]
    public void ClearedInventorySurvivesSaveAndRestore()
    {
        var sim = Room(); var player = sim.Players.First(); Prepare(player);
        ActorInventoryActions.ClearInventory(player);
        var bytes = SimSavegame.Write(sim);
        Prepare(player); SimSavegame.Apply(sim, bytes);
        Assert.Equal((WeaponKind)0, player.Inventory.Weapons);
        Assert.Equal(0, player.Inventory.Bullets);
        Assert.False(player.Inventory.BlueKey);
        Assert.Empty(player.Inventory.HealthPickups);
        Assert.Equal(0, player.PowerDamageTics);
    }

    [Fact]
    public void DestroyedAndNonplayerActorsDoNotClearOtherPlayers()
    {
        var sim = Room(); var player = sim.Players.First(); Prepare(player);
        ActorInventoryActions.ClearInventory(sim.AddBot(20, 0));
        Assert.True(player.Inventory.BlueKey);
        player.Destroy(); ActorInventoryActions.ClearInventory(player);
        Assert.True(player.Inventory.BlueKey); Assert.NotEmpty(player.Inventory.HealthPickups);
    }

    private static void Prepare(PlayerPawn player)
    {
        ActorInventoryActions.GiveInventory(player, "Backpack");
        ActorInventoryActions.SetInventory(player, "MedPatch", 2);
        player.Inventory.BlueKey = true;
        player.Inventory.TryKeepArmorPickup(100, 50, armorType: "StoredSuit");
        player.Inventory.TryKeepArmorPickup(80, 33, armorType: "Reserve");
        player.Inventory.AbsorbCount = 9;
        player.GivePowerDamage(); player.GivePowerProtection(); player.GivePowerBuddha();
        player.DrainStrength = 2;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 400 }] },
        spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
