using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DropItemChanceTests
{
    private static AuthoritySimulation Room(int seed = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2035, X = 100, Id = 7 },
            new LevelThing { Type = 2035, X = 200, Id = 7 }],
    }, rngSeed: seed);

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(127)]
    [InlineData(255)]
    [InlineData(256)]
    [InlineData(int.MaxValue)]
    public void ChanceUsesInclusiveNativeThresholdAndAlwaysConsumesDraw(int chance)
    {
        var sim = Room(); var reference = Room();
        var random = (int)(reference.NextCombatRandom() & 255);
        var expected = random <= chance ? 1 : 0;
        Assert.Equal(expected, ActorDropItem.Drop(sim, 0, sim.Players.Single(), "Clip", 0, chance));
        Assert.Equal(expected, sim.Actors.Count(actor => actor.DoomEdNum == PickupCatalog.Clip));
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
    }

    [Fact]
    public void RandomValueEqualToChanceSucceeds()
    {
        var sim = Room(); var reference = Room();
        var chance = (int)(reference.NextCombatRandom() & 255);
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, sim.Players.Single(), "Clip", 0, chance));
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
    }

    [Fact]
    public void ZeroChanceCanSucceedWhenRandomByteIsZero()
    {
        var seed = Enumerable.Range(0, 65536).First(candidate => (Room(candidate).NextCombatRandom() & 255) == 0);
        var sim = Room(seed);
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, sim.Players.Single(), "Clip", 0, 0));
    }

    [Fact]
    public void EveryTidMatchConsumesItsOwnChanceDraw()
    {
        var sim = Room(); var reference = Room();
        reference.NextCombatRandom(); reference.NextCombatRandom();
        Assert.Equal(2, ActorDropItem.Drop(sim, 7, null, "Clip", 0, 255));
        Assert.Equal(2, sim.Actors.Count(actor => actor.DoomEdNum == PickupCatalog.Clip));
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
    }

    [Fact]
    public void UnresolvedClassAndMissingDropperDoNotDraw()
    {
        var sim = Room(); var before = sim.CombatRandomState;
        Assert.Equal(0, ActorDropItem.Drop(sim, 0, sim.Players.Single(), "UnknownPickup", 0, 256));
        Assert.Equal(0, ActorDropItem.Drop(sim, 0, null, "Clip", 0, 256));
        Assert.Equal(0, ActorDropItem.Drop(sim, 99, null, "Clip", 0, 256));
        Assert.Equal(before, sim.CombatRandomState);
    }
}
