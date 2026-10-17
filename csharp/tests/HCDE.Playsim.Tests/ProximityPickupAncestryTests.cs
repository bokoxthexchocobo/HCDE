using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProximityPickupAncestryTests
{
    [Theory]
    [InlineData(PickupCatalog.ClipBox, "Clip")]
    [InlineData(PickupCatalog.ShellBox, "Shell")]
    [InlineData(PickupCatalog.RocketBox, "RocketAmmo")]
    [InlineData(PickupCatalog.CellPack, "Cell")]
    [InlineData(PickupCatalog.Clip, "Ammo")]
    [InlineData(PickupCatalog.ClipBox, "Ammo")]
    [InlineData(PickupCatalog.Shotgun, "Weapon")]
    [InlineData(PickupCatalog.Shotgun, "DoomWeapon")]
    [InlineData(PickupCatalog.Chainsaw, "Weapon")]
    [InlineData(PickupCatalog.Pistol, "StateProvider")]
    [InlineData(PickupCatalog.GreenArmor, "Inventory")]
    [InlineData(PickupCatalog.Backpack, "Inventory")]
    public void PickupParentQueriesRequireAncestryFlagAcrossFreshSave(int type, string parent)
    {
        var sim = Room(type); var saved = SimSavegame.Write(sim, true); var fresh = Room(type); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var player = world.Players.Single();
            Assert.Equal(0, ActorProximity.CountProximity(player, parent, 100));
            Assert.Equal(1, ActorProximity.CountProximity(player, parent.ToLowerInvariant(), 100, flags: 1));
            Assert.True(ActorProximity.Check(player, parent, 100, flags: 1 | 64));
            Assert.Equal(world.Actors.Single(a => a.DoomEdNum == type).Id, player.TargetId);
        }
    }

    [Theory]
    [InlineData(PickupCatalog.Chainsaw, "DoomWeapon")]
    [InlineData(PickupCatalog.Backpack, "Ammo")]
    [InlineData(PickupCatalog.GreenArmor, "Weapon")]
    [InlineData(PickupCatalog.Clip, "ClipBox")]
    public void UnrelatedAndDerivedClassesDoNotMatchAncestorQueries(int type, string query)
    {
        var sim = Room(type); Assert.Equal(0, ActorProximity.CountProximity(sim.Players.Single(), query, 100, flags: 1));
    }

    [Fact]
    public void AbsentInventoryParentSupportsZeroCountChecks()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] });
        Assert.True(ActorProximity.Check(sim.Players.Single(), "Inventory", 100, count: 0, flags: 1));
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 50 }],
        Sectors = [new LevelSector { CeilingHeight = 128 }],
    });
}
