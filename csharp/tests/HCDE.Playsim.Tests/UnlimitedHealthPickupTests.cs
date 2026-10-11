using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UnlimitedHealthPickupTests
{
    [Theory]
    [InlineData("MedPatch", 20)]
    [InlineData("MedicalKit", 15)]
    [InlineData("SurgeryKit", 5)]
    public void NewStackIsCappedButMatchingGrantsCanExceedMaximum(string name, int maximum)
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, name, int.MaxValue);
        Assert.Equal(maximum, player.Inventory.CountHealthPickup(name));
        AcsPlayerInventory.Give(player, name, 3);
        Assert.Equal(maximum + 3, player.Inventory.CountHealthPickup(name));
        Assert.Equal(maximum, AcsPlayerInventory.Count(player, name, true));
        AcsPlayerInventory.Give(player, name, int.MaxValue);
        Assert.Equal(int.MaxValue, player.Inventory.CountHealthPickup(name));
        var bytes = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(int.MaxValue, player.Inventory.CountHealthPickup(name));
    }

    [Fact]
    public void ContactCollectsWorldItemEvenWithFullStoredStack()
    {
        var sim = Room(worldItem: true); var player = sim.Players.Single();
        player.Inventory.GiveHealthPickup("MedPatch", 20); var item = sim.Actors[^1];
        sim.Tick(); Assert.True(item.Destroyed); Assert.Equal(21, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void ConfigurationChangesSimulationChecksum()
    {
        Assert.NotEqual(Room().Checksum, Room(unlimited: false).Checksum);
    }

    private static AuthoritySimulation Room(bool unlimited = true, bool worldItem = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = worldItem ? [new LevelThing { Type = 1 }, new LevelThing { Type = 2011 }] : [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(StrifeHealthPickups: true, UnlimitedHealthPickups: unlimited));
}
