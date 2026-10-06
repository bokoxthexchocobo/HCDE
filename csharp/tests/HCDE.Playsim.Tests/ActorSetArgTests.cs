using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSetArgTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EachArgumentRoundTripsIndependently(int index)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetArg(actor, index, -100);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.SetArg(actor, index, 200); sim.RestoreState(state);
        Assert.Equal(-100, actor.SpecialArgs[index]);
        Assert.Equal(1, actor.SpecialArgs.Count(arg => arg != 0));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(int.MaxValue)]
    public void InvalidIndexHasNativeNoOpBehavior(int index)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        ActorPropertyActions.SetArg(actor, index, 10);
        Assert.Equal(bytes, SimSavegame.Write(sim)); Assert.All(actor.SpecialArgs, arg => Assert.Equal(0, arg));
    }

    [Fact]
    public void ClearedArgumentIsRetainedEvenWhenAllSpecialFieldsAreZero()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetArg(actor, 0, 10); ActorPropertyActions.SetArg(actor, 0, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.NotNull(state.Actors[0].ActorSpecial);
        actor.SpecialArgs[0] = 20; sim.RestoreState(state); Assert.Equal(0, actor.SpecialArgs[0]);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void FrameArgumentChangesFeedExecutedDeathSpecial()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Special = 112;
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, Action: self =>
        { ActorPropertyActions.SetArg(self, 0, 7); ActorPropertyActions.SetArg(self, 1, 35); }), new(-1, 2)], 0);
        actor.States.Enter(actor, 1); ActorDamage.Apply(actor, 1000);
        Assert.Equal(35, sim.LightOf(0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7, LightLevel = 160 }] });
}
