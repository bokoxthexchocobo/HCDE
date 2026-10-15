using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorNativeHealingTests
{
    [Theory]
    [InlineData(0, 51)]
    [InlineData(0.5, 55)]
    [InlineData(1, 60)]
    [InlineData(2, 70)]
    public void PlayerSectorHealingUsesSkillFactorAndMinimumOne(double factor, int expected)
    {
        var sim = Room(factor); var player = sim.Players.Single(); player.Health = 50;
        SectorDamage.Tick(sim, player); Assert.Equal(expected, player.Health);
    }

    [Theory]
    [InlineData(0, 0, 100)]
    [InlineData(20, 0, 109)]
    [InlineData(0, 105, 105)]
    [InlineData(20, 105, 105)]
    public void SectorHealingUsesExplicitHundredAndPickupOverrides(int bonus, int pickupMaximum, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 99;
        player.MaxHealth = 200; player.Stamina = 50; player.BonusHealth = bonus;
        player.MaxPickupHealth = pickupMaximum;
        SectorDamage.Tick(sim, player); Assert.Equal(expected, player.Health);
    }

    [Theory]
    [InlineData(30, 40)]
    [InlineData(59, 60)]
    [InlineData(80, 80)]
    public void MonsterHealingIgnoresPlayerFactorAndUsesSpawnMaximum(int health, int expected)
    {
        var sim = Room(2); var actor = sim.AddBot(128, 0, 3001); actor.Health = health;
        actor.ForceSectorDamage = true;
        SectorDamage.Tick(sim, actor); Assert.Equal(expected, actor.Health);
    }

    [Fact]
    public void SavedScaledHealingReplaysExactly()
    {
        var sim = Room(2); var player = sim.Players.Single(); player.Health = 50;
        var saved = SimSavegame.Write(sim, true); SectorDamage.Tick(sim, player);
        Assert.Equal(70, player.Health); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); SectorDamage.Tick(sim, player);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room(double factor = 1) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, DamageAmount = -10, DamageInterval = 1 }],
        Things = [new LevelThing { Type = 1 }] }, spawnOptions: new SpawnOptions(HealthFactor: factor));
}
