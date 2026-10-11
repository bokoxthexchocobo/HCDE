using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AutoUseOrdinaryDamageTests
{
    private static PlayerPawn Player(bool skill = true, SpawnGameMode mode = SpawnGameMode.Single, double factor = 1)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1, Single = true, Coop = true, Deathmatch = true }] },
            spawnOptions: new SpawnOptions(Mode: mode, AutoUseHealth: skill, PlayerDamageFactor: factor));
        var player = sim.Players.Single(); player.NoPain = true; player.Health = 10;
        player.Inventory.AddHealthPickup(25, 2, 1); return player;
    }

    [Fact]
    public void LethalRemainderConsumesHealthAfterArmorBeforeSubtraction()
    {
        var player = Player(); player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50;
        var result = ActorDamage.Apply(player, 30);
        Assert.Equal(20, player.Health); Assert.Equal(15, result.HealthLost); Assert.Equal(15, result.ArmorLost);
        Assert.False(result.Killed); Assert.Equal(15, player.DamageCount);
        Assert.Equal(1, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Fact]
    public void NonlethalArmorRemainderDoesNotConsumeHealth()
    {
        var player = Player(); player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50;
        ActorDamage.Apply(player, 10); Assert.Equal(5, player.Health);
        Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Theory]
    [InlineData(DamageFlags.Forced, 20)]
    [InlineData(DamageFlags.None, ActorDamage.TelefragDamage)]
    public void ForcedAndOrdinaryTelefragSkipAutoHealing(DamageFlags flags, int damage)
    {
        var player = Player(); Assert.True(ActorDamage.Apply(player, damage, flags: flags).Killed);
        Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Fact]
    public void LaxTelefragAllowsAutoHealingAfterDamageReduction()
    {
        var player = Player(factor: 0.00002); player.LaxTelefragDamage = true;
        Assert.False(ActorDamage.Apply(player, ActorDamage.TelefragDamage).Killed);
        Assert.Equal(15, player.Health); Assert.Equal(1, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Fact]
    public void DeathmatchWithoutSkillUsesOnlyModeTwo()
    {
        var player = Player(skill: false, mode: SpawnGameMode.Deathmatch);
        player.Inventory.AddHealthPickup(100, 1, 2);
        Assert.False(ActorDamage.Apply(player, 20).Killed); Assert.Equal(90, player.Health);
        Assert.Equal(new HealthPickupStack(25, 2, 1), Assert.Single(player.Inventory.HealthPickups));
    }

    [Fact]
    public void DisabledSkillAndProtectionDoNotConsumeItems()
    {
        var player = Player(skill: false); Assert.True(ActorDamage.Apply(player, 20).Killed);
        Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount);
        player = Player(); player.GodMode = true;
        Assert.Equal(default, ActorDamage.Apply(player, 20));
        Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount);
    }
}
