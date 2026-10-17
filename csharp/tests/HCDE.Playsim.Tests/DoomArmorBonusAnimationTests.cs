using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomArmorBonusAnimationTests
{
    [Theory]
    [InlineData(2018, 44, 13)]
    [InlineData(2019, 45, 12)]
    public void ArmorUsesNativeNormalBrightLoop(ushort editor, int index, int period)
    {
        var sim = Room(editor, index); var actor = Item(sim);
        Assert.Equal(0, actor.States.Current); Assert.False(actor.FullBright); Assert.Equal(6, actor.States.RemainingTics);
        for (var i = 1; i <= period * 3; i++)
        {
            sim.Tick(); var bright = i % period >= 6;
            Assert.Equal(bright ? 1 : 0, actor.States.Current); Assert.Equal(bright, actor.FullBright);
        }
    }

    [Theory]
    [InlineData(2014, 46)]
    [InlineData(2015, 47)]
    public void BonusesUseSixNonBrightStatesAndLoop(ushort editor, int index)
    {
        var sim = Room(editor, index); var actor = Item(sim);
        for (var i = 0; i < 72; i++)
        {
            Assert.Equal(i / 6 % 6, actor.States.Current); Assert.False(actor.FullBright); sim.Tick();
        }
    }

    [Theory]
    [InlineData(2018, 44)]
    [InlineData(2019, 45)]
    [InlineData(2014, 46)]
    [InlineData(2015, 47)]
    public void MidLoopSaveReconstructsDestroyedMapPickupAndReplays(ushort editor, int index)
    {
        var sim = Room(editor, index); for (var i = 0; i < 19; i++) sim.Tick();
        var actor = Item(sim); var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(actor, Item(sim)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(editor, index); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 40; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData(2018)]
    [InlineData(2019)]
    [InlineData(2014)]
    [InlineData(2015)]
    public void DroppedPickupUsesSameStateTableAndFreshReconstruction(ushort editor)
    {
        var sim = Room(2007, null); var source = sim.Players.Single(); source.X = Fixed.FromInt(-200);
        Assert.True(sim.SpawnDroppedPickup(source, editor)); var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        drop.SetXYZ(400, 0, 0); drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        for (var i = 0; i < 19; i++) sim.Tick();
        Assert.Equal(editor is 2014 or 2015 ? 3 : 1, drop.States.Current);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(2007, null); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 20; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Fact]
    public void CrushAndRemovalRemainNonBright()
    {
        var sim = Room(2018, 44); var actor = Item(sim); for (var i = 0; i < 6; i++) sim.Tick(); Assert.True(actor.FullBright);
        actor.States.Enter(actor, actor.GenericCrushState); Assert.False(actor.FullBright); Assert.Equal(-1, actor.States.RemainingTics);
        actor.States.Enter(actor, actor.NullState); sim.Tick(); Assert.True(actor.Destroyed);
    }

    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(ushort type, int? index)
    {
        var patch = index.HasValue ? DehackedPatch.Apply($"Thing {index}\nMass = 200\n") : null;
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
