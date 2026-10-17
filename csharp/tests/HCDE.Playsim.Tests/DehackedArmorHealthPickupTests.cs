using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedArmorHealthPickupTests
{
    [Theory]
    [InlineData(44, 2018, "GreenArmor")]
    [InlineData(45, 2019, "BlueArmor")]
    [InlineData(46, 2014, "HealthBonus")]
    [InlineData(47, 2015, "ArmorBonus")]
    public void NativeIdentityAndPatchedDimensionsReconstructDestroyedPickup(int index, ushort editor, string name)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nHeight = 2097152\nWidth = 786432\nMass = 200\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index); Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(editor, patch); var item = Item(sim);
        Assert.True(item.SpecialPickup); Assert.False(item.Solid); Assert.False(item.Shootable); Assert.False(item.IsMonster);
        Assert.Equal(32, item.Height.ToDouble()); Assert.Equal(12, item.Radius.ToDouble()); Assert.Equal(200, item.Mass);
        var saved = SimSavegame.Write(sim, true); item.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(item, Item(sim)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(editor, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(44, 2018, 100, 100)]
    [InlineData(45, 2019, 100, 200)]
    [InlineData(46, 2014, 101, 0)]
    [InlineData(47, 2015, 100, 1)]
    public void PatchedPickupStillCollectsNativeGiftAndReplaysAfterSave(int index, ushort editor, int health, int armor)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n"); var sim = Room(editor, patch);
        var saved = SimSavegame.Write(sim, true); var player = sim.Players.Single(); player.X = Fixed.FromInt(200); sim.Tick();
        Assert.Equal(health, player.Health); Assert.Equal(armor, player.Inventory.Armor); Assert.DoesNotContain(sim.Actors, a => a.MapPickupThingIndex.HasValue);
        var result = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved); player.X = Fixed.FromInt(200); sim.Tick();
        Assert.Equal(result, SimSavegame.Write(sim, true)); Assert.Equal(health, player.Health); Assert.Equal(armor, player.Inventory.Armor);
    }

    [Theory]
    [InlineData(44, 2018)]
    [InlineData(46, 2014)]
    public void PrimaryReplacementCanRemovePickupAndAddCollisionWithoutChangingBaseline(int index, ushort editor)
    {
        var original = DehackedPatch.Apply($"Thing {index}\nMass = 200\n");
        var replacement = DehackedPatch.Apply($"Thing {index}\nBits = SOLID+SHOOTABLE\n", original); Assert.Empty(replacement.Errors);
        var sim = Room(editor, replacement); var actor = sim.Actors.Single(a => a is not PlayerPawn);
        Assert.True(actor.Solid); Assert.True(actor.Shootable); Assert.False(actor.SpecialPickup);
        sim.Players.Single().X = Fixed.FromInt(200); sim.Tick(); Assert.False(actor.Destroyed);
        Assert.True(Item(Room(editor, original)).SpecialPickup);
    }

    [Fact]
    public void UntouchedCatalogDefinitionsDoNotAlterExistingPickupSpawns()
    {
        var plain = Room(2018, null); var empty = Room(2018, DehackedPatch.Apply(""));
        Assert.Equal(SimSavegame.Write(plain, true), SimSavegame.Write(empty, true));
    }

    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(ushort type, DehackedPatchResult? patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
