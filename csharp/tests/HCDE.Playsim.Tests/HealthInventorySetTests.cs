using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealthInventorySetTests
{
    [Fact]
    public void ExistingStackUsesAbsoluteAmountAndPreservesHealth()
    {
        var player = Room().Players.Single(); player.Health = 30;
        player.Inventory.GiveHealthPickup("MedPatch", 5);
        Assert.False(player.Inventory.SetHealthPickup("MEDPATCH", 5));
        Assert.True(player.Inventory.SetHealthPickup("medpatch", 3));
        Assert.Equal(3, player.Inventory.CountHealthPickup("MedPatch"));
        Assert.True(player.Inventory.SetHealthPickup("MedPatch", 8));
        Assert.Equal(new HealthPickupStack(10, 8, 3, "MedPatch"), Assert.Single(player.Inventory.HealthPickups));
        Assert.Equal(30, player.Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonpositiveAmountRemovesOnlyFirstStack(int amount)
    {
        var inventory = new PlayerInventory();
        inventory.AddHealthPickup(10, 2, 3, "MedPatch");
        inventory.AddHealthPickup(25, 3, 0, "MedPatch");
        Assert.True(inventory.SetHealthPickup("MedPatch", amount));
        Assert.Equal(new HealthPickupStack(25, 3, 0, "MedPatch"), Assert.Single(inventory.HealthPickups));
        Assert.True(new PlayerInventory().SetHealthPickup("MedPatch", amount));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExistingIncreaseUsesExplicitMaximumOverride(bool unlimited)
    {
        var inventory = Room(unlimited).Players.Single().Inventory;
        inventory.GiveHealthPickup("MedPatch", 1);
        Assert.True(inventory.SetHealthPickup("MedPatch", 100));
        Assert.Equal(20, inventory.CountHealthPickup("MedPatch"));
        Assert.True(inventory.SetHealthPickup("MedPatch", 100, beyondMax: true));
        Assert.True(inventory.SetHealthPickup("MedPatch", 90));
        Assert.Equal(90, inventory.CountHealthPickup("MedPatch"));
        Assert.True(inventory.SetHealthPickup("MedPatch", 91));
        Assert.Equal(20, inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void NewStackUsesPickupCapEvenWithMaximumOverrideAndSurvivesSave()
    {
        var sim = Room(); var inventory = sim.Players.Single().Inventory;
        Assert.True(inventory.SetHealthPickup("ArtiHealth", 100, beyondMax: true));
        Assert.Equal(new HealthPickupStack(25, 16, 1, "ArtiHealth"), Assert.Single(inventory.HealthPickups));
        var saved = SimSavegame.Write(sim); inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal(16, inventory.CountHealthPickup("ArtiHealth"));
    }

    [Fact]
    public void UnknownClassFailsWithoutChangingHeldItems()
    {
        var inventory = new PlayerInventory(); inventory.GiveHealthPickup("SurgeryKit", 2);
        Assert.False(inventory.SetHealthPickup("UnknownHealth", 3));
        Assert.False(inventory.SetHealthPickup("UnknownHealth", 0));
        Assert.Equal(new HealthPickupStack(-100, 2, 0, "SurgeryKit"), Assert.Single(inventory.HealthPickups));
    }

    private static AuthoritySimulation Room(bool unlimited = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(UnlimitedHealthPickups: unlimited, DefaultInventoryMaximum: 16));
}
