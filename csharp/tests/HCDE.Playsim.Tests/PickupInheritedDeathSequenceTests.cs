using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupInheritedDeathSequenceTests
{
    [Theory]
    [InlineData(2018)]
    [InlineData(2014)]
    [InlineData(2013)]
    [InlineData(2007)]
    [InlineData(2001)]
    public void InheritedDieFromSpawnResolvesAndReplaysAfterFreshSave(ushort editor)
    {
        var sim = Room(editor); var item = Item(sim);
        Assert.True(AcsActorStates.TryFindNamedState(item, "diefromspawn", true, out var state));
        Assert.Equal(item.DieFromSpawnState, state);
        Assert.False(AcsActorStates.HasNamedState(item, "DieFromSpawn.More", true));
        Assert.True(AcsActorStates.HasNamedState(item, "DieFromSpawn.More", false));
        item.States.Enter(item, state); Assert.False(item.FullBright); Assert.Equal(1, item.States.RemainingTics);
        Assert.False(item.IsDead); var saved = SimSavegame.Write(sim, true);
        var fresh = Room(editor); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        sim.Tick(); fresh.Tick(); Assert.True(item.Destroyed); Assert.DoesNotContain(fresh.Actors, a => a.MapPickupThingIndex.HasValue);
        Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Fact]
    public void InheritedCrushHoldsUntilAdvancedThenStops()
    {
        var sim = Room(2007); var item = Item(sim); item.States.Enter(item, item.GenericCrushState);
        sim.Tick(); Assert.False(item.Destroyed); Assert.Equal(-1, item.States.RemainingTics);
        item.States.ForceRemainingTics(1); sim.Tick(); Assert.True(item.Destroyed);
    }

    [Theory]
    [InlineData(2014)]
    [InlineData(2013)]
    public void DroppedPickupReconstructsInheritedSequenceBeyondGenericTable(ushort editor)
    {
        var sim = Room(editor); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), editor));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        drop.States.Enter(drop, drop.DieFromSpawnState);
        var saved = SimSavegame.Write(sim, true); drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(editor); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        sim.Tick(); fresh.Tick(); Assert.DoesNotContain(fresh.Actors, a => a.SpawnedDroppedPickup);
        Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    private static Actor Item(AuthoritySimulation sim) => sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
    private static AuthoritySimulation Room(ushort editor) => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = editor, X = 400 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
