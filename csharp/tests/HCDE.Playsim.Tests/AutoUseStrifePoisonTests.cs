using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AutoUseStrifePoisonTests
{
    [Theory]
    [InlineData(40)]
    [InlineData(50)]
    public void NegativePlayerHealthKeepsSelectedStackUntilThreshold(int amount)
    {
        var player = Player(); player.Health = 10;
        player.Inventory.AddHealthPickup(25, amount, 3);
        player.Inventory.AddHealthPickup(amount, 1, 3);
        ActorDamage.ApplyPoisonDamage(player, 20);
        Assert.Equal(45, player.Health);
        Assert.Equal(amount - 3, player.Inventory.HealthPickups[0].Amount);
        Assert.Equal(1, player.Inventory.HealthPickups[1].Amount);
    }

    [Fact]
    public void NegativePlayerHealthMovesToAnotherStackOnlyAfterDepletion()
    {
        var player = Player(); player.Health = 10;
        player.Inventory.AddHealthPickup(25, 2, 3);
        player.Inventory.AddHealthPickup(2, 10, 3);
        ActorDamage.ApplyPoisonDamage(player, 20);
        Assert.Equal(30, player.Health);
        Assert.Equal(new HealthPickupStack(2, 5, 3), Assert.Single(player.Inventory.HealthPickups));
    }

    [Theory]
    [InlineData(60, 20, 20, 2, 40, 1)]
    [InlineData(10, 20, 25, 3, 45, 0)]
    [InlineData(10, 40, 20, 3, -50, 2)]
    [InlineData(60, 10, 20, 2, 50, 2)]
    public void PreservesNativeMirroringAndSecondSubtraction(int initial, int damage, int healing, int amount, int expected, int remaining)
    {
        var player = Player(); player.Health = initial; player.Inventory.AddHealthPickup(healing, amount, 3);
        var result = ActorDamage.ApplyPoisonDamage(player, damage);
        Assert.Equal(expected, player.Health); Assert.Equal(expected <= 0, result.Killed);
        Assert.Equal(remaining, player.Inventory.HealthPickups.Sum(i => i.Amount));
        Assert.Equal(0, player.DamageCount);
    }

    [Theory]
    [InlineData(true, SpawnGameMode.Single)]
    [InlineData(false, SpawnGameMode.Deathmatch)]
    public void DisabledAndDeathmatchKeepModeThreeItems(bool disabled, SpawnGameMode mode)
    {
        var player = Player(disabled, mode); player.Health = 60; player.Inventory.AddHealthPickup(20, 2, 3);
        ActorDamage.ApplyPoisonDamage(player, 20);
        Assert.Equal(40, player.Health); Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Fact]
    public void BuddhaAppliesAfterMirroredHealingStillLeavesLethalActorHealth()
    {
        var player = Player(); player.Health = 10; player.Buddha = true; player.Inventory.AddHealthPickup(20, 3, 3);
        Assert.False(ActorDamage.ApplyPoisonDamage(player, 40).Killed);
        Assert.Equal(1, player.Health); Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    private static PlayerPawn Player(bool disabled = false, SpawnGameMode mode = SpawnGameMode.Single)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1, Single = true, Coop = true, Deathmatch = true }] },
            spawnOptions: new SpawnOptions(Mode: mode, DisableAutoHealth: disabled));
        return sim.Players.Single();
    }
}
