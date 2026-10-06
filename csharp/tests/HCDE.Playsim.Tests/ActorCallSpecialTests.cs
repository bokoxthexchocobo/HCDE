using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorCallSpecialTests
{
    [Fact]
    public void FrameExecutesImmediatelyWithoutChangingStoredDeathSpecial()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetSpecial(actor, 112, 7, 99); actor.ActivationType = 32;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0, Action: self =>
            Assert.True(ActorSpecialActions.CallSpecial(self, 112, 7, 35)))], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(112, actor.Special);
        Assert.Equal(new[] { 7, 99, 0, 0, 0 }, actor.SpecialArgs); Assert.Equal(32, actor.ActivationType);
        ActorDamage.Apply(actor, 1000); Assert.Equal(99, sim.LightOf(0));
    }

    [Fact]
    public void AllFiveArgumentsReachDispatcherWithCallerAsActivator()
    {
        var actor = Room().AddBot(0, 0);
        Assert.True(ActorSpecialActions.CallSpecial(actor, 127, 0, 112, int.MinValue, -1, int.MaxValue));
        Assert.Equal(112, actor.Special);
        Assert.Equal(new[] { int.MinValue, -1, int.MaxValue, 0, 0 }, actor.SpecialArgs);
    }

    [Fact]
    public void OmittedArgumentsAreZeroAndActivatorReceivesEffect()
    {
        var actor = Room().AddBot(0, 0); actor.VelocityX = Fixed.FromInt(4);
        Assert.True(ActorSpecialActions.CallSpecial(actor, 19));
        Assert.Equal(0, actor.VelocityX.Raw);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void UnsupportedSpecialReturnsFalseWithoutActorChanges(int special)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var bytes = SimSavegame.Write(sim);
        Assert.False(ActorSpecialActions.CallSpecial(actor, special, 1, 2, 3, 4, 5));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ResultingActorVelocitySurvivesSaveRestore()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.True(ActorSpecialActions.CallSpecial(actor, 128, 0, 12));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorSpecialActions.CallSpecial(actor, 128, 0, 20); sim.RestoreState(state);
        Assert.Equal(3, actor.VelocityZ.ToDouble()); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void DetachedActorRequiresSimulation()
    {
        Assert.Throws<InvalidOperationException>(() => ActorSpecialActions.CallSpecial(new Actor(), 112, 7, 35));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7, LightLevel = 160 }] });
}
