using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFadeActionTests
{
    [Theory]
    [InlineData(false, 0, -0.25, false)]
    [InlineData(false, 1, -0.25, true)]
    [InlineData(false, 2, 0, false)]
    [InlineData(false, 3, 0, true)]
    [InlineData(true, 0, 1.25, false)]
    [InlineData(true, 1, 1.25, true)]
    [InlineData(true, 2, 1, false)]
    [InlineData(true, 3, 1, true)]
    public void FadeFlagsAndRemovalMatchNative(bool fadeIn, int flags, double expected, bool removed)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Alpha = 0.5;
        actor.RenderStyle = ActorRenderStyle.Normal with { Flags = 10 };
        if (fadeIn) ActorPropertyActions.FadeIn(actor, 0.75, flags);
        else ActorPropertyActions.FadeOut(actor, 0.75, flags);
        Assert.Equal(expected, actor.Alpha); Assert.Equal(8, actor.RenderStyle.Flags);
        Assert.Equal(removed, actor.Destroyed);
    }

    [Theory]
    [InlineData(true, 0, 0.6)]
    [InlineData(false, 0, 0.4)]
    [InlineData(true, -0.25, 0.25)]
    [InlineData(false, -0.25, 0.75)]
    public void ZeroUsesNativeStepAndNegativeStepsReverseFade(bool fadeIn, double amount, double expected)
    {
        var actor = new Actor { Alpha = 0.5 };
        if (fadeIn) ActorPropertyActions.FadeIn(actor, amount); else ActorPropertyActions.FadeOut(actor, amount, 0);
        Assert.Equal(expected, actor.Alpha, 12); Assert.False(actor.Destroyed);
    }

    [Theory]
    [InlineData(0.25, 0.5, 0, 0.25, false)]
    [InlineData(0.75, 0.5, 1, 0.75, true)]
    [InlineData(0.5, 0, 1, 0.5, true)]
    [InlineData(0.75, 0, 0, 0.5, false)]
    [InlineData(0.75, -0.25, 0, 0.25, false)]
    [InlineData(2, 3, 3, 1, false)]
    public void FadeToTargetOvershootClampAndRemoveOrder(double target, double amount, int flags, double expected, bool removed)
    {
        var actor = new Actor { Alpha = 0.5 };
        ActorPropertyActions.FadeTo(actor, target, amount, flags);
        Assert.Equal(expected, actor.Alpha); Assert.Equal(removed, actor.Destroyed);
        Assert.Equal(0, actor.RenderStyle.Flags);
    }

    [Theory]
    [InlineData(6, -1, 0)]
    [InlineData(6, 2, 1)]
    [InlineData(17, 0.25, 0.25)]
    [InlineData(-1, 0.5, 0.5)]
    [InlineData(99, 0.5, 0.5)]
    public void SetStyleClampsAlphaAndUsesNativeIndices(int mode, double alpha, double expected)
    {
        var actor = new Actor(); ActorPropertyActions.SetRenderStyle(actor, alpha, mode);
        Assert.Equal(expected, actor.Alpha); Assert.Equal(ActorRenderStyle.FromNative(mode), actor.RenderStyle);
        if (mode == 6) Assert.Equal(ActorRenderStyle.Translucent, actor.RenderStyle);
        if (mode < 0 || mode == 99) Assert.Equal(ActorBlendOperation.None, actor.RenderStyle.Operation);
    }

    [Fact]
    public void StateActionFadeAffectsSeekingAndReplaysFromSave()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0, Action: self => ActorPropertyActions.FadeOut(self, 1, 0))], 0);
        var bytes = SimSavegame.Write(sim);
        for (var pass = 0; pass < 2; pass++)
        {
            actor.States.Enter(actor, 1);
            Assert.Equal(0, actor.Alpha); Assert.False(actor.Destroyed);
            Assert.False(new Actor { DontSeekInvisible = true }.CanSeek(actor));
            var faded = SimSavegame.Write(sim); actor.Alpha = 1;
            SimSavegame.Apply(sim, faded); Assert.Equal(0, actor.Alpha);
            SimSavegame.Apply(sim, bytes); Assert.Equal(1, actor.Alpha);
        }
    }

    [Fact]
    public void DefaultFadeOutRemovesAndInvalidInputsDoNotChangeState()
    {
        var actor = new Actor { Alpha = 0.05 };
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.FadeOut(actor, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.FadeTo(actor, double.PositiveInfinity));
        Assert.Equal(0.05, actor.Alpha); Assert.Equal(ActorRenderStyle.Normal, actor.RenderStyle);
        ActorPropertyActions.FadeOut(actor); Assert.True(actor.Destroyed);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });

    private sealed class NonMapActor : Actor
    {
        public override bool IsMapActor() => false;
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void RemovalProtectsPlayerAndNonMapActors(int action, bool player)
    {
        Actor actor = player ? new PlayerPawn() : new NonMapActor();
        actor.Alpha = 0.5;
        if (action == 0) ActorPropertyActions.FadeOut(actor, 1, 1);
        else if (action == 1) ActorPropertyActions.FadeIn(actor, 1, 1);
        else ActorPropertyActions.FadeTo(actor, 0.5, 1, 1);
        Assert.False(actor.Destroyed);
    }
}
