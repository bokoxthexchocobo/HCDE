using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProximityItemParentTests
{
    [Theory]
    [InlineData(PickupCatalog.Stimpack, "Health")]
    [InlineData(PickupCatalog.Medikit, "Health")]
    [InlineData(PickupCatalog.HealthBonus, "Health")]
    [InlineData(PickupCatalog.Soulsphere, "Health")]
    [InlineData(PickupCatalog.ArmorBonus, "BasicArmorBonus")]
    [InlineData(PickupCatalog.ArmorBonus, "Armor")]
    [InlineData(PickupCatalog.GreenArmor, "BasicArmorPickup")]
    [InlineData(PickupCatalog.MegaArmor, "Armor")]
    [InlineData(PickupCatalog.BlueCard, "DoomKey")]
    [InlineData(PickupCatalog.YellowSkull, "Key")]
    [InlineData(PickupCatalog.Backpack, "BackpackItem")]
    [InlineData(PickupCatalog.Megasphere, "CustomInventory")]
    [InlineData(PickupCatalog.Megasphere, "StateProvider")]
    [InlineData(PickupCatalog.Berserk, "CustomInventory")]
    [InlineData(PickupCatalog.Berserk, "StateProvider")]
    [InlineData(PickupCatalog.RadSuit, "PowerupGiver")]
    public void DoomPickupParentsMatchOnlyWithAncestryAcrossFreshSave(int type, string parent)
    {
        var sim = Room(type); var saved = SimSavegame.Write(sim, true); var fresh = Room(type); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            Assert.Equal(0, ActorProximity.CountProximity(world.Players.Single(), parent, 100));
            Assert.Equal(1, ActorProximity.CountProximity(world.Players.Single(), parent.ToLowerInvariant(), 100, flags: 1));
        }
    }

    [Theory]
    [InlineData(false, PickupCatalog.Stimpack)]
    [InlineData(false, PickupCatalog.Medikit)]
    [InlineData(false, PickupCatalog.Megasphere)]
    [InlineData(true, 82)]
    [InlineData(true, 32)]
    public void CarriedHealthItemsInheritHealthPickupInsteadOfHealth(bool raven, int type)
    {
        var sim = Room(type, strife: !raven, raven: raven); var saved = SimSavegame.Write(sim, true);
        var fresh = Room(type, strife: !raven, raven: raven); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var player = world.Players.Single();
            Assert.Equal(1, ActorProximity.CountProximity(player, "HealthPickup", 100, flags: 1));
            Assert.Equal(0, ActorProximity.CountProximity(player, "Health", 100, flags: 1));
            Assert.Equal(0, ActorProximity.CountProximity(player, "CustomInventory", 100, flags: 1));
        }
    }

    [Theory]
    [InlineData(PickupCatalog.Berserk, "PowerupGiver")]
    [InlineData(PickupCatalog.Megasphere, "Health")]
    [InlineData(PickupCatalog.RadSuit, "CustomInventory")]
    [InlineData(PickupCatalog.GreenArmor, "BasicArmorBonus")]
    [InlineData(PickupCatalog.ArmorBonus, "BasicArmorPickup")]
    public void SimilarPickupBehaviorDoesNotImplySharedParent(int type, string query)
    {
        var sim = Room(type); Assert.Equal(0, ActorProximity.CountProximity(sim.Players.Single(), query, 100, flags: 1));
    }

    private static AuthoritySimulation Room(int type, bool strife = false, bool raven = false)
        => AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 50 }],
            Sectors = [new LevelSector { CeilingHeight = 128 }],
        }, spawnOptions: new SpawnOptions(StrifeHealthPickups: strife, RavenHealthPickups: raven));
}
