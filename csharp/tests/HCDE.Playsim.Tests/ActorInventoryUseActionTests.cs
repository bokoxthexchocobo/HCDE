using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryUseActionTests
{
    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public void SuccessfulUseHealsOnceAndHonorsInfiniteInventory(bool infinite, int remaining)
    {
        var sim = Room(infinite); var player = sim.Players.Single();
        player.Health = 30; ActorInventoryActions.SetInventory(player, "MedPatch", 2);
        var result = false;
        player.States.Configure(player, [new(-1, 0), new(4, 0,
            Action: self => result = ActorInventoryActions.UseInventory(self, "medpatch"))], 0);
        player.States.Enter(player, 1);
        Assert.True(result); Assert.Equal(40, player.Health);
        Assert.Equal(remaining, player.Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(1, player.States.Current);
        var saved = SimSavegame.Write(sim);
        player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal(remaining, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void StoredArmorUseEquipsAndConsumesOnlyOnSuccess(bool infinite, int remaining)
    {
        var player = Room(infinite).Players.Single();
        player.Inventory.Armor = 10;
        player.Inventory.TryKeepArmorPickup(100, 50, armorType: "StoredSuit");
        Assert.True(ActorInventoryActions.UseInventory(player, "StoredSuit"));
        Assert.Equal(100, player.Inventory.Armor);
        Assert.Equal("StoredSuit", player.Inventory.ArmorType);
        Assert.Equal(remaining, player.Inventory.SpareArmor.Count);
        Assert.False(ActorInventoryActions.UseInventory(player, "StoredSuit"));
        Assert.Equal(remaining, player.Inventory.SpareArmor.Count);
    }

    [Fact]
    public void WeaponUseQueuesSwitchButReturnsFalseAndRetainsOwnership()
    {
        var player = new PlayerPawn();
        ActorInventoryActions.GiveInventory(player, "Shotgun");
        player.Inventory.Pending = null; player.Inventory.Shells = 0;
        Assert.False(ActorInventoryActions.UseInventory(player, "Shotgun"));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void FailedHealthUseAndDeadOwnerDoNotConsumeItems()
    {
        var player = new PlayerPawn();
        ActorInventoryActions.SetInventory(player, "MedPatch", 2);
        Assert.False(ActorInventoryActions.UseInventory(player, "MedPatch"));
        player.Health = 0;
        Assert.False(ActorInventoryActions.UseInventory(player, "MedPatch"));
        Assert.Equal(2, player.Inventory.CountHealthPickup("MedPatch"));
        player.Health = 30; player.Destroy();
        Assert.False(ActorInventoryActions.UseInventory(player, "MedPatch"));
        Assert.Equal(2, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void MissingAndUnusableClassesFailWithoutSideEffects()
    {
        var player = new PlayerPawn();
        Assert.False(ActorInventoryActions.UseInventory(player, null));
        Assert.False(ActorInventoryActions.UseInventory(player, "Unknown"));
        Assert.False(ActorInventoryActions.UseInventory(player, "Clip"));
        Assert.False(ActorInventoryActions.UseInventory(player, "BasicArmor"));
        Assert.False(ActorInventoryActions.UseInventory(new Actor(), "MedPatch"));
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Null(player.Inventory.Pending);
    }

    private static AuthoritySimulation Room(bool infinite) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(InfiniteStoredInventoryUse: infinite));
}
