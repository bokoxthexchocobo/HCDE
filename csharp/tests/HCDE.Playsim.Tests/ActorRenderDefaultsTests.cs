using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRenderDefaultsTests
{
    [Theory]
    [InlineData(58, null, true, 0.5)]
    [InlineData(3004, null, false, 1)]
    [InlineData(58, "Thing 14\nBits = 6\n", false, 0.5)]
    [InlineData(3004, "Thing 2\nBits = SOLID + SHOOTABLE + SHADOW\n", true, 0.5)]
    public void ClassAndShadowPatchDefaultsApplyToMapDynamicAndLegacySaves(int type, string? patch, bool fuzzy, double alpha)
    {
        var sim = Room(type, patch);
        var actor = sim.Actors.Single();
        var style = fuzzy ? ActorRenderStyle.OptFuzzy : ActorRenderStyle.Normal;
        Assert.Equal(style, actor.RenderStyle); Assert.Equal(alpha, actor.Alpha);
        var dynamic = sim.AddBot(500, 0, type);
        Assert.Equal(style, dynamic.RenderStyle); Assert.Equal(alpha, dynamic.Alpha);
        var legacy = sim.CaptureState();
        foreach (var pose in legacy.Actors) pose.RenderProperties = null;
        var bytes = SimSavegame.Write(legacy);
        actor.RenderStyle = ActorRenderStyle.Translucent; actor.Alpha = 0;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(style, actor.RenderStyle); Assert.Equal(alpha, actor.Alpha);
    }

    [Theory]
    [InlineData(58)]
    [InlineData(3004)]
    public void ExplicitNormalOverrideSurvivesSaveAndResurrectionPreservesRenderState(int type)
    {
        var sim = Room(type, null); var corpse = sim.Actors.Single();
        corpse.RenderStyle = ActorRenderStyle.Normal; corpse.Alpha = 1;
        var bytes = SimSavegame.Write(sim);
        corpse.RenderStyle = ActorRenderStyle.Translucent; corpse.Alpha = 0;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(ActorRenderStyle.Normal, corpse.RenderStyle); Assert.Equal(1, corpse.Alpha);
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.RenderStyle = ActorRenderStyle.Translucent; corpse.Alpha = 0.25;
        bytes = SimSavegame.Write(sim);
        for (var pass = 0; pass < 2; pass++)
        {
            Assert.True(ThingRaise.Execute(sim, 17, corpse, 0, 0));
            Assert.Equal(ActorRenderStyle.Translucent, corpse.RenderStyle);
            Assert.Equal(0.25, corpse.Alpha);
            SimSavegame.Apply(sim, bytes);
        }
    }

    [Fact]
    public void NormalPresetUsesNativePackedFactors()
    {
        Assert.Equal(ActorAlphaFactor.Source, ActorRenderStyle.Normal.Source);
        Assert.Equal(ActorAlphaFactor.InverseSource, ActorRenderStyle.Normal.Destination);
        Assert.Equal(2, ActorRenderStyle.Normal.Flags);
    }

    private static AuthoritySimulation Room(int type, string? patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = type }],
    }, dehacked: patch == null ? null : DehackedPatch.Apply(patch));
}
