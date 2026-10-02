using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupFlagTests
{
    [Theory]
    [InlineData(PickupCatalog.Clip)]
    [InlineData(PickupCatalog.HealthBonus)]
    [InlineData(PickupCatalog.ArmorBonus)]
    public void ClearedPickupFlagPreventsCollectionUntilRestored(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type }],
        });
        var player = sim.Players.Single(); var item = sim.Actors[^1];
        Assert.True(AcsActorFlags.TryGet(player, "PICKUP", out var initial)); Assert.True(initial);
        Assert.True(AcsActorFlags.TrySet(player, "pickup", false));
        sim.Tick(); Assert.False(item.Destroyed);
        Assert.True(AcsActorFlags.TryGet(player, "PiCkUp", out var disabled)); Assert.False(disabled);
        Assert.True(AcsActorFlags.TrySet(player, "PICKUP", true));
        sim.Tick(); Assert.True(item.Destroyed);
    }

    [Fact]
    public void OrdinaryActorsDefaultToNoPickupAndAllowFlagModification()
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TryGet(actor, "PICKUP", out var initial)); Assert.False(initial);
        Assert.True(AcsActorFlags.TrySet(actor, "PICKUP", true)); Assert.True(actor.CanPickupItems);
        actor.Destroy(); Assert.False(AcsActorFlags.TrySet(actor, "PICKUP", false));
    }

    [Fact]
    public void PickupFlagChangeAffectsChecksum()
    {
        var level = new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] };
        var first = AuthoritySimulation.Start(level); var second = AuthoritySimulation.Start(level);
        second.Players.Single().CanPickupItems = false;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }
}
