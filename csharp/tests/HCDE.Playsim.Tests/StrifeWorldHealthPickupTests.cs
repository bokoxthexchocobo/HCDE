using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StrifeWorldHealthPickupTests
{
    [Theory]
    [InlineData(2011, "MedPatch")]
    [InlineData(2012, "MedicalKit")]
    [InlineData(83, "SurgeryKit")]
    public void ContactStoresOneItemWithoutChangingHealthOrArmor(int number, string name)
    {
        var sim = Room(number); var player = sim.Players.Single(); var item = sim.Actors[^1];
        player.Health = 40; sim.Tick();
        Assert.Equal(40, player.Health); Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal(1, player.Inventory.CountHealthPickup(name));
        Assert.True(item.Destroyed); Assert.DoesNotContain(item, sim.Actors);
        var bytes = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(1, player.Inventory.CountHealthPickup(name));
    }

    [Theory]
    [InlineData(2011, "MedPatch")]
    [InlineData(2012, "MedicalKit")]
    [InlineData(83, "SurgeryKit")]
    public void FullInventoryLeavesWorldItemAvailable(int number, string name)
    {
        var sim = Room(number); var player = sim.Players.Single(); var item = sim.Actors[^1];
        player.Inventory.GiveHealthPickup(name, int.MaxValue); sim.Tick();
        Assert.False(item.Destroyed); Assert.Contains(item, sim.Actors);
        player.Inventory.TakeHealthPickup(name, 1); sim.Tick();
        Assert.True(item.Destroyed);
    }

    [Fact]
    public void DefaultDoomPickupStillHealsImmediately()
    {
        var sim = Room(2011, false); var player = sim.Players.Single(); player.Health = 40;
        sim.Tick(); Assert.Equal(50, player.Health); Assert.Empty(player.Inventory.HealthPickups);
    }

    [Fact]
    public void PickupSelectionChangesChecksum()
    {
        Assert.NotEqual(Room(2011).Checksum, Room(2011, false).Checksum);
    }

    private static AuthoritySimulation Room(int number, bool strife = true) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = number }]
    }, spawnOptions: new SpawnOptions(StrifeHealthPickups: strife));
}
