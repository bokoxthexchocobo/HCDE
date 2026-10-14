using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RavenHealthDropTests
{
    [Theory]
    [InlineData("ArtiHealth", 82)]
    [InlineData("ArtiSuperHealth", 32)]
    public void TossOneArtifactWithDelaySaveRestoreAndRecollection(string name, int number)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.GiveHealthPickup(name, 2);
        Assert.True(AcsPlayerInventory.Drop(player, name.ToLowerInvariant()));
        Assert.Equal(1, player.Inventory.CountHealthPickup(name));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == number);
        Assert.True(drop.Dropped); Assert.False(drop.SpecialPickup); Assert.Equal(30, drop.PickupDelay);
        Assert.Equal(1, drop.PickupAmount);
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == number);
        Assert.Equal(30, drop.PickupDelay); Assert.Equal(1, drop.PickupAmount);
        drop.X = Fixed.FromInt(200); drop.Y = default;
        drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        for (var i = 0; i < 30; i++) sim.Tick();
        Assert.True(drop.SpecialPickup);
        player.X = drop.X; player.Y = drop.Y; player.Z = drop.Z; sim.Tick();
        Assert.True(drop.Destroyed); Assert.Equal(2, player.Inventory.CountHealthPickup(name));
    }

    [Theory]
    [InlineData("ArtiHealth", 82)]
    [InlineData("ArtiSuperHealth", 32)]
    public void ScriptDropPreservesArtifactQuantityInsteadOfWeaponAmmoScaling(string name, int number)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.GiveHealthPickup(name, 2);
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, player, name, 3, 256));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == number);
        Assert.Equal(3, drop.PickupAmount); Assert.Equal(2, player.Inventory.CountHealthPickup(name));
        drop.Z = player.Z; drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        sim.Tick(); Assert.True(drop.Destroyed); Assert.Equal(5, player.Inventory.CountHealthPickup(name));
    }

    [Theory]
    [InlineData(false, 25)]
    public void UnsupportedModeCannotLoseInventory(bool raven, int health)
    {
        var sim = Room(raven); var player = sim.Players.Single(); player.Inventory.AddHealthPickup(health, 2, 1, "ArtiHealth");
        Assert.False(AcsPlayerInventory.Drop(player, "ArtiHealth"));
        Assert.Equal(2, player.Inventory.CountHealthPickup("ArtiHealth")); Assert.Single(sim.Actors);
    }

    private static AuthoritySimulation Room(bool raven = true) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(RavenHealthPickups: raven, DefaultInventoryMaximum: 16));
}
