using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialBaselineRestoreTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void BaselineRestoresEachLaterArgument(int index)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.Null(sim.CaptureState().Actors.Single().ActorSpecial);
        ActorPropertyActions.SetArg(actor, index, int.MinValue);
        SimSavegame.Apply(sim, bytes);
        Assert.All(actor.SpecialArgs, arg => Assert.Equal(0, arg));
        Assert.False(actor.SpecialChanged); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void BaselineRemovesLaterSpecialBeforeActivation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        actor.Special = 112; actor.SpecialArgs[0] = 7; actor.SpecialArgs[1] = 35;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(0, actor.Special); Assert.False(actor.SpecialChanged);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.False(actor.ActivateSpecial(null)); Assert.Equal(160, sim.LightOf(0));
    }

    [Fact]
    public void BaselineRemovesLaterActivationFlagsWhileExplicitClearRemainsRecorded()
    {
        var sim = Room(); var baseline = sim.AddBot(0, 0); var cleared = sim.AddBot(100, 0);
        cleared.ActivationType = 0;
        var bytes = SimSavegame.Write(sim);
        baseline.ActivationType = cleared.ActivationType = 256;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(0, baseline.ActivationType); Assert.Equal(0, cleared.ActivationType);
        // This archive extension writes a record for every actor when any actor has one.
        Assert.True(baseline.SpecialChanged); Assert.True(cleared.SpecialChanged);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.False(baseline.ActivateSpecial(null)); Assert.False(cleared.ActivateSpecial(null));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7, LightLevel = 160 }] });
}
