using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VisibilityFloorClipRestoreTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbsentOverridesUndoLaterActionsAndPreserveBytes(bool mixed)
    {
        var sim = Room(); var actor = sim.Actors[0];
        if (mixed) { sim.Actors[1].Invisible = true; sim.Actors[1].FloorClip = 8; }
        var bytes = SimSavegame.Write(sim);
        ActorPropertyActions.HideThing(actor);
        ActorPropertyActions.SinkMobj(actor, 8);
        SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Invisible); Assert.Equal(0, actor.FloorClip);
        Assert.False(actor.HasVisibilityOverride); Assert.False(actor.HasFloorClipOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ExplicitDefaultsRemainExplicitAfterRestore()
    {
        var sim = Room(); var actor = sim.Actors[0];
        actor.Invisible = false; actor.FloorClip = 0;
        var bytes = SimSavegame.Write(sim);
        actor.Invisible = true; actor.FloorClip = 8;
        SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Invisible); Assert.Equal(0, actor.FloorClip);
        Assert.True(actor.HasVisibilityOverride); Assert.True(actor.HasFloorClipOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 }],
    });
}
