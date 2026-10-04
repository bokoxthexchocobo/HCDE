using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpecialPickupFlagTests
{
    private static AuthoritySimulation Room(int type = PickupCatalog.Clip) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type }],
    });

    [Theory]
    [InlineData(PickupCatalog.Clip)]
    [InlineData(PickupCatalog.HealthBonus)]
    [InlineData(PickupCatalog.ArmorBonus)]
    public void SpecialFlagControlsItemCollectionIncludingAlwaysPickup(int type)
    {
        var sim = Room(type); var item = sim.Actors[^1];
        Assert.True(AcsActorFlags.TryGet(item, "SPECIAL", out var initial)); Assert.True(initial);
        Assert.True(AcsActorFlags.TrySet(item, "special", false));
        sim.Tick(); Assert.False(item.Destroyed);
        Assert.True(AcsActorFlags.TrySet(item, "SpEcIaL", true));
        sim.Tick(); Assert.True(item.Destroyed);
    }

    [Fact]
    public void DeathDropsEnableSpecialButBackpackTossWaitsForDelay()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.False(player.SpecialPickup);
        Assert.True(sim.SpawnDroppedPickup(player, PickupCatalog.Shotgun));
        Assert.True(sim.Actors[^1].SpecialPickup);
        player.Inventory.GiveBackpack(0, 0, 0, 0);
        var backpack = sim.DropBackpack(player)!;
        Assert.False(backpack.SpecialPickup);
        Assert.Equal(30, backpack.PickupDelay);
    }

    [Fact]
    public void ClearingSpecialChangesChecksumWithoutChangingPickupGrant()
    {
        var first = Room(); var second = Room();
        first.Players.Single().X = second.Players.Single().X = Fixed.FromInt(200);
        second.Actors[^1].SpecialPickup = false;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
        Assert.True(PickupCatalog.TryGive(second.Players.Single(), PickupCatalog.Clip));
    }
}
