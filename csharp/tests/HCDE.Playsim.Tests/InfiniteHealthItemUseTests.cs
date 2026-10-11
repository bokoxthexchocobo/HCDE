using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InfiniteHealthItemUseTests
{
    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public void ManualUseRetainsSuccessfulItemOnlyWhenEnabled(bool enabled, int remaining)
    {
        var player = Room(enabled).Players.Single(); player.Health = 30;
        player.Inventory.GiveHealthPickup("MedPatch", 2);
        Assert.Equal(1, AcsPlayerInventory.Use(player, "MedPatch")); Assert.Equal(40, player.Health);
        Assert.Equal(remaining, player.Inventory.CountHealthPickup("MedPatch"));
        player.Health = 100; Assert.Equal(0, AcsPlayerInventory.Use(player, "MedPatch"));
        Assert.Equal(remaining, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void StrifeAutoUseCanReuseSingleItemUntilThresholdAndSaveRetainsStack()
    {
        var sim = Room(true); var player = sim.Players.Single(); player.Inventory.GiveHealthPickup("MedPatch", 1);
        Assert.Equal(50, player.Inventory.AutoUseStrifeHealth(10, 100));
        Assert.Equal(1, player.Inventory.CountHealthPickup("MedPatch"));
        var saved = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal(1, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void RavenEmergencyHealingStillConsumesInventory()
    {
        var player = Room(true).Players.Single(); player.Inventory.GiveHealthPickup("ArtiHealth", 1);
        Assert.Equal(25, player.Inventory.AutoUseHealth(10, true)); Assert.Empty(player.Inventory.HealthPickups);
    }

    [Fact]
    public void InfiniteUseOptionChangesChecksumIndependentlyOfUnlimitedPickupOption()
    {
        Assert.NotEqual(Room(false).Checksum, Room(true).Checksum);
    }

    private static AuthoritySimulation Room(bool enabled) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(InfiniteHealthItemUse: enabled));
}
