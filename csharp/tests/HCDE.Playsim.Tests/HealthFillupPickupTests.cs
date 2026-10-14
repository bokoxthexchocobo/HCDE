using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealthFillupPickupTests
{
    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 75)]
    [InlineData(2, 50)]
    [InlineData(3, 50)]
    [InlineData(4, 100)]
    public void SkillSelectsPercentageIndependentOfGrantQuantityAndHealingFactor(int skill, int expected)
    {
        for (var path = 0; path < 3; path++)
        {
            var sim = Room(skill); var player = sim.Players.Single(); player.Health = 10;
            var saved = SimSavegame.Write(sim, true);
            Grant(player, path);
            Assert.Equal(expected, player.Health);
            var after = SimSavegame.Write(sim, true);
            SimSavegame.Apply(sim, saved); Grant(player, path);
            Assert.Equal(after, SimSavegame.Write(sim, true));
            Assert.Equal(0, AcsPlayerInventory.Count(player, "HealthFillup", false));
            Assert.Equal(1, AcsPlayerInventory.Count(player, "HealthFillup", true));
        }
    }

    [Theory]
    [InlineData(50)]
    [InlineData(150)]
    [InlineData(0)]
    public void AtOrAboveTargetAndDeadPlayersRejectWithoutReducingHealth(int health)
    {
        var player = Room(2).Players.Single(); player.Health = health;
        Assert.False(ActorInventoryActions.GiveInventory(player, "healthfillup", 3));
        Assert.Equal(health, player.Health);
    }

    [Fact]
    public void PercentageUsesUpgradesAndPickupMaximum()
    {
        var player = Room(1).Players.Single(); player.Health = 10; player.Stamina = 20; player.BonusHealth = 10;
        Assert.True(ActorPickupActions.HealthFillup(player)); Assert.Equal(97, player.Health);
        player.Health = 10; player.MaxPickupHealth = 200;
        Assert.True(ActorPickupActions.HealthFillup(player)); Assert.Equal(150, player.Health);
    }

    private static void Grant(PlayerPawn player, int path)
    {
        if (path == 0) AcsPlayerInventory.Give(player, "HealthFillup", 7);
        else if (path == 1) Assert.True(ActorInventoryActions.GiveInventory(player, "HealthFillup", 7));
        else Assert.True(ActorInventoryActions.SetInventory(player, "HealthFillup", 7));
    }

    private static AuthoritySimulation Room(int skill) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(Skill: skill, HealthFactor: .25));
}
