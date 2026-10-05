using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DecorationGenericCrushTests
{
    [Theory]
    [InlineData(85)]
    [InlineData(30)]
    [InlineData(24)]
    [InlineData(23)]
    public void InheritedGenericCrushHoldsAndRestores(int type)
    {
        var sim = Room(type); var actor = sim.Actors.Single();
        Assert.True(AcsActorStates.HasNamedState(actor, "genericcrush", true));
        actor.States.Enter(actor, actor.GenericCrushState);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.False(actor.Destroyed); Assert.Equal(-1, actor.States.RemainingTics);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(type); restored.RestoreState(state); var loaded = restored.Actors.Single();
        Assert.Equal(loaded.GenericCrushState, loaded.States.Current); Assert.Equal(-1, loaded.States.RemainingTics);
        restored.Tick(); Assert.False(loaded.Destroyed);
    }

    [Fact]
    public void MapGibsSpawnAliasesCrushLabel()
    {
        var actor = Room(24).Actors.Single();
        Assert.Equal(actor.GenericCrushState, actor.SpawnState); Assert.Equal(actor.SpawnState, actor.States.Current);
        Assert.True(AcsActorStates.HasNamedState(actor, "Spawn", true));
        Assert.True(AcsActorStates.HasNamedState(actor, "GenericCrush", true));
        Assert.False(AcsActorStates.HasNamedState(actor, "Corpse", true));
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 160 }], Things = [new LevelThing { Type = type }],
    });
}
