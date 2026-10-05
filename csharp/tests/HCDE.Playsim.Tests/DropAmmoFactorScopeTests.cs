using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DropAmmoFactorScopeTests
{
    [Theory]
    [InlineData(PickupCatalog.GreenArmor)]
    [InlineData(PickupCatalog.Backpack)]
    [InlineData(PickupCatalog.Stimpack)]
    public void AmmoDropFactorDoesNotChangeOtherPickupFlags(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        }, spawnOptions: new SpawnOptions(Skill: 0, ArmorFactor: 0.5));
        sim.DropAmmoFactor = 0;
        var player = sim.Players.Single();
        player.Health = 50;
        Assert.True(sim.SpawnDroppedPickup(player, type));
        var drop = sim.Actors[^1];
        Assert.False(drop.IgnoreAmmoSkill);
        Assert.False(drop.SuppressWeaponPickupAmmo);
        Assert.True(PickupCatalog.TryGive(player, type, ignoreSkill: drop.IgnoreAmmoSkill));
        if (type == PickupCatalog.GreenArmor) Assert.Equal(50, player.Inventory.Armor);
        if (type == PickupCatalog.Backpack) Assert.Equal(70, player.Inventory.Bullets);
        if (type == PickupCatalog.Stimpack) Assert.Equal(60, player.Health);
    }
}
