using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InfiniteStoredArmorUseTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void SuccessfulManualArmorUseRetainsOnlyWhenEnabled(bool enabled, int remaining)
    {
        var player = Room(enabled).Players.Single(); player.Inventory.Armor = 10;
        player.Inventory.TryKeepArmorPickup(100, 50, armorType: "StoredSuit");
        Assert.Equal(1, AcsPlayerInventory.Use(player, "StoredSuit")); Assert.Equal(100, player.Inventory.Armor);
        Assert.Equal(remaining, player.Inventory.SpareArmor.Count);
        Assert.Equal(0, AcsPlayerInventory.Use(player, "StoredSuit")); Assert.Equal(remaining, player.Inventory.SpareArmor.Count);
    }

    [Fact]
    public void AutomaticPromotionRetainsBestSuitForLaterReuse()
    {
        var player = Room(true).Players.Single(); player.Inventory.Armor = 10;
        player.Inventory.TryKeepArmorPickup(100, 33, armorType: "WeakSuit");
        player.Inventory.TryKeepArmorPickup(200, 50, armorType: "StrongSuit");
        player.Inventory.Armor = 0; player.Inventory.PromoteSpareArmor();
        Assert.Equal(200, player.Inventory.Armor); Assert.Equal("StrongSuit", player.Inventory.ArmorType);
        Assert.Equal(2, player.Inventory.SpareArmor.Count);
        player.Inventory.Armor = 0; player.Inventory.PromoteSpareArmor(); Assert.Equal(200, player.Inventory.Armor);
    }

    [Fact]
    public void HealthOnlyOptionDoesNotRetainArmorAndBroaderOptionChangesChecksum()
    {
        var sim = Room(false, healthOnly: true); var player = sim.Players.Single(); player.Inventory.Armor = 10;
        player.Inventory.TryKeepArmorPickup(100, 50, armorType: "StoredSuit");
        Assert.Equal(1, AcsPlayerInventory.Use(player, "StoredSuit")); Assert.Empty(player.Inventory.SpareArmor);
        Assert.NotEqual(sim.Checksum, Room(true).Checksum);
    }

    [Fact]
    public void BroaderOptionAlsoRetainsHealthItemAfterSaveRestoration()
    {
        var sim = Room(true); var player = sim.Players.Single(); player.Inventory.GiveHealthPickup("MedPatch", 1);
        var saved = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        player.Health = 30; Assert.Equal(1, AcsPlayerInventory.Use(player, "MedPatch"));
        Assert.Equal(1, player.Inventory.CountHealthPickup("MedPatch"));
    }

    private static AuthoritySimulation Room(bool enabled, bool healthOnly = false) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(InfiniteStoredInventoryUse: enabled, InfiniteHealthItemUse: healthOnly));
}
