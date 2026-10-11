using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PercentageHealthPickupTests
{
    [Theory]
    [InlineData(-100, 150)]
    [InlineData(-50, 75)]
    public void PercentageItemsUsePickupMaximumAndIgnoreSkillScaling(int healing, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 40; player.MaxPickupHealth = 150;
        player.Inventory.AddHealthPickup(healing, 2);
        Assert.True(player.Inventory.UseHealthPickup(0)); Assert.Equal(expected, player.Health);
        Assert.Equal(1, Assert.Single(player.Inventory.HealthPickups).Amount);
        Assert.False(player.Inventory.UseHealthPickup(0)); Assert.Single(player.Inventory.HealthPickups);
    }

    [Fact]
    public void SurgeryKitPercentageSurvivesSaveRestoreAndUsesUpgradedMaximum()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 40; player.Stamina = 10; player.BonusHealth = 20;
        player.Inventory.AddHealthPickup(-100, 1);
        var bytes = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(-100, Assert.Single(player.Inventory.HealthPickups).Health);
        Assert.True(player.Inventory.UseHealthPickup(0)); Assert.Equal(130, player.Health);
        Assert.Empty(player.Inventory.HealthPickups);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AutomaticDamageCannotUsePercentageItems(bool poison)
    {
        var player = Room().Players.Single(); player.Health = 10; player.Inventory.AddHealthPickup(-100, 2);
        var result = poison ? ActorDamage.ApplyPoisonDamage(player, 20) : ActorDamage.Apply(player, 20);
        Assert.True(result.Killed); Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount);
    }

    [Fact]
    public void MalformedAutomaticPercentageStackIsRejectedOnRead()
    {
        var sim = Room(); sim.Players.Single().Inventory.AddHealthPickup(-100, 1);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 20), 3);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-health-pickups-invalid", error);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void UnsupportedAutomaticPercentageModesAreRejected(int mode)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerInventory().AddHealthPickup(-100, 1, mode));
        var state = Room().CaptureState(); state.Actors.Single(a => a.HealthPickups is not null).HealthPickups = [new(-100, 1, mode)];
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(HealthFactor: .25, AutoUseHealth: true));
}
