using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedRenderPropertiesTests
{
    [Theory]
    [InlineData("None", false)]
    [InlineData("Normal", true)]
    [InlineData("Fuzzy", true)]
    [InlineData("SoulTrans", false)]
    [InlineData("OptFuzzy", true)]
    [InlineData("Translucent", false)]
    [InlineData("Add", false)]
    [InlineData("Shaded", false)]
    public void SupportedNativeNamesReachActorsAndSaveReplay(string name, bool visible)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nTranslucency = 0\nRender Style = sTyLe_{name}\n");
        Assert.Empty(patch.Errors);
        var sim = Room(patch); var actor = sim.Actors.Single();
        var style = ActorRenderStyle.FromDehacked(Enum.Parse<DehackedRenderStyle>(name));
        Assert.Equal(style, actor.RenderStyle); Assert.Equal(0, actor.Alpha);
        var seeker = new Actor { DontSeekInvisible = true };
        Assert.Equal(visible, seeker.CanSeek(actor));
        var bytes = SimSavegame.Write(sim);
        actor.RenderStyle = ActorRenderStyle.Normal; actor.Alpha = 1;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(style, actor.RenderStyle); Assert.Equal(0, actor.Alpha);
        var dynamic = sim.AddBot(500, 0, 3004);
        Assert.Equal(style, dynamic.RenderStyle); Assert.Equal(0, dynamic.Alpha);
    }

    [Theory]
    [InlineData("Translucency = 16384\nRender Style = STYLE_Normal\nBits = 262150\n", DehackedRenderStyle.Normal, 0.25)]
    [InlineData("Render Style = STYLE_Normal\nTranslucency = 16384\nBits = 262150\n", DehackedRenderStyle.Translucent, 0.25)]
    [InlineData("Render Style = STYLE_Add\nBits = 262150\n", DehackedRenderStyle.Add, 0.5)]
    [InlineData("Translucency = -32768\n", DehackedRenderStyle.Translucent, -0.5)]
    [InlineData("Translucency = 131072\n", DehackedRenderStyle.Translucent, 2)]
    public void FieldOrderAndShadowFallbackMatchNative(string body, DehackedRenderStyle style, double alpha)
    {
        var patch = DehackedPatch.Apply("Thing 2\n" + body); Assert.Empty(patch.Errors);
        var actor = Room(patch).Actors.Single();
        Assert.Equal(ActorRenderStyle.FromDehacked(style), actor.RenderStyle);
        Assert.Equal(alpha, actor.Alpha);
    }

    [Fact]
    public void ShadowTransitionUsesCurrentSectionNotEarlierExplicitStyle()
    {
        var baseline = DehackedPatch.Apply("Thing 2\nRender Style = STYLE_Add\nTranslucency = 16384\n");
        var patch = DehackedPatch.Apply("Thing 2\nBits = 262150\nThing 2\nBits = 6\n", baseline);
        Assert.Empty(patch.Errors);
        var actor = Room(patch).Actors.Single();
        Assert.Equal(ActorRenderStyle.Normal, actor.RenderStyle); Assert.Equal(0.5, actor.Alpha);
        Assert.Equal(DehackedRenderStyle.Translucent, baseline.Actors[1].RenderStyle);
        Assert.Equal(0.25, baseline.Actors[1].Alpha);
    }

    [Theory]
    [InlineData("STYLE_1")]
    [InlineData("STYLE_Stencil")]
    [InlineData("Normal")]
    public void UnknownNativePatchNamesReportErrorsAndPreserveStyle(string name)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nRender Style = STYLE_Add\nRender Style = {name}\n");
        Assert.Single(patch.Errors);
        Assert.Equal(ActorRenderStyle.FromDehacked(DehackedRenderStyle.Add), Room(patch).Actors.Single().RenderStyle);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch);
}
