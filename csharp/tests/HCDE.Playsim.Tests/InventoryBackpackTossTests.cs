using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryBackpackTossTests
{
    [Fact]
    public void AcsDropCreatesDepletedDelayedBackpackAndDetachesCapacity()
    {
        var sim = Room(); var player = sim.Players.Single();
        PickupCatalog.TryGive(player, PickupCatalog.Backpack);
        player.Inventory.Bullets = 250;
        Assert.True(AcsPlayerInventory.Drop(player, "Backpack"));
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Backpack);
        Assert.True(drop.Depleted);
        Assert.True(drop.Dropped);
        Assert.False(drop.SpecialPickup);
        Assert.Equal(30, drop.PickupDelay);
        Assert.Equal(10, drop.Z.ToDouble());
        Assert.Equal(5, drop.VelocityX.ToDouble());
        Assert.Equal(1, drop.VelocityZ.ToDouble());
        Assert.False(player.Inventory.HasBackpack);
        Assert.Equal(200, player.Inventory.MaxBullets);
        Assert.Equal(200, player.Inventory.Bullets);
        Assert.False(AcsPlayerInventory.Drop(player, "Backpack"));
        Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Backpack);
    }

    [Fact]
    public void RecollectionRestoresCapacityWithoutCreatingAmmo()
    {
        var sim = Room(); var player = sim.Players.Single();
        PickupCatalog.TryGive(player, PickupCatalog.Backpack);
        var bullets = player.Inventory.Bullets;
        var shells = player.Inventory.Shells;
        AcsPlayerInventory.Drop(player, "Backpack");
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Backpack);
        drop.VelocityX = default; drop.VelocityY = default; drop.VelocityZ = default;
        drop.NoGravity = true;
        for (var i = 0; i < 29; i++) sim.Tick();
        Assert.False(player.Inventory.HasBackpack);
        sim.Tick();
        Assert.True(player.Inventory.HasBackpack);
        Assert.Equal(400, player.Inventory.MaxBullets);
        Assert.Equal(bullets, player.Inventory.Bullets);
        Assert.Equal(shells, player.Inventory.Shells);
        Assert.DoesNotContain(drop, sim.Actors);
    }

    [Fact]
    public void CustomCapacitiesSurviveBackpackDetach()
    {
        var sim = Room(); var player = sim.Players.Single();
        PickupCatalog.TryGive(player, PickupCatalog.Backpack);
        player.Inventory.MaxBullets = 500;
        player.Inventory.Bullets = 450;
        Assert.True(AcsPlayerInventory.Drop(player, "Backpack"));
        Assert.Equal(500, player.Inventory.MaxBullets);
        Assert.Equal(450, player.Inventory.Bullets);
    }

    [Fact]
    public void SavePreservesDepletedFlagAndRemainingPickupDelay()
    {
        var sim = Room(); var player = sim.Players.Single();
        PickupCatalog.TryGive(player, PickupCatalog.Backpack);
        AcsPlayerInventory.Drop(player, "Backpack");
        sim.Tick();
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Backpack);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        drop.Depleted = false; drop.PickupDelay = 0; drop.SpecialPickup = true;
        sim.RestoreState(state);
        Assert.True(drop.Depleted);
        Assert.Equal(29, drop.PickupDelay);
        Assert.False(drop.SpecialPickup);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(Skill: 0));
}
