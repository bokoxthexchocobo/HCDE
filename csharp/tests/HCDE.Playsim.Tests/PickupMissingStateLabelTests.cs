using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupMissingStateLabelTests
{
    [Theory]
    [InlineData(44, 2018)]
    [InlineData(46, 2014)]
    [InlineData(56, 2013)]
    [InlineData(64, 2007)]
    [InlineData(78, 2001)]
    public void MissingPainLabelDoesNotEnterAnimationOrCrushAndFatalDamageRemoves(int index, ushort editor)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nBits = SPECIAL+SHOOTABLE\nHit points = 100\nPain chance = 256\n");
        Assert.Empty(patch.Errors); var sim = Room(editor, patch);
        var actor = sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        Assert.Equal(-1, actor.PainState); Assert.Equal(-1, actor.DeathState);
        var current = actor.States.Current; var tics = actor.States.RemainingTics;
        Assert.False(actor.TriggerPainChance()); Assert.Equal(current, actor.States.Current);
        ActorDamage.Apply(actor, 10); Assert.Equal(90, actor.Health);
        Assert.Equal(current, actor.States.Current); Assert.Equal(tics, actor.States.RemainingTics);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(editor, patch); SimSavegame.Apply(fresh, saved);
        var restored = fresh.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.False(restored.TriggerPainChance()); Assert.False(actor.TriggerPainChance());
        ActorDamage.Apply(actor, 100); ActorDamage.Apply(restored, 100);
        Assert.True(actor.Destroyed); Assert.True(restored.Destroyed);
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    private static AuthoritySimulation Room(ushort editor, DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = editor, X = 400 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
