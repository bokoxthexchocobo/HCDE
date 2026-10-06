using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpareArmorSaveTests
{
    [Fact]
    public void SavePreservesSpareOrderMetadataAndPromotionTie()
    {
        var sim = Room(); var inventory = sim.Players.Single().Inventory;
        SpareArmor[] spares = [new(100, 50, 20, 5, "First", true),
            new(200, 50, 0, 0, "Second"), new(80, 33, 10, 2, "Third")];
        inventory.RestoreSpareArmor(spares);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        var loaded = restored.Players.Single().Inventory;
        Assert.Equal(spares, loaded.SpareArmor);
        Assert.Equal(bytes, SimSavegame.Write(restored));
        loaded.PromoteSpareArmor();
        Assert.Equal("First", loaded.ArmorType); Assert.Equal(100, loaded.Armor);
        Assert.Equal(spares.Skip(1), loaded.SpareArmor);
    }

    [Fact]
    public void LegacyRestoreClearsRuntimeSpareArmor()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        sim.Players.Single().Inventory.RestoreSpareArmor([new(100, 33, 0, 0)]);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state);
        Assert.Empty(sim.Players.Single().Inventory.SpareArmor);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
