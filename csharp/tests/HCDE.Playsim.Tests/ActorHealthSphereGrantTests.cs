using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorHealthSphereGrantTests
{
    [Theory]
    [InlineData("HealthBonus", 199, 200)]
    [InlineData("Soulsphere", 150, 200)]
    public void GrantsUseExplicitCapsAndFailWhenFull(string name, int initial, int expected)
    {
        var player = Room().Players.Single(); player.Health = initial;
        Assert.True(ActorInventoryActions.GiveInventory(player, name)); Assert.Equal(expected, player.Health);
        Assert.False(ActorInventoryActions.GiveInventory(player, name)); Assert.Empty(player.Inventory.HealthPickups);
    }

    [Fact]
    public void SphereMultipliesHealingAndAppliesBonusMaximum()
    {
        var player = Room().Players.Single(); player.Health = 10; player.BonusHealth = 50;
        Assert.True(ActorInventoryActions.GiveInventory(player, "Soulsphere", 2)); Assert.Equal(210, player.Health);
        Assert.True(ActorInventoryActions.GiveInventory(player, "Soulsphere")); Assert.Equal(250, player.Health);
    }

    [Fact]
    public void PatchedSphereDefaultsMatchWorldPickup()
    {
        var sim = Room("Misc 0\nSoulsphere Health = 30\nMax Soulsphere = 180\n");
        var player = sim.Players.Single(); player.Health = 100;
        Assert.True(ActorInventoryActions.GiveInventory(player, "Soulsphere", 2)); Assert.Equal(160, player.Health);
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Soulsphere)); Assert.Equal(180, player.Health);
    }

    [Theory]
    [InlineData(false, 300)]
    [InlineData(true, 150)]
    public void PatchedBonusCapHonorsCompatibility(bool compatible, int expected)
    {
        var sim = Room("Misc 0\nMax Health = 150\n", compatible ? CompatSurface.DehHealth : CompatSurface.None);
        var player = sim.Players.Single(); player.Health = 100;
        Assert.True(ActorInventoryActions.GiveInventory(player, "HealthBonus", 1000)); Assert.Equal(expected, player.Health);
        Assert.False(PickupCatalog.TryGive(player, PickupCatalog.HealthBonus));
    }

    private static AuthoritySimulation Room(string? patch = null, CompatSurface compat = CompatSurface.None)
        => AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            dehacked: patch is null ? null : DehackedPatch.Apply(patch), compat: compat);
}
