using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorActivationClearSaveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConsumedActivationFlagsRestoreAsZero(bool archive)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.ActivationType = 256;
        Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(0, actor.ActivationType);
        var state = sim.CaptureState(); var bytes = archive ? SimSavegame.Write(state) : null;
        actor.ActivationType = 512;
        if (archive) SimSavegame.Apply(sim, bytes!); else sim.RestoreState(state);
        Assert.Equal(0, actor.ActivationType);
        Assert.False(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.False(actor.Dormant);
    }

    [Fact]
    public void ExplicitZeroFlagAssignmentSurvivesArchive()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.ActivationType = 0;
        var bytes = SimSavegame.Write(sim.CaptureState());
        actor.ActivationType = 64;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(0, actor.ActivationType);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }],
    });
}
