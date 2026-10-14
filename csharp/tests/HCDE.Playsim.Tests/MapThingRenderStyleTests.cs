using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapThingRenderStyleTests
{
    [Theory]
    [InlineData("None", 0)]
    [InlineData("Normal", 1)]
    [InlineData("Translucent", 6)]
    [InlineData("Additive", 7)]
    [InlineData("Multiply", 14)]
    [InlineData("ColorAdd", 18)]
    public void ParsedStyleAndAlphaReachDefaultStartup(string name, int native)
    {
        var sim = Room(name); var actor = sim.Actors.Single();
        Assert.Equal(ActorRenderStyle.FromNative(native), actor.RenderStyle); Assert.Equal(0.25, actor.Alpha);
        Assert.Equal(ActorRenderStyle.OptFuzzy, actor.DefaultRenderStyle); Assert.Equal(0.5, actor.DefaultAlpha);
    }

    [Fact]
    public void UnknownMapStylePreservesClassDefault()
    {
        var sim = Room("unknown"); Assert.Equal(ActorRenderStyle.OptFuzzy, sim.Actors.Single().RenderStyle);
    }

    [Fact]
    public void SavedRuntimeStyleSurvivesIdentityReconstruction()
    {
        var sim = Room("None"); var actor = sim.Actors.Single();
        actor.RenderStyle = ActorRenderStyle.Translucent; actor.Alpha = 0.75;
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        actor = sim.Actors.Single(a => !a.Destroyed);
        Assert.Equal(ActorRenderStyle.Translucent, actor.RenderStyle); Assert.Equal(0.75, actor.Alpha);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room(string name)
    {
        var text = $$"""namespace = "ZDoom"; thing { type = 58; renderstyle = "{{name}}"; alpha = 0.25; skill3 = true; single = true; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        return AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
    }
}
