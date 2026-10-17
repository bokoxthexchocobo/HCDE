using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedLampColumnTests
{
    [Theory]
    [InlineData(80, 85, "TechLamp", 80)]
    [InlineData(81, 86, "TechLamp2", 60)]
    [InlineData(82, 2028, "Column", 48)]
    [InlineData(83, 30, "TallGreenColumn", 52)]
    [InlineData(84, 31, "ShortGreenColumn", 40)]
    [InlineData(85, 32, "TallRedColumn", 52)]
    [InlineData(86, 33, "ShortRedColumn", 40)]
    [InlineData(87, 37, "SkullColumn", 40)]
    [InlineData(88, 36, "HeartColumn", 40)]
    [InlineData(89, 41, "EvilEye", 54)]
    [InlineData(90, 42, "FloatingSkull", 26)]
    [InlineData(91, 43, "TorchTree", 56)]
    [InlineData(92, 44, "BlueTorch", 68)]
    [InlineData(93, 45, "GreenTorch", 68)]
    [InlineData(94, 46, "RedTorch", 68)]
    [InlineData(95, 55, "ShortBlueTorch", 37)]
    [InlineData(96, 56, "ShortGreenTorch", 37)]
    [InlineData(97, 57, "ShortRedTorch", 37)]
    [InlineData(98, 47, "Stalagtite", 40)]
    [InlineData(99, 48, "TechPillar", 128)]
    [InlineData(100, 34, "Candlestick", 14)]
    [InlineData(101, 35, "Candelabra", 60)]
    public void NativeIdentityDefaultsAndRemappedReconstruction(int index, int editor, string name, int height)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 9097\nHit points = 77\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index); Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(9097, patch); var actor = Assert.Single(sim.Actors);
        Assert.Equal(height, actor.Height.ToDouble()); Assert.Equal(editor == 34 ? 20 : 16, actor.Radius.ToDouble());
        Assert.Equal(editor != 34, actor.Solid); Assert.False(actor.Shootable); Assert.False(actor.IsMonster);
        Assert.Equal(-16, actor.ProjectilePassHeight.ToDouble()); Assert.Equal(77, actor.Health); Assert.Null(actor.Brain);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(actor, Assert.Single(sim.Actors)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(9097, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 20; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData(80, 85)]
    [InlineData(88, 36)]
    [InlineData(89, 41)]
    [InlineData(90, 42)]
    [InlineData(95, 55)]
    public void PatchedAnimationRetainsStockTiming(int index, ushort editor)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n"); var sim = Room(editor, patch); var stock = Room(editor, null);
        for (var i = 0; i < 48; i++)
        {
            sim.Tick(); stock.Tick();
            Assert.Equal(stock.Actors.Single().States.Current, sim.Actors.Single().States.Current);
            Assert.Equal(stock.Actors.Single().States.RemainingTics, sim.Actors.Single().States.RemainingTics);
        }
    }

    [Fact]
    public void CandlestickPrimaryReplacementCanAddSolidityWithoutChangingBaseline()
    {
        var original = DehackedPatch.Apply("Thing 100\nMass = 200\n");
        var changed = DehackedPatch.Apply("Thing 100\nBits = SOLID\nWidth = 786432\n", original); Assert.Empty(changed.Errors);
        var actor = Assert.Single(Room(34, changed).Actors); Assert.True(actor.Solid); Assert.Equal(12, actor.Radius.ToDouble());
        Assert.False(Assert.Single(Room(34, original).Actors).Solid);
    }

    private static AuthoritySimulation Room(ushort type, DehackedPatchResult? patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = type }], Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
