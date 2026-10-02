using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AlwaysPickupBonusTests
{
    [Theory]
    [InlineData(PickupCatalog.HealthBonus, 200)]
    [InlineData(PickupCatalog.HealthBonus, 250)]
    [InlineData(PickupCatalog.ArmorBonus, 200)]
    [InlineData(PickupCatalog.ArmorBonus, 250)]
    public void BonusIsConsumedAtOrAboveCapWithoutReducingCurrentValue(int type, int value)
    {
        var sim = Room(type); var player = sim.Players.Single(); var item = sim.Actors[^1];
        if (type == PickupCatalog.HealthBonus) player.Health = value;
        else player.Inventory.Armor = value;
        sim.Tick();
        Assert.True(item.Destroyed); Assert.DoesNotContain(item, sim.Actors);
        Assert.Equal(value, type == PickupCatalog.HealthBonus ? player.Health : player.Inventory.Armor);
    }

    [Theory]
    [InlineData(PickupCatalog.Stimpack)]
    [InlineData(PickupCatalog.Medikit)]
    [InlineData(PickupCatalog.GreenArmor)]
    [InlineData(PickupCatalog.MegaArmor)]
    public void OrdinaryPickupStillRemainsWhenItOffersNoBenefit(int type)
    {
        var sim = Room(type); var player = sim.Players.Single(); var item = sim.Actors[^1];
        player.Health = 200; player.Inventory.Armor = 200;
        sim.Tick(); Assert.False(item.Destroyed); Assert.Contains(item, sim.Actors);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type }],
    });
}
