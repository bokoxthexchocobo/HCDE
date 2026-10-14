using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChainsawArmorStealTests
{
    [Theory]
    [InlineData(0, 0, 10)]
    [InlineData(195, 0, 200)]
    [InlineData(195, 198, 198)]
    [InlineData(205, 0, 205)]
    public void StolenArmorUsesBonusLimitsWithoutHealing(int armor, int maximum, int expected)
    {
        var (_, player, _) = Room(); player.Inventory.Armor = armor;
        Attack(player, maximum); Assert.Equal(expected, player.Inventory.Armor); Assert.Equal(50, player.Health);
        if (armor == 0)
        {
            Assert.Equal(PlayerInventory.GreenSavePercent, player.Inventory.ArmorSavePercent);
            Assert.Equal("ArmorBonus", player.Inventory.ArmorType);
        }
    }

    [Fact]
    public void ExistingArmorProtectionIsPreserved()
    {
        var (_, player, _) = Room(); player.Inventory.Armor = 50;
        player.Inventory.ArmorSavePercent = 50; player.Inventory.ArmorType = "BlueArmor";
        Attack(player); Assert.Equal(60, player.Inventory.Armor);
        Assert.Equal(50, player.Inventory.ArmorSavePercent); Assert.Equal("BlueArmor", player.Inventory.ArmorType);
    }

    [Fact]
    public void DontDrainPreventsArmorStealing()
    {
        var (_, player, target) = Room(); target.DontDrain = true;
        Attack(player); Assert.Equal(0, player.Inventory.Armor);
    }

    [Fact]
    public void ArmorStealReplaysAfterRestore()
    {
        var (sim, player, _) = Room(); var saved = SimSavegame.Write(sim, true);
        Attack(player); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Attack(player);
        Assert.Equal(10, player.Inventory.Armor); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static void Attack(PlayerPawn player, int maximum = 0)
        => ChainsawActions.Attack(player, 10, spread: 0,
            flags: ChainsawAttackFlags.NoRandom | ChainsawAttackFlags.StealArmor,
            lifeSteal: 1, lifeStealMax: maximum);

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 48 }]
        });
        var player = sim.Players.Single(); player.Health = 50;
        sim.Actors[1].Brain!.Enabled = false; sim.DrainSoundRequests();
        return (sim, player, sim.Actors[1]);
    }
}
