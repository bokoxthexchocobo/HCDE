using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AutoUseStrifeHealthTests
{
    [Fact]
    public void DisabledAutoHealthChangesSimulationChecksum()
    {
        var level = new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1 }] };
        var enabled = AuthoritySimulation.Start(level);
        var disabled = AuthoritySimulation.Start(level, spawnOptions: new SpawnOptions(DisableAutoHealth: true));
        Assert.NotEqual(enabled.Checksum, disabled.Checksum);
    }
    [Theory]
    [InlineData(49, 69, 2)]
    [InlineData(50, 50, 3)]
    [InlineData(0, 0, 3)]
    public void ThresholdAndDeadHealth(int health, int expected, int remaining)
    {
        var inventory = new PlayerInventory(); inventory.AddHealthPickup(20, 3, 3);
        Assert.Equal(expected, inventory.AutoUseStrifeHealth(health, 100));
        Assert.Equal(remaining, Assert.Single(inventory.HealthPickups).Amount);
    }

    [Fact]
    public void PreservesNativeAmountComparisonWhenChoosingStack()
    {
        var inventory = new PlayerInventory(); inventory.AddHealthPickup(100, 1, 3); inventory.AddHealthPickup(10, 1, 3);
        Assert.Equal(50, inventory.AutoUseStrifeHealth(40, 100));
        Assert.Equal(100, Assert.Single(inventory.HealthPickups).Health);
    }

    [Fact]
    public void AppliesSkillFactorAndMaximumWithoutUsingOtherModes()
    {
        var inventory = new PlayerInventory(); inventory.AddHealthPickup(100, 1, 1); inventory.AddHealthPickup(10, 10, 3);
        Assert.Equal(45, inventory.AutoUseStrifeHealth(40, 45, .5));
        Assert.Equal(9, inventory.HealthPickups[1].Amount);
    }

    [Theory]
    [InlineData(SpawnGameMode.Single, DamageFlags.None, 60)]
    [InlineData(SpawnGameMode.Deathmatch, DamageFlags.None, 40)]
    [InlineData(SpawnGameMode.Single, DamageFlags.Forced, 40)]
    public void OrdinaryDamageUsesModeThreeAfterSubtraction(SpawnGameMode mode, DamageFlags flags, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1, Single = true, Coop = true, Deathmatch = true }] },
            spawnOptions: new SpawnOptions(Mode: mode));
        var player = sim.Players.Single(); player.Health = 60; player.Inventory.AddHealthPickup(20, 2, 3);
        ActorDamage.Apply(player, 20, flags: flags);
        Assert.Equal(expected, player.Health);
        Assert.Equal(expected == 60 ? 1 : 2, Assert.Single(player.Inventory.HealthPickups).Amount);
        Assert.Equal(20, player.DamageCount);
    }
}
