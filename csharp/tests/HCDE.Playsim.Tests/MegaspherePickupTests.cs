using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MegaspherePickupTests
{
    [Fact]
    public void GrantsHealthAndDistinctBlueArmor()
    {
        var player = new PlayerPawn { Health = 20 };
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Megasphere));
        Assert.Equal(200, player.Health);
        Assert.Equal(200, player.Inventory.Armor);
        Assert.Equal(200, player.Inventory.ArmorMaximum);
        Assert.Equal(200, player.Inventory.ArmorActualSaveAmount);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.Equal("BlueArmorForMegasphere", player.Inventory.ArmorType);
    }

    [Theory]
    [InlineData(250, 0, true)]
    [InlineData(20, 250, true)]
    [InlineData(250, 250, false)]
    public void DoesNotReduceExistingHealthOrArmor(int health, int armor, bool changed)
    {
        var player = new PlayerPawn { Health = health };
        player.Inventory.Armor = armor;
        Assert.Equal(changed, PickupCatalog.TryGive(player, PickupCatalog.Megasphere));
        Assert.Equal(Math.Max(health, 200), player.Health);
        Assert.Equal(Math.Max(armor, 200), player.Inventory.Armor);
    }

    [Theory]
    [InlineData(0.5, 110)]
    [InlineData(2, 200)]
    public void HealthGrantUsesSkillFactorAndExplicitLimit(double factor, int expected)
    {
        var sim = Room(factor); var player = sim.Players.Single();
        player.Health = 10; player.MaxHealth = 50;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Megasphere));
        Assert.Equal(expected, player.Health);
        Assert.Equal(200, player.Inventory.Armor);
    }

    [Fact]
    public void FullPlayerStillConsumesMapPickup()
    {
        var sim = Room(1); var player = sim.Players.Single();
        player.Health = 200; player.Inventory.Armor = 200;
        var pickup = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Megasphere);
        sim.Tick();
        Assert.True(pickup.Destroyed);
        Assert.Equal(200, player.Health);
        Assert.Equal(200, player.Inventory.Armor);
    }

    [Fact]
    public void DropClassLookupRecognizesMegasphere()
    {
        Assert.True(PickupCatalog.TryEditorNumberForDropName("MegaSphere", out var type));
        Assert.Equal(83, type);
        Assert.True(PickupCatalog.IsPickup(type));
    }

    private static AuthoritySimulation Room(double factor) => AuthoritySimulation.Start(new PlayLevel
    {
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Megasphere }],
    }, spawnOptions: new SpawnOptions(HealthFactor: factor));
}
