using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StoredArmorIgnoreSkillTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(2)]
    public void InitialActivationBypassesSkillFactor(double factor)
    {
        var player = Room(factor).Players.Single();
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "Reserve", ignoreSkill: true);
        Assert.Equal(80, player.Inventory.Armor);
        Assert.Equal(80, player.Inventory.ArmorActualSaveAmount);
        Assert.Empty(player.Inventory.SpareArmor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void StoredFlagSurvivesUntilExplicitUse(double factor)
    {
        var player = Room(factor).Players.Single();
        player.Inventory.Armor = 100;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "Reserve", ignoreSkill: true);
        Assert.True(player.Inventory.SpareArmor.Single().IgnoreSkill);
        player.Inventory.Armor = 60;
        Assert.Equal(1, AcsPlayerInventory.Use(player, "Reserve"));
        Assert.Equal(80, player.Inventory.Armor);
        Assert.Empty(player.Inventory.SpareArmor);
    }

    [Fact]
    public void UseRejectsEqualWornAmountEvenWhenFactorWouldIncreaseIt()
    {
        var player = Room(2).Players.Single();
        player.Inventory.Armor = 80;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "Reserve", ignoreSkill: true);
        Assert.Equal(0, AcsPlayerInventory.Use(player, "Reserve"));
        Assert.Single(player.Inventory.SpareArmor);
    }

    [Fact]
    public void DepletionPromotionBypassesZeroFactor()
    {
        var player = Room(0).Players.Single();
        player.Inventory.Armor = 1;
        player.Inventory.ArmorSavePercent = 100;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "Reserve", ignoreSkill: true);
        ActorDamage.Apply(player, 1);
        Assert.Equal(80, player.Inventory.Armor);
        Assert.Equal("Reserve", player.Inventory.ArmorType);
        Assert.Empty(player.Inventory.SpareArmor);
    }

    [Fact]
    public void StoredFlagChangesChecksumBeforeActivation()
    {
        var normal = Room(1); var ignored = Room(1);
        foreach (var sim in new[] { normal, ignored }) sim.Players.Single().Inventory.Armor = 100;
        normal.Players.Single().Inventory.TryKeepArmorPickup(80, 50);
        ignored.Players.Single().Inventory.TryKeepArmorPickup(80, 50, ignoreSkill: true);
        normal.Tick();
        ignored.Tick();
        Assert.NotEqual(normal.Checksum, ignored.Checksum);
    }

    private static AuthoritySimulation Room(double factor) => AuthoritySimulation.Start(new PlayLevel
    {
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(ArmorFactor: factor));
}
