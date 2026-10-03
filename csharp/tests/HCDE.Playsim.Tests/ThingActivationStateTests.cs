using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingActivationStateTests
{
    [Fact]
    public void CustomLabelsRunActionsOnceAndAreVisibleToStateQueries()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); var actions = 0;
        actor.ActiveState = 1; actor.InactiveState = 2;
        actor.States.Configure(actor, [new(-1, 0), new(3, 0, _ => actions++), new(5, 0, _ => actions++)], 0);
        Assert.True(AcsActorStates.HasNamedState(actor, "active", true));
        Assert.True(AcsActorStates.HasNamedState(actor, "Inactive", true));
        ThingActivation.Execute(sim, actor, 0, false);
        Assert.True(actor.Dormant); Assert.Equal(2, actor.States.Current); Assert.Equal(5, actor.States.RemainingTics);
        ThingActivation.Execute(sim, actor, 0, false); Assert.Equal(1, actions);
        ThingActivation.Execute(sim, actor, 0, true);
        Assert.False(actor.Dormant); Assert.Equal(1, actor.States.Current); Assert.Equal(3, actor.States.RemainingTics);
        Assert.Equal(2, actions);
    }

    [Fact]
    public void StateActionRemovingLaterTaggedTargetDoesNotActivateDestroyedActor()
    {
        var sim = Room(); var first = Assert.Single(sim.Actors); var second = sim.AddBot(64, 0);
        second.ThingId = 7; first.Dormant = second.Dormant = true; first.ActiveState = 1;
        first.States.Configure(first, [new(-1, 0), new(-1, 1, _ => second.Destroy())], 0);
        Assert.True(ThingActivation.Execute(sim, null, 7, true));
        Assert.True(second.Destroyed); Assert.True(second.Dormant);
    }

    [Fact]
    public void ActivationLabelsParticipateInSimulationHash()
    {
        var first = Room(); var second = Room(); Assert.Single(first.Actors).ActiveState = 1;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004, Id = 7 }],
    });
}
