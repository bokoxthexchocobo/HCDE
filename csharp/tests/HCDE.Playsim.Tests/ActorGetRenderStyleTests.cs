using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGetRenderStyleTests
{
    public static IEnumerable<object[]> NativeStyles => new (ActorBlendOperation Op, ActorAlphaFactor Src, ActorAlphaFactor Dst, byte Flags)[] {
        (ActorBlendOperation.None, ActorAlphaFactor.Zero, ActorAlphaFactor.Zero, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 2),
        (ActorBlendOperation.Fuzz, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 1),
        (ActorBlendOperation.FuzzOrAdd, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 10),
        (ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.One, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 12),
        (ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 8),
        (ActorBlendOperation.Shadow, ActorAlphaFactor.Zero, ActorAlphaFactor.Zero, 0),
        (ActorBlendOperation.ReverseSubtract, ActorAlphaFactor.Source, ActorAlphaFactor.One, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.One, 8),
        (ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.One, 12),
        (ActorBlendOperation.Add, ActorAlphaFactor.DestinationColor, ActorAlphaFactor.Zero, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.InverseDestinationColor, ActorAlphaFactor.Zero, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.SourceColor, ActorAlphaFactor.InverseSourceColor, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.One, ActorAlphaFactor.Zero, 0),
        (ActorBlendOperation.Add, ActorAlphaFactor.SourceColor, ActorAlphaFactor.One, 0)
    }.Select((style, index) => new object[] { index, new ActorRenderStyle(style.Op, style.Src, style.Dst, style.Flags) });

    [Theory]
    [MemberData(nameof(NativeStyles))]
    public void NativeTableIdentitySurvivesFreshSave(int index, ActorRenderStyle style)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.RenderStyle = style; actor.Alpha = 0;
        Assert.Equal(index, actor.GetRenderStyle());
        Assert.Equal(style, ActorRenderStyle.FromNative(index));
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(index, fresh.Players.Single().GetRenderStyle());
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(ActorBlendOperation.Subtract, 0)]
    [InlineData(ActorBlendOperation.FuzzOrSubtract, 0)]
    [InlineData(ActorBlendOperation.Add, 16)]
    [InlineData(ActorBlendOperation.Add, 3)]
    public void UnrecognizedStylesReturnMinusOneWithoutMutation(ActorBlendOperation operation, byte flags)
    {
        var actor = new Actor { RenderStyle = new(operation, ActorAlphaFactor.Source, ActorAlphaFactor.One, flags) };
        var style = actor.RenderStyle;
        Assert.Equal(-1, actor.GetRenderStyle()); Assert.Equal(style, actor.RenderStyle);
    }

    [Fact]
    public void NativeIdentityDiffersFromAcsPropertyAndTracksFadeChanges()
    {
        var actor = new Actor(); Assert.Equal(1, actor.GetRenderStyle());
        ActorPropertyActions.FadeOut(actor, 0.25, 0);
        Assert.Equal(6, actor.GetRenderStyle()); Assert.Equal(64, actor.RenderStyle.ToAcs());
        ActorPropertyActions.SetTranslucent(actor, 0.5, 1);
        Assert.Equal(7, actor.GetRenderStyle()); Assert.Equal(65, actor.RenderStyle.ToAcs());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
