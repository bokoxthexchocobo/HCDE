using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSetSpecialActionTests
{
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void SignedSpecialAndAllArgumentsRoundTrip(int special)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetSpecial(actor, special, int.MinValue, -1, 0, 1, int.MaxValue);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.SetSpecial(actor, 5); sim.RestoreState(state);
        Assert.Equal(special, actor.Special);
        Assert.Equal(new[] { int.MinValue, -1, 0, 1, int.MaxValue }, actor.SpecialArgs);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void AllZeroResetIsRetainedAndOmittedArgumentsAreReplaced()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetSpecial(actor, 112, 7, 35, 3, 4, 5);
        ActorPropertyActions.SetSpecial(actor, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.NotNull(state.Actors.Single().ActorSpecial);
        ActorPropertyActions.SetSpecial(actor, 112, 7, 99); sim.RestoreState(state);
        Assert.Equal(0, actor.Special); Assert.All(actor.SpecialArgs, arg => Assert.Equal(0, arg));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void FrameAssignmentRunsOnDeathRatherThanDuringAssignment()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, Action: self =>
            ActorPropertyActions.SetSpecial(self, 112, 7, 35)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1); Assert.Equal(160, sim.LightOf(0));
        ActorDamage.Apply(actor, 1000); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void ResetRemovesPreviouslyAssignedDeathEffect()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetSpecial(actor, 112, 7, 35); ActorPropertyActions.SetSpecial(actor, 0);
        ActorDamage.Apply(actor, 1000); Assert.Equal(160, sim.LightOf(0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7, LightLevel = 160 }] });
}
