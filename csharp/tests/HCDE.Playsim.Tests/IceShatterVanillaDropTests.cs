using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceShatterVanillaDropTests
{
    [Theory]
    [InlineData(3004, PickupCatalog.Clip)]
    [InlineData(84, PickupCatalog.Clip)]
    [InlineData(9, PickupCatalog.Shotgun)]
    [InlineData(65, PickupCatalog.Chaingun)]
    public void ShatterDropsNativeVanillaItemBeforeCorpseRemoval(int monsterType, int itemType)
    {
        var sim = Room(monsterType); var corpse = sim.Actors.Single(actor => actor.DoomEdNum == monsterType);
        sim.SpawnIceChunks(corpse);
        Assert.True(corpse.Destroyed);
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == itemType);
        Assert.Equal(corpse.X, drop.X); Assert.Equal(corpse.Y, drop.Y);
        Assert.False(drop.Destroyed); Assert.False(drop.Solid); Assert.False(drop.Shootable);
        sim.SpawnIceChunks(corpse);
        Assert.Single(sim.Actors, actor => actor.DoomEdNum == itemType);
    }

    [Fact]
    public void DelayedShatterDoesNotDropItem()
    {
        var sim = Room(3004); var corpse = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        corpse.VelocityX = Fixed.FromInt(1); sim.SpawnIceChunks(corpse);
        Assert.DoesNotContain(sim.Actors, actor => PickupCatalog.IsPickup(actor.DoomEdNum));
    }

    [Fact]
    public void MonsterWithoutNativeDropDoesNotCreatePickup()
    {
        var sim = Room(3001); var corpse = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        sim.SpawnIceChunks(corpse);
        Assert.DoesNotContain(sim.Actors, actor => PickupCatalog.IsPickup(actor.DoomEdNum));
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 100, Y = 64 }],
    });
}
