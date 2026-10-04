using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArmorSkillFactorTests
{
    [Theory]
    [InlineData(2, 200)]
    [InlineData(0.555, 55)]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    public void SuitPickupsAndScriptGrantsUseScaledSaveAmount(double factor, int expected)
    {
        var player = Room(factor).Players.Single();
        Assert.Equal(expected > 0, PickupCatalog.TryGive(player, PickupCatalog.GreenArmor));
        Assert.Equal(expected, player.Inventory.Armor);
        player.Inventory.Armor = 0;
        AcsPlayerInventory.Give(player, "GreenArmor", 1);
        Assert.Equal(expected, player.Inventory.Armor);
        if (expected > 0) Assert.Equal(expected, player.Inventory.ArmorActualSaveAmount);
    }

    [Fact]
    public void ReplacementComparesScaledAmountToWornArmor()
    {
        var player = Room(0.5).Players.Single();
        player.Inventory.TryKeepArmorPickup(120, 25, armorType: "Existing");
        Assert.False(PickupCatalog.TryGive(player, PickupCatalog.GreenArmor));
        Assert.Equal("Existing", player.Inventory.ArmorType);
        AcsPlayerInventory.Give(player, "GreenArmor", 2);
        Assert.Equal(100, player.Inventory.Armor);
        Assert.Equal("GreenArmor", player.Inventory.ArmorType);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(2)]
    public void IgnoreSkillBypassesSuitScaling(double factor)
    {
        var player = Room(factor).Players.Single();
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.MegaArmor, ignoreSkill: true));
        Assert.Equal(200, player.Inventory.Armor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonpositiveBonusGrantIsConsumedWithoutChangingMetadata(double factor)
    {
        var player = Room(factor).Players.Single();
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus));
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal("None", player.Inventory.ArmorType);
        Assert.Equal(0, player.Inventory.ArmorActualSaveAmount);
    }

    [Fact]
    public void BonusScalingTruncatesAndRetainsBonusCap()
    {
        var player = Room(1.5).Players.Single();
        AcsPlayerInventory.Give(player, "ArmorBonus", 3);
        Assert.Equal(4, player.Inventory.Armor);
        Assert.Equal(200, player.Inventory.ArmorActualSaveAmount);
        player.Inventory.Armor = 199;
        AcsPlayerInventory.Give(player, "ArmorBonus", 3);
        Assert.Equal(200, player.Inventory.Armor);
    }

    [Fact]
    public void MegasphereAndItsHelperUseArmorFactorIndependentlyOfHealth()
    {
        var player = Room(0.5).Players.Single();
        player.Health = 1;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Megasphere));
        Assert.Equal(100, player.Inventory.Armor);
        Assert.Equal(200, player.Health);
        player.Inventory.Armor = 0;
        AcsPlayerInventory.Give(player, "BlueArmorForMegasphere", 2);
        Assert.Equal(200, player.Inventory.Armor);
    }

    [Fact]
    public void FactorAffectsChecksumAndRemainsLaunchConfigurationAfterRestore()
    {
        var normal = Room(1); var scaled = Room(2);
        Assert.NotEqual(normal.Checksum, scaled.Checksum);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(scaled), out var state, out var error), error);
        scaled.RestoreState(state);
        Assert.Equal(2, scaled.ArmorFactor);
        Assert.True(PickupCatalog.TryGive(scaled.Players.Single(), PickupCatalog.GreenArmor));
        Assert.Equal(200, scaled.Players.Single().Inventory.Armor);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonfiniteFactorsAreRejected(double factor) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Room(factor));

    private static AuthoritySimulation Room(double factor) => AuthoritySimulation.Start(new PlayLevel
    {
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(ArmorFactor: factor));
}
