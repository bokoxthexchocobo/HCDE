using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AlwaysPickupOverrideTests
{
    [Theory]
    [InlineData(PickupCatalog.HealthBonus)]
    [InlineData(PickupCatalog.ArmorBonus)]
    [InlineData(PickupCatalog.Megasphere)]
    public void ClearingDefaultAlwaysPickupLeavesFullCapacityItem(int type)
    {
        var sim = Room(type); var player = sim.Players.Single(); var item = sim.Actors[^1];
        player.Health = 200; player.Inventory.Armor = 200;
        Assert.True(AcsActorFlags.TryGet(item, "ALWAYSPICKUP", out var initial));
        Assert.True(initial);
        Assert.True(AcsActorFlags.TrySet(item, "inventory.alwayspickup", false));
        sim.Tick();
        Assert.Contains(item, sim.Actors);
        Assert.Equal(200, player.Health);
        Assert.Equal(200, player.Inventory.Armor);
    }

    [Fact]
    public void EnablingAlwaysPickupConsumesFullAmmoWithoutChangingAmount()
    {
        var sim = Room(PickupCatalog.Clip); var player = sim.Players.Single(); var item = sim.Actors[^1];
        player.Inventory.Bullets = 200;
        Assert.True(AcsActorFlags.TrySet(item, "ALWAYSPICKUP", true));
        sim.Tick();
        Assert.DoesNotContain(item, sim.Actors);
        Assert.Equal(200, player.Inventory.Bullets);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SavedOverrideRestoresBothValuesAndChecksum(bool value)
    {
        var sim = Room(PickupCatalog.Clip); var player = sim.Players.Single(); var item = sim.Actors[^1];
        player.X = Fixed.FromInt(500);
        AcsActorFlags.TrySet(item, "ALWAYSPICKUP", value);
        sim.Tick(); var checksum = sim.Checksum;
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        item.AlwaysPickupOverride = !value;
        sim.RestoreState(state);
        Assert.Equal(value, item.AlwaysPickupOverride);
        Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void NonInventoryActorRejectsAlwaysPickup()
    {
        Assert.False(AcsActorFlags.TrySet(new PlayerPawn(), "ALWAYSPICKUP", true));
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type }],
    });
}
