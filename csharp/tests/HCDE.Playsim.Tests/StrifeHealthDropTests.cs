using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StrifeHealthDropTests
{
    [Theory]
    [InlineData("MedPatch", 2011)]
    [InlineData("MedicalKit", 2012)]
    [InlineData("SurgeryKit", 83)]
    public void InventoryTossDropsOneWithDelayAndPreservesSaveState(string name, int number)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.GiveHealthPickup(name, 2);
        Assert.True(AcsPlayerInventory.Drop(player, name.ToLowerInvariant()));
        Assert.Equal(1, player.Inventory.CountHealthPickup(name));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == number);
        Assert.True(drop.Dropped); Assert.False(drop.SpecialPickup); Assert.Equal(30, drop.PickupDelay);
        Assert.Equal(1, drop.PickupAmount); Assert.Equal(10, drop.Z.ToDouble());
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        var restored = Assert.Single(sim.Actors, actor => actor.DoomEdNum == number);
        Assert.Equal(30, restored.PickupDelay); Assert.Equal(1, restored.PickupAmount);
        restored.X = Fixed.FromInt(200); restored.Y = Fixed.FromInt(0);
        restored.VelocityX = restored.VelocityY = restored.VelocityZ = default;
        for (var i = 0; i < 30; i++) sim.Tick();
        Assert.True(restored.SpecialPickup);
        player.X = restored.X; player.Y = restored.Y; player.Z = restored.Z; sim.Tick();
        Assert.True(restored.Destroyed); Assert.Equal(2, player.Inventory.CountHealthPickup(name));
    }

    [Fact]
    public void ScriptDropItemSpawnsHealthQuantityWithoutTakingInventory()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.GiveHealthPickup("MedPatch", 2);
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, player, "MEDPATCH", 3, 256));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == 2011);
        Assert.Equal(3, drop.PickupAmount); Assert.Equal(2, player.Inventory.CountHealthPickup("MedPatch"));
        drop.X = player.X; drop.Y = player.Y; drop.Z = player.Z;
        drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        sim.Tick(); Assert.True(drop.Destroyed); Assert.Equal(5, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Theory]
    [InlineData(false, 10)]
    public void UnsupportedRoutingPreservesInventory(bool strife, int health)
    {
        var sim = Room(strife); var player = sim.Players.Single(); player.Inventory.AddHealthPickup(health, 2, 3, "MedPatch");
        Assert.False(AcsPlayerInventory.Drop(player, "MedPatch"));
        Assert.Equal(2, player.Inventory.CountHealthPickup("MedPatch")); Assert.Single(sim.Actors);
    }

    [Fact]
    public void ConflictingDoomClassNamesDoNotSpawnStrifeItems()
    {
        var sim = Room(); var player = sim.Players.Single();
        foreach (var name in new[] { "Stimpack", "Medikit", "Megasphere" })
            Assert.Equal(0, ActorDropItem.Drop(sim, 0, player, name, 1, 256));
        Assert.Single(sim.Actors);
    }

    private static AuthoritySimulation Room(bool strife = true) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(StrifeHealthPickups: strife));
}
