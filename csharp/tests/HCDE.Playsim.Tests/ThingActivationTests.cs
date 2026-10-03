using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingActivationTests
{
    [Fact]
    public void DeactivationHoldsStateAndActivationReleasesItWithOneTic()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.States.Configure(actor, [new ActorFrame(2, 1), new ActorFrame(-1, 1)], 0);
        actor.ReactionTime = 10;
        Assert.True(LineSpecials.Execute(sim, actor, 131, 0));
        sim.Tick();
        Assert.True(actor.Dormant); Assert.Equal(-1, actor.States.RemainingTics);
        Assert.Equal(0, actor.States.Current); Assert.Equal(10, actor.ReactionTime);
        Assert.True(LineSpecials.Execute(sim, actor, 130, 0));
        Assert.False(actor.Dormant); Assert.Equal(1, actor.States.RemainingTics);
        sim.Tick(); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void TaggedActivationFindsActorsAndMissingTidFails()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        Assert.True(ThingActivation.Execute(sim, null, 7, false)); Assert.True(actor.Dormant);
        Assert.False(ThingActivation.Execute(sim, null, 8, true));
        Assert.False(ThingActivation.Execute(sim, null, 0, true));
    }

    [Theory]
    [InlineData(false, 30)]
    [InlineData(true, 0)]
    public void NonMonsterAndDeadBodiesAreFoundWithoutChangingDormancy(bool monster, int health)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.IsMonster = monster; actor.Health = health;
        Assert.True(ThingActivation.Execute(sim, actor, 0, false)); Assert.False(actor.Dormant);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, Id = 7 }],
    });
}
