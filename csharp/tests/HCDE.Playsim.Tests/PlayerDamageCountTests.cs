using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerDamageCountTests
{
    [Fact]
    public void PostArmorDamageAccumulatesCapsAndFades()
    {
        var player = new PlayerPawn { Health = 1000, NoPain = true };
        player.Inventory.Armor = 50; player.Inventory.ArmorSavePercent = 50;
        ActorDamage.Apply(player, 20); Assert.Equal(10, player.DamageCount);
        ActorDamage.Apply(player, 200); Assert.Equal(100, player.DamageCount);
        player.Tick(); Assert.Equal(99, player.DamageCount);
    }

    [Fact]
    public void SaveRestoresCounterAndLegacyResetsIt()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }] });
        var legacy = SimSavegame.Write(sim); var player = sim.Players.Single(); player.NoPain = true;
        ActorDamage.Apply(player, 7); var bytes = SimSavegame.Write(sim);
        player.DamageCount = 0; SimSavegame.Apply(sim, bytes);
        Assert.Equal(7, player.DamageCount); Assert.Equal(bytes, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.DamageCount);
    }
}
