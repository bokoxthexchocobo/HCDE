using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArmorMetadataSaveTests
{
    [Theory]
    [InlineData(33, 0, 0, 0)]
    [InlineData(50, 20, 5, 7)]
    [InlineData(100, 10, 0, 9)]
    public void SaveRestoresAbsorptionBehavior(int percent, int cap, int full, int absorbed)
    {
        var sim = Room(); var player = sim.Players.Single(); var armor = player.Inventory;
        armor.Armor = 80; armor.ArmorType = "GreenArmor";
        armor.ArmorMaximum = 100; armor.ArmorActualSaveAmount = 100;
        armor.ArmorSavePercent = percent; armor.MaxAbsorb = cap;
        armor.MaxFullAbsorb = full; armor.AbsorbCount = absorbed;
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state); var other = restored.Players.Single();
        Assert.Equal(SimWornArmor.Capture(armor), SimWornArmor.Capture(other.Inventory));
        Assert.Equal(bytes, SimSavegame.Write(restored));
        ActorDamage.Apply(player, 30); ActorDamage.Apply(other, 30);
        Assert.Equal(player.Health, other.Health);
        Assert.Equal(armor.Armor, other.Inventory.Armor);
        Assert.Equal(armor.AbsorbCount, other.Inventory.AbsorbCount);
    }

    [Fact]
    public void LegacyRestoreClearsRuntimeAbsorptionOverrides()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        var armor = sim.Players.Single().Inventory;
        armor.ArmorSavePercent = 100; armor.MaxAbsorb = 99; armor.AbsorbCount = 7;
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state);
        Assert.Null(SimWornArmor.Capture(armor));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
