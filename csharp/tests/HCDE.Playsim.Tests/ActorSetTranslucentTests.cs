using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSetTranslucentTests
{
    public static IEnumerable<object[]> Cases => new[] { 0, 1, 2, -1, 99 }
        .SelectMany(mode => new[] { -2.0, 0.25, 3.0 }.Select(alpha => new object[] { mode, alpha }));

    [Theory]
    [MemberData(nameof(Cases))]
    public void LegacyModeMappingClampsAlphaAndSurvivesFreshSave(int mode, double alpha)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.SetTranslucent(actor, alpha, mode);
        Assert.Equal(Math.Clamp(alpha, 0, 1), actor.Alpha);
        Assert.Equal(mode == 2 ? ActorBlendOperation.Fuzz : ActorBlendOperation.Add, actor.RenderStyle.Operation);
        Assert.Equal(ActorAlphaFactor.Source, actor.RenderStyle.Source);
        Assert.Equal(mode is 0 or 2 ? ActorAlphaFactor.InverseSource : ActorAlphaFactor.One, actor.RenderStyle.Destination);
        Assert.Equal(0, actor.RenderStyle.Flags);
        Assert.Equal(ActorRenderStyle.Normal, actor.DefaultRenderStyle); Assert.Equal(1, actor.DefaultAlpha);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(actor.RenderStyle, fresh.Players.Single().RenderStyle);
        Assert.Equal(actor.Alpha, fresh.Players.Single().Alpha);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidAlphaFailsBeforeMutatingStyle(double alpha)
    {
        var sim = Room(); var saved = SimSavegame.Write(sim, true);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.SetTranslucent(sim.Players.Single(), alpha, 2));
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void DefaultModeClearsOpaqueAlphaOverrideAndWorksAsStateAction()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, a => ActorPropertyActions.SetTranslucent(a, 0))], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(ActorRenderStyle.Translucent, actor.RenderStyle);
        Assert.False(actor.RenderStyle.IsVisible(actor.Alpha));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
    });
}
