using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedKeyHealthTests
{
    [Theory]
    [InlineData(48, 5, "BlueCard", PickupCatalog.KeyColor.Blue)]
    [InlineData(49, 13, "RedCard", PickupCatalog.KeyColor.Red)]
    [InlineData(50, 6, "YellowCard", PickupCatalog.KeyColor.Yellow)]
    [InlineData(51, 39, "YellowSkull", PickupCatalog.KeyColor.Yellow)]
    [InlineData(52, 38, "RedSkull", PickupCatalog.KeyColor.Red)]
    [InlineData(53, 40, "BlueSkull", PickupCatalog.KeyColor.Blue)]
    public void NativeKeyMappingCollectionAndFreshSaveReplay(int index, ushort editor, string name, PickupCatalog.KeyColor color)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index); Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(editor, patch); var item = Item(sim);
        Assert.True(item.SpecialPickup); Assert.False(item.Solid); Assert.False(item.Shootable);
        Assert.Equal(16, item.Height.ToDouble()); Assert.Equal(20, item.Radius.ToDouble());
        var saved = SimSavegame.Write(sim, true); item.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(item, Item(sim)); var fresh = Room(editor, patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        sim.Players.Single().X = Fixed.FromInt(200); fresh.Players.Single().X = Fixed.FromInt(200); sim.Tick(); fresh.Tick();
        Assert.True(sim.Players.Single().Inventory.HasKey(color)); Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(48, 5)]
    [InlineData(49, 13)]
    [InlineData(50, 6)]
    [InlineData(51, 39)]
    [InlineData(52, 38)]
    [InlineData(53, 40)]
    public void PatchedKeyDefaultIsExcludedFromDeathmatchUnlessPrimaryBitsClearIt(int index, ushort editor)
    {
        var original = DehackedPatch.Apply($"Thing {index}\nMass = 200\n");
        Assert.Single(Room(editor, original, SpawnGameMode.Deathmatch).Actors);
        var replacement = DehackedPatch.Apply($"Thing {index}\nBits = SPECIAL\n", original); Assert.Empty(replacement.Errors);
        Assert.Equal(2, Room(editor, replacement, SpawnGameMode.Deathmatch).Actors.Count);
        Assert.Equal(2, Room(editor, original).Actors.Count);
    }

    [Theory]
    [InlineData(54, 2011, "Stimpack", 60)]
    [InlineData(55, 2012, "Medikit", 75)]
    public void MedicalPickupIdentityPreservesGiftAndReconstruction(int index, ushort editor, string name, int health)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nHeight = 2097152\n"); Assert.Empty(patch.Errors);
        Assert.Equal(name, patch.Actors.Single(a => a.Index == index).Name);
        var sim = Room(editor, patch); sim.Players.Single().Health = 50; var item = Item(sim);
        Assert.Equal(32, item.Height.ToDouble()); Assert.False(item.Solid);
        var saved = SimSavegame.Write(sim, true); item.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        sim.Players.Single().X = Fixed.FromInt(200); sim.Tick(); Assert.Equal(health, sim.Players.Single().Health);
        Assert.DoesNotContain(sim.Actors, a => a.MapPickupThingIndex.HasValue);
    }

    [Fact]
    public void LayeringPreservesKeyClassDefaultsWithoutMutatingBaseline()
    {
        var original = DehackedPatch.Apply("Thing 48\nWidth = 786432\n");
        var inherited = DehackedPatch.Apply("Thing 48\nMass = 200\n", original);
        var replacement = DehackedPatch.Apply("Thing 48\nBits = SOLID\n", inherited);
        Assert.True(Item(Room(5, inherited)).SpecialPickup); Assert.Equal(12, Item(Room(5, inherited)).Radius.ToDouble());
        Assert.False(Room(5, replacement).Actors.Single(a => a is not PlayerPawn).SpecialPickup);
        Assert.Equal(0x02000001u, original.Actors.Single(a => a.Index == 48).Bits);
    }

    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(ushort type, DehackedPatchResult patch, SpawnGameMode mode = SpawnGameMode.Single) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch, spawnOptions: new SpawnOptions(Mode: mode));
}
