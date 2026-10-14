using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SoundTargetCompatibilityTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void CompatibilityAndNoSectorSelectSectorInsteadOfStoredActorNoise(bool compatibility, bool noSector)
    {
        var (sim, actor, oldNoise, newNoise) = Room(compatibility); actor.NoSector = noSector;
        actor.LastHeardTargetId = oldNoise.Id; actor.Brain!.Enabled = false;
        SoundPropagation.Alert(sim, newNoise, newNoise, 1);
        Assert.Equal(oldNoise.Id, actor.LastHeardTargetId); Assert.Equal(newNoise.Id, sim.SectorSoundTargetId(0));
        actor.Brain.Enabled = true; actor.Brain.Tick(sim, actor);
        Assert.Equal(compatibility || noSector ? newNoise.Id : oldNoise.Id, actor.TargetId);
        Assert.Equal(oldNoise.Id, actor.LastHeardTargetId);
    }

    [Fact]
    public void EmptySectorTargetDoesNotFallBackToLastHeard()
    {
        var (sim, actor, oldNoise, _) = Room(true); actor.LastHeardTargetId = oldNoise.Id;
        actor.Brain!.Tick(sim, actor); Assert.Null(actor.TargetId); Assert.Equal(oldNoise.Id, actor.LastHeardTargetId);
    }

    [Fact]
    public void SavedSectorTargetReplaysWithConfiguredCompatibility()
    {
        var (sim, actor, oldNoise, newNoise) = Room(true); actor.LastHeardTargetId = oldNoise.Id;
        actor.Brain!.Enabled = false; SoundPropagation.Alert(sim, newNoise, newNoise, 1); actor.Brain.Enabled = true;
        var saved = SimSavegame.Write(sim, true); actor.Brain.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor);
        Assert.Equal(newNoise.Id, actor.TargetId); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor OldNoise, Actor NewNoise) Room(bool compatibility)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 }, new LevelThing { Type = 3002, X = 300 }] },
            compat: compatibility ? CompatSurface.SoundTarget : CompatSurface.None);
        return (sim, sim.Actors[0], sim.Actors[1], sim.Actors[2]);
    }
}
