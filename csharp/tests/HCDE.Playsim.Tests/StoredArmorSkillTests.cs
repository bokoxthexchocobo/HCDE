using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StoredArmorSkillTests
{
    [Theory]
    [InlineData(0.5, 40)]
    [InlineData(1.555, 124)]
    [InlineData(2, 160)]
    public void ExplicitUseScalesReserveAtActivation(double factor, int expected)
    {
        var player = Player(factor);
        Assert.Equal(80, player.Inventory.SpareArmor.Single().SaveAmount);
        player.Inventory.Armor = 1;
        Assert.Equal(1, AcsPlayerInventory.Use(player, "Reserve"));
        Assert.Equal(expected, player.Inventory.Armor);
        Assert.Equal(expected, player.Inventory.ArmorMaximum);
        Assert.Equal(expected, player.Inventory.ArmorActualSaveAmount);
        Assert.Equal("Reserve", player.Inventory.ArmorType);
        Assert.Empty(player.Inventory.SpareArmor);
    }

    [Fact]
    public void UseChecksScaledAmountBeforeConsumingReserve()
    {
        var player = Player(0.5);
        player.Inventory.Armor = 50;
        Assert.Equal(0, AcsPlayerInventory.Use(player, "Reserve"));
        Assert.Single(player.Inventory.SpareArmor);
        Assert.Equal(50, player.Inventory.Armor);
    }

    [Theory]
    [InlineData(0.5, 40)]
    [InlineData(2, 160)]
    public void DamagePromotionScalesReserve(double factor, int expected)
    {
        var player = Player(factor);
        player.Inventory.Armor = 1;
        player.Inventory.ArmorSavePercent = 100;
        ActorDamage.Apply(player, 1);
        Assert.Equal(expected, player.Inventory.Armor);
        Assert.Equal("Reserve", player.Inventory.ArmorType);
        Assert.Empty(player.Inventory.SpareArmor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonpositiveScaledReserveRemainsAfterFailedUseAndPromotion(double factor)
    {
        var player = Player(factor);
        player.Inventory.Armor = 0;
        Assert.Equal(0, AcsPlayerInventory.Use(player, "Reserve"));
        ActorDamage.Apply(player, 1);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal("None", player.Inventory.ArmorType);
        Assert.Single(player.Inventory.SpareArmor);
    }

    private static PlayerPawn Player(double factor)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        }, spawnOptions: new SpawnOptions(ArmorFactor: factor));
        var player = sim.Players.Single();
        player.Inventory.Armor = 100;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "Reserve");
        return player;
    }
}
