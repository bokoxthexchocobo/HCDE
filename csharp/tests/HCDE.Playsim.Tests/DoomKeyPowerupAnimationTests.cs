using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomKeyPowerupAnimationTests
{
    [Theory]
    [InlineData(5, 48)]
    [InlineData(13, 49)]
    [InlineData(6, 50)]
    [InlineData(39, 51)]
    [InlineData(38, 52)]
    [InlineData(40, 53)]
    public void KeyTimingAndBrightnessRepeatAcrossSaveAndDestroyedReconstruction(ushort editor, int index)
    {
        var sim = Room(editor, index); var key = Item(sim);
        var initialTics = key.States.RemainingTics; Assert.InRange(initialTics, 1, 10);
        for (var i = 0; i < 27; i++)
        {
            var bright = i >= initialTics && (i - initialTics) % 20 < 10;
            Assert.Equal(bright, key.FullBright); Assert.Equal(bright ? 1 : 0, key.States.Current); sim.Tick();
        }
        var saved = SimSavegame.Write(sim, true); key.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        var fresh = Room(editor, index); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 40; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData(2013, 56, 6)]
    [InlineData(83, 63, 4)]
    public void SpheresLoopNativeNumberOfBrightSixTicStates(ushort editor, int index, int count)
    {
        var sim = Room(editor, index); var item = Item(sim);
        for (var i = 0; i < count * 12; i++)
        {
            Assert.Equal(i / 6 % count, item.States.Current); Assert.True(item.FullBright); sim.Tick();
        }
    }

    [Theory]
    [InlineData(2023, 58)]
    [InlineData(2025, 60)]
    public void StaticPowerupsHoldBrightUntilCrushOrRemoval(ushort editor, int index)
    {
        var sim = Room(editor, index); var item = Item(sim);
        for (var i = 0; i < 40; i++) { Assert.True(item.FullBright); Assert.Equal(-1, item.States.RemainingTics); sim.Tick(); }
        item.States.Enter(item, item.GenericCrushState); Assert.False(item.FullBright);
        item.States.Enter(item, item.NullState); sim.Tick(); Assert.True(item.Destroyed);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(2013)]
    [InlineData(83)]
    [InlineData(2023)]
    [InlineData(2025)]
    public void DroppedStateTablesSurviveFreshSaveAndReplay(ushort editor)
    {
        var sim = Room(2007, null); var source = sim.Players.Single(); source.X = Fixed.FromInt(-200);
        Assert.True(sim.SpawnDroppedPickup(source, editor)); var item = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        item.SetXYZ(400, 0, 0); item.VelocityX = item.VelocityY = item.VelocityZ = default;
        for (var i = 0; i < 19; i++) sim.Tick();
        Assert.True(item.FullBright); var saved = SimSavegame.Write(sim, true); var fresh = Room(2007, null);
        SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 40; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Fact]
    public void PropertyOnlyPatchKeepsSameKeyTimingAsStock()
    {
        var patched = Room(5, 48); var stock = Room(5, null);
        for (var i = 0; i < 60; i++)
        {
            Assert.Equal(Item(stock).States.RemainingTics, Item(patched).States.RemainingTics);
            Assert.Equal(Item(stock).FullBright, Item(patched).FullBright); patched.Tick(); stock.Tick();
        }
    }

    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(ushort type, int? index) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] },
        dehacked: index.HasValue ? DehackedPatch.Apply($"Thing {index}\nMass = 200\n") : null);
}
