using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DropItemProbabilityRandomTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(127)]
    [InlineData(255)]
    [InlineData(256)]
    public void ProbabilityConsumesNamedByteAndUsesInclusiveComparison(int chance)
    {
        var sim = Room(); var source = sim.Players.Single();
        var random = NativeStateRandom.Seed(42, 0x2d1fda00u); var expected = 0;
        var combat = sim.CombatRandomState;
        for (var i = 0; i < 64; i++)
        {
            if ((NativeStateRandom.Next(ref random) & 255) <= chance) expected++;
            Assert.Equal(1, ActorDropItem.Drop(sim, 0, source, "Clip", 0, chance));
        }
        Assert.Equal(expected, sim.Actors.Count(actor => actor.DoomEdNum == PickupCatalog.Clip));
        Assert.Equal(combat, sim.CombatRandomState);
        Assert.Equal(NativeStateRandom.Next(ref random) & 255, sim.NextDropItemByte());
    }

    [Fact]
    public void InvalidDropTypeDoesNotDrawAndSaveRestoresNextProbability()
    {
        var sim = Room(); var older = SimSavegame.Write(sim);
        Assert.Equal(0, ActorDropItem.Drop(sim, 0, sim.Players.Single(), "MissingClass", 0, 256));
        Assert.Equal(Room().NextDropItemByte(), sim.NextDropItemByte());
        var saved = SimSavegame.Write(sim); var next = sim.NextDropItemByte();
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(saved, SimSavegame.Write(restored)); Assert.Equal(next, restored.NextDropItemByte());
        SimSavegame.Apply(sim, older); Assert.Equal(Room().NextDropItemByte(), sim.NextDropItemByte());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] },
        rngSeed: 42, compat: CompatSurface.NoTossDrops);
}
