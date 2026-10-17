using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChainsawNativeDamageStealTests
{
    [Theory]
    [InlineData(false, false, 10)]
    [InlineData(false, true, 5)]
    [InlineData(true, false, 10)]
    [InlineData(true, true, 5)]
    public void BuddhaHealthClampDoesNotReducePostArmorStealAcrossFreshSave(bool armorSteal, bool armored, int stolen)
    {
        var sim = Room(); var target = sim.Actors.Single(a => a is not PlayerPawn);
        target.Health = 3; target.Buddha = true;
        if (armored) { target.Armor = 100; target.ArmorSavePercent = 50; }
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var player = world.Players.Single(); var victim = world.Actors.Single(a => a is not PlayerPawn);
            ChainsawActions.Attack(player, 10, spread: 0,
                flags: ChainsawAttackFlags.NoRandom | (armorSteal ? ChainsawAttackFlags.StealArmor : 0), lifeSteal: 1);
            Assert.Equal(1, victim.Health);
            Assert.Equal(armorSteal ? 50 : 50 + stolen, player.Health);
            Assert.Equal(armorSteal ? stolen : 0, player.Inventory.Armor);
            if (armored) Assert.Equal(95, victim.Armor);
        }
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 48 }],
            Sectors = [new LevelSector { CeilingHeight = 128 }],
        });
        sim.Players.Single().Health = 50;
        sim.Actors.Single(a => a is not PlayerPawn).Brain = null;
        return sim;
    }
}
