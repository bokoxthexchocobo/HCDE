using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PistolPickupTests
{
    [Theory]
    [InlineData(2, 20)]
    [InlineData(0, 40)]
    public void MapPistolUsesNativeAmmoGiveAndSkill(int skill, int bullets)
    {
        var sim = Room(skill, true); var player = sim.Players.Single();
        AcsPlayerInventory.Clear(player);
        sim.Tick();
        Assert.True(player.Inventory.Owns(WeaponKind.Pistol));
        Assert.Equal(bullets, player.Inventory.Bullets);
        Assert.DoesNotContain(sim.Actors, a => a.DoomEdNum == PickupCatalog.Pistol);
    }

    [Fact]
    public void DuplicatePistolPickupGivesAmmoWithoutChangingSelection()
    {
        var player = new PlayerPawn();
        player.Inventory.Pending = WeaponKind.Shotgun;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Pistol));
        Assert.Equal(70, player.Inventory.Bullets);
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }

    [Fact]
    public void FullAmmoLeavesOwnedPistolPickupUntaken()
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = player.Inventory.MaxBullets;
        Assert.False(PickupCatalog.TryGive(player, PickupCatalog.Pistol));
    }

    [Fact]
    public void DropItemResolvesPistolClassAndUsesDroppedAmmoAmount()
    {
        var sim = Room(2); var player = sim.Players.Single();
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, player, "pistol", 0, 256));
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Pistol);
        Assert.Equal(10, drop.PickupAmount);
        Assert.False(drop.SuppressWeaponPickupAmmo);
    }

    private static AuthoritySimulation Room(int skill, bool pistol = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = pistol
            ? [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Pistol }]
            : [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(Skill: skill));
}
