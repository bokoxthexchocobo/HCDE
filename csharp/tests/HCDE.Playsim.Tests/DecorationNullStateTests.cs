using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DecorationNullStateTests
{
    [Theory]
    [InlineData(85)]
    [InlineData(30)]
    [InlineData(24)]
    [InlineData(23)]
    public void NullLabelSelectsOneTicRemovalState(int type)
    {
        var sim = Room(type); var actor = sim.Actors.Single();
        Assert.True(AcsActorStates.HasNamedState(actor, "nUlL", true));
        Assert.True(AcsActorStates.HasNamedState(actor, "Null", false));
        Assert.True(AcsActorStates.HasNamedState(actor, "Null.Extra", false));
        Assert.False(AcsActorStates.HasNamedState(actor, "Null.Extra", true));
        actor.States.Enter(actor, actor.NullState);
        Assert.False(actor.Destroyed); Assert.Equal(1, actor.States.RemainingTics);
        sim.Tick(); Assert.True(actor.Destroyed); Assert.Empty(sim.Actors);
        Assert.False(AcsActorStates.HasNamedState(actor, "Null", true));
    }

    [Fact]
    public void SavedNullStateRemovesOnNextRestoredTick()
    {
        var sim = Room(85); var actor = sim.Actors.Single(); actor.States.Enter(actor, actor.NullState);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(85); restored.RestoreState(state); var loaded = restored.Actors.Single();
        Assert.Equal(loaded.NullState, loaded.States.Current); Assert.Equal(1, loaded.States.RemainingTics);
        restored.Tick(); Assert.True(loaded.Destroyed); Assert.Empty(restored.Actors);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 160 }], Things = [new LevelThing { Type = type }],
    });
}
