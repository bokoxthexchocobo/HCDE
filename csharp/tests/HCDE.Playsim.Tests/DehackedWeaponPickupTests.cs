using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedWeaponPickupTests
{
    [Theory]
    [InlineData(73, 2006, "BFG9000", WeaponKind.Bfg, AmmoKind.Cells, 40)]
    [InlineData(74, 2002, "Chaingun", WeaponKind.Chaingun, AmmoKind.Bullets, 20)]
    [InlineData(75, 2005, "Chainsaw", WeaponKind.Chainsaw, AmmoKind.Bullets, 0)]
    [InlineData(76, 2003, "RocketLauncher", WeaponKind.RocketLauncher, AmmoKind.Rockets, 2)]
    [InlineData(77, 2004, "PlasmaRifle", WeaponKind.Plasma, AmmoKind.Cells, 40)]
    [InlineData(78, 2001, "Shotgun", WeaponKind.Shotgun, AmmoKind.Shells, 8)]
    [InlineData(79, 82, "SuperShotgun", WeaponKind.SuperShotgun, AmmoKind.Shells, 8)]
    public void NativeIdentityWeaponGrantAndDestroyedSaveReconstruction(int index, ushort editor, string name,
        WeaponKind weapon, AmmoKind ammo, int amount)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index); Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(editor, patch); var item = Item(sim); var player = sim.Players.Single(); player.Inventory.Bullets = 0;
        Assert.Equal(editor == 2006 ? 20 : 16, item.Height.ToDouble()); Assert.False(item.Solid); Assert.False(item.Shootable);
        var saved = SimSavegame.Write(sim, true); item.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(item, Item(sim)); var fresh = Room(editor, patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        sim.Players.Single().X = Fixed.FromInt(200); fresh.Players.Single().X = Fixed.FromInt(200); sim.Tick(); fresh.Tick();
        Assert.True(player.Inventory.Owns(weapon)); Assert.Equal(amount, player.Inventory.Ammo(ammo));
        Assert.DoesNotContain(sim.Actors, a => a.MapPickupThingIndex.HasValue); Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void CooperativePatchedWeaponStaysWithoutRepeatedAmmo()
    {
        var patch = DehackedPatch.Apply("Thing 78\nMass = 200\n"); var sim = Room(2001, patch, SpawnGameMode.Cooperative);
        var player = sim.Players.Single(); player.X = Fixed.FromInt(200); sim.Tick(); Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.Equal(8, player.Inventory.Shells); Assert.NotNull(Item(sim)); sim.Tick(); Assert.Equal(8, player.Inventory.Shells);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(2001, patch, SpawnGameMode.Cooperative); SimSavegame.Apply(fresh, saved);
        fresh.Tick(); Assert.Equal(8, fresh.Players.Single().Inventory.Shells); Assert.NotNull(Item(fresh));
    }

    [Fact]
    public void BfgSharedStockHeightAndExplicitPatchHeightAreDistinct()
    {
        Assert.Equal(20, Item(Room(2006, null)).Height.ToDouble());
        var patch = DehackedPatch.Apply("Thing 73\nHeight = 2097152\n"); Assert.Equal(32, Item(Room(2006, patch)).Height.ToDouble());
    }

    [Fact]
    public void LayeringRetainsDimensionsAndPrimaryReplacementCanDisablePickup()
    {
        var original = DehackedPatch.Apply("Thing 78\nWidth = 786432\n");
        var inherited = DehackedPatch.Apply("Thing 78\nMass = 200\n", original);
        var changed = DehackedPatch.Apply("Thing 78\nBits = SOLID\n", inherited); Assert.Empty(changed.Errors);
        var actor = Room(2001, changed).Actors.Single(a => a is not PlayerPawn);
        Assert.True(actor.Solid); Assert.False(actor.SpecialPickup); Assert.Equal(12, actor.Radius.ToDouble());
        Assert.True(Item(Room(2001, original)).SpecialPickup);
    }

    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(ushort type, DehackedPatchResult? patch, SpawnGameMode mode = SpawnGameMode.Single) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch, spawnOptions: new SpawnOptions(Mode: mode));
}
