using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DecorationStateLabelTests
{
    [Theory]
    [InlineData(85)]
    [InlineData(36)]
    [InlineData(42)]
    [InlineData(49)]
    [InlineData(30)]
    [InlineData(10)]
    [InlineData(24)]
    public void AnimationFrameIndicesDoNotCreateGenericLabels(int type)
    {
        var actor = Room(type).Actors.Single();
        Assert.True(AcsActorStates.HasNamedState(actor, "sPaWn", true));
        foreach (var name in new[] { "Pain", "Death", "Corpse", "See", "Active", "Inactive" })
        {
            Assert.False(AcsActorStates.HasNamedState(actor, name, true));
            Assert.False(AcsActorStates.HasNamedState(actor, name, false));
        }
    }

    [Fact]
    public void LabelVisibilitySurvivesSaveRestoreMidAnimation()
    {
        var sim = Room(85); for (var i = 0; i < 9; i++) sim.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(85); restored.RestoreState(state);
        var actor = restored.Actors.Single(); Assert.Equal(2, actor.States.Current);
        Assert.False(AcsActorStates.HasNamedState(actor, "Death", true));
        Assert.True(AcsActorStates.HasNamedState(actor, "Spawn", true));
    }

    [Fact]
    public void LivingMonsterKeepsSupportedLabels()
    {
        var actor = Room(3001).Actors.Single();
        Assert.True(AcsActorStates.HasNamedState(actor, "Pain", true));
        Assert.True(AcsActorStates.HasNamedState(actor, "Death", true));
        Assert.True(AcsActorStates.HasNamedState(actor, "Corpse", true));
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 160 }], Things = [new LevelThing { Type = type }],
    });
}
