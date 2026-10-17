using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedAmmoIdentityTests
{
    [Theory]
    [InlineData(64, 2007, "Clip", AmmoKind.Bullets, 10)]
    [InlineData(65, 2048, "ClipBox", AmmoKind.Bullets, 50)]
    [InlineData(66, 2010, "RocketAmmo", AmmoKind.Rockets, 1)]
    [InlineData(67, 2046, "RocketBox", AmmoKind.Rockets, 5)]
    [InlineData(68, 2047, "Cell", AmmoKind.Cells, 20)]
    [InlineData(69, 17, "CellPack", AmmoKind.Cells, 100)]
    [InlineData(70, 2008, "Shell", AmmoKind.Shells, 4)]
    [InlineData(71, 2049, "ShellBox", AmmoKind.Shells, 20)]
    public void NativeIdentityCollectionAndSaveReconstruction(int index, ushort editor, string name, AmmoKind kind, int amount)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\nWidth = 786432\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index); Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(editor, patch); var item = Item(sim); var player = sim.Players.Single(); player.Inventory.Bullets = 0;
        Assert.True(item.SpecialPickup); Assert.False(item.Solid); Assert.False(item.Shootable); Assert.Equal(12, item.Radius.ToDouble());
        var saved = SimSavegame.Write(sim, true); item.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(item, Item(sim)); var fresh = Room(editor, patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true)); Collect(sim); Collect(fresh);
        Assert.Equal(amount, player.Inventory.Ammo(kind)); Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
        Assert.DoesNotContain(sim.Actors, a => a.MapPickupThingIndex.HasValue);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(2, 10)]
    [InlineData(4, 20)]
    public void PatchedAmmoRetainsSkillScaling(int skill, int expected)
    {
        var patch = DehackedPatch.Apply("Thing 64\nMass = 200\n"); var sim = Room(2007, patch, skill);
        sim.Players.Single().Inventory.Bullets = 0; Collect(sim); Assert.Equal(expected, sim.Players.Single().Inventory.Bullets);
    }

    [Fact]
    public void BackpackRetainsHeightCapExpansionAndSavedCollectionReplay()
    {
        var patch = DehackedPatch.Apply("Thing 72\nMass = 200\n"); Assert.Empty(patch.Errors); var sim = Room(8, patch);
        Assert.Equal(26, Item(sim).Height.ToDouble()); Assert.False(Item(sim).Solid);
        var saved = SimSavegame.Write(sim, true); Collect(sim); var inventory = sim.Players.Single().Inventory;
        Assert.True(inventory.HasBackpack); Assert.Equal(400, inventory.MaxBullets);
        var result = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved); Assert.False(inventory.HasBackpack); Collect(sim);
        Assert.Equal(result, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void LayeringAndPrimaryReplacementCanDisablePickupWithoutMutatingBaseline()
    {
        var original = DehackedPatch.Apply("Thing 64\nHeight = 2097152\n");
        var inherited = DehackedPatch.Apply("Thing 64\nMass = 200\n", original);
        Assert.Equal(32, Item(Room(2007, inherited)).Height.ToDouble());
        var changed = DehackedPatch.Apply("Thing 64\nBits = SOLID\n", inherited); Assert.Empty(changed.Errors);
        var actor = Room(2007, changed).Actors.Single(a => a is not PlayerPawn);
        Assert.False(actor.SpecialPickup); Assert.True(actor.Solid); Assert.True(Item(Room(2007, original)).SpecialPickup);
    }

    private static void Collect(AuthoritySimulation sim) { sim.Players.Single().X = Fixed.FromInt(200); sim.Tick(); }
    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(ushort type, DehackedPatchResult patch, int skill = 2) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch, spawnOptions: new SpawnOptions(Skill: skill));
}
