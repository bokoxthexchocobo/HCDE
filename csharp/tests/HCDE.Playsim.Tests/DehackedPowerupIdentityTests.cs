using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedPowerupIdentityTests
{
    [Theory]
    [InlineData(56, 2013, "Soulsphere", 16)]
    [InlineData(58, 2023, "Berserk", 16)]
    [InlineData(60, 2025, "RadSuit", 46)]
    [InlineData(63, 83, "Megasphere", 16)]
    public void NativeIdentityDefaultsAndDestroyedReconstruction(int index, ushort editor, string name, int height)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index); Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(editor, patch); var item = Item(sim);
        Assert.Equal(height, item.Height.ToDouble()); Assert.Equal(20, item.Radius.ToDouble());
        Assert.True(item.SpecialPickup); Assert.False(item.Solid); Assert.False(item.Shootable);
        if (editor == PickupCatalog.RadSuit) Assert.True(item.States.CurrentFullBright);
        var saved = SimSavegame.Write(sim, true); item.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(item, Item(sim)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(editor, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(56, 2013)]
    [InlineData(58, 2023)]
    [InlineData(60, 2025)]
    [InlineData(63, 83)]
    public void CollectionEffectsMatchUnpatchedPickupAndReplayAfterSave(int index, ushort editor)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n");
        var sim = Room(editor, patch); var stock = Room(editor, null);
        sim.Players.Single().Health = stock.Players.Single().Health = 50;
        var saved = SimSavegame.Write(sim, true);
        Collect(sim); Collect(stock); var player = sim.Players.Single(); var reference = stock.Players.Single();
        Assert.Equal(reference.Health, player.Health); Assert.Equal(reference.Inventory.Armor, player.Inventory.Armor);
        Assert.Equal(reference.HasPowerStrength, player.HasPowerStrength);
        Assert.Equal(reference.PowerIronFeetTics, player.PowerIronFeetTics);
        Assert.DoesNotContain(sim.Actors, a => a.MapPickupThingIndex.HasValue);
        var result = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved); Collect(sim);
        Assert.Equal(result, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void RadSuitKeepsBrightDefaultWhenHeightIsExplicitlyPatched()
    {
        var patch = DehackedPatch.Apply("Thing 60\nHeight = 2097152\n");
        var sim = Room(2025, patch); Assert.Equal(32, Item(sim).Height.ToDouble()); Assert.True(Item(sim).States.CurrentFullBright);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(2025, patch); SimSavegame.Apply(fresh, saved);
        Assert.True(Item(fresh).States.CurrentFullBright); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void PrimaryReplacementAndLayeringRemainIndependentOfPowerupGiftIdentity()
    {
        var original = DehackedPatch.Apply("Thing 56\nWidth = 786432\n");
        var changed = DehackedPatch.Apply("Thing 56\nBits = SOLID\n", original); Assert.Empty(changed.Errors);
        var actor = Room(2013, changed).Actors.Single(a => a is not PlayerPawn);
        Assert.True(actor.Solid); Assert.False(actor.SpecialPickup); Assert.Equal(12, actor.Radius.ToDouble());
        Assert.True(Item(Room(2013, original)).SpecialPickup);
    }

    private static void Collect(AuthoritySimulation sim) { sim.Players.Single().X = Fixed.FromInt(200); sim.Tick(); }
    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(ushort type, DehackedPatchResult? patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
