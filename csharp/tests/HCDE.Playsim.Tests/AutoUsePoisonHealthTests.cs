using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AutoUsePoisonHealthTests
{
    [Theory]
    [InlineData(true, 20, 25, 2, 10)]
    [InlineData(false, 20, 20, 3, 8)]
    [InlineData(true, 110, 115, 0, 6)]
    [InlineData(true, 200, 0, 3, 10)]
    public void NativeGroupPriorityAndCombinedUse(bool skill, int required, int gain, int firstLeft, int secondLeft)
    {
        var inventory = new PlayerInventory(); inventory.AddHealthPickup(25, 3, 1); inventory.AddHealthPickup(10, 10, 2);
        Assert.Equal(gain, inventory.AutoUseHealth(required, skill));
        Assert.Equal(firstLeft, inventory.HealthPickups.Where(i => i.AutoUseMode == 1).Sum(i => i.Amount));
        Assert.Equal(secondLeft, inventory.HealthPickups.Where(i => i.AutoUseMode == 2).Sum(i => i.Amount));
    }
    [Fact]
    public void LargestPerItemIsConsumedFirstAndModeThreeIsExcluded()
    {
        var inventory = new PlayerInventory(); inventory.AddHealthPickup(10, 3, 1);
        inventory.AddHealthPickup(25, 2, 1); inventory.AddHealthPickup(100, 1, 3);
        Assert.Equal(50, inventory.AutoUseHealth(30, true));
        Assert.Equal(new HealthPickupStack(10, 3, 1), inventory.HealthPickups[0]);
        Assert.Equal(new HealthPickupStack(100, 1, 3), inventory.HealthPickups[1]);
    }
    [Fact]
    public void PoisonUsesStoredHealthBeforeLethalSubtractionAndSavePreservesRemainingStack()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }] }, spawnOptions: new SpawnOptions(AutoUseHealth: true));
        var player = sim.Players.Single(); player.Health = 1;
        var legacy = SimSavegame.Write(sim); player.Inventory.AddHealthPickup(25, 2, 1);
        var saved = SimSavegame.Write(sim);
        var damage = ActorDamage.ApplyPoisonDamage(player, 10);
        Assert.False(damage.Killed); Assert.Equal(16, player.Health);
        Assert.Equal(1, Assert.Single(player.Inventory.HealthPickups).Amount);
        SimSavegame.Apply(sim, saved); Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount);
        Assert.Equal(saved, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, legacy); Assert.Empty(player.Inventory.HealthPickups);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 12), 0);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-health-pickups-invalid", error);
    }

    [Fact]
    public void InventoryResetClearsHealthAndInvalidStacksAreRejected()
    {
        var inventory = new PlayerInventory(); inventory.AddHealthPickup(25, 1, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.AddHealthPickup(0, 1, 1));
        inventory.ResetToPistolStart(); Assert.Empty(inventory.HealthPickups);
    }

    [Fact]
    public void DisabledSinglePlayerSkillDoesNotAutoHeal()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); player.Health = 1; player.Inventory.AddHealthPickup(100, 1, 2);
        Assert.True(ActorDamage.ApplyPoisonDamage(player, 2).Killed);
        Assert.Single(player.Inventory.HealthPickups);
    }
}
