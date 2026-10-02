using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupDimensionTests
{
    [Theory]
    [InlineData(PickupCatalog.Clip, 16)]
    [InlineData(PickupCatalog.Shotgun, 16)]
    [InlineData(PickupCatalog.BlueSkull, 16)]
    [InlineData(PickupCatalog.Stimpack, 16)]
    [InlineData(PickupCatalog.CellPack, 16)]
    [InlineData(PickupCatalog.Backpack, 26)]
    public void MapAndDroppedPickupsUseNativeDimensions(int type, int height)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 100 }],
        });
        var mapItem = Assert.Single(sim.Actors, actor => actor.DoomEdNum == type);
        Assert.Equal(Fixed.FromInt(height), mapItem.Height); Assert.Equal(Fixed.FromInt(20), mapItem.Radius);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), type));
        var dropped = sim.Actors[^1];
        Assert.Equal(mapItem.Height, dropped.Height); Assert.Equal(mapItem.Radius, dropped.Radius);
    }

    [Fact]
    public void TossedDepletedBackpackUsesBackpackHeight()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
        });
        var player = sim.Players.Single(); player.Inventory.GiveBackpack(0, 0, 0, 0, false);
        var drop = sim.DropBackpack(player);
        Assert.NotNull(drop); Assert.Equal(Fixed.FromInt(26), drop.Height);
    }
}
