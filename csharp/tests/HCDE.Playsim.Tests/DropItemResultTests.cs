using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DropItemResultTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2035, Id = 7, X = 100 },
            new LevelThing { Type = 2035, Id = 7, X = 200 }],
    }, compat: CompatSurface.NoTossDrops);

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(127)]
    [InlineData(256)]
    public void TidResultCountsAttemptsRegardlessOfChance(int chance)
    {
        var sim = Room(); var reference = Room();
        var expectedDrops = 0;
        for (var i = 0; i < 2; i++)
            if ((reference.NextDropItemByte()) <= chance) expectedDrops++;
        Assert.Equal(2, ActorDropItem.Drop(sim, 7, null, "Clip", 0, chance));
        Assert.Equal(expectedDrops, sim.Actors.Count(actor => actor.DoomEdNum == PickupCatalog.Clip));
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
    }

    [Fact]
    public void ActivatorResultCountsFailedChanceAttempt()
    {
        var sim = Room();
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, sim.Players.Single(), "Clip", 0, -1));
        Assert.DoesNotContain(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
    }

    [Fact]
    public void AcsDispatcherPublishesAttemptCount()
    {
        var sim = Room();
        var stack = new List<int> { 7, 0, 0, -1 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding(),
            ["Clip"], new AcsGlobalStrings(), AcsCallFunctions.DropItem, 4, out var result));
        Assert.Equal(2, result);
        Assert.DoesNotContain(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
    }
}
