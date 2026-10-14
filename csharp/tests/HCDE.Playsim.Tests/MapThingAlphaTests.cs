using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapThingAlphaTests
{
    [Theory]
    [InlineData(-1, 0.5)]
    [InlineData(-2, 0.5)]
    [InlineData(0, 0)]
    [InlineData(0.25, 0.25)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public void MapAlphaOverridesOnlyForNonnegativeValues(double alpha, double expected)
    {
        var sim = Room(alpha); var actor = sim.Actors.Single();
        Assert.Equal(expected, actor.Alpha); Assert.Equal(0.5, actor.DefaultAlpha);
        Assert.Equal(ActorRenderStyle.OptFuzzy, actor.RenderStyle);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.25)]
    [InlineData(1)]
    public void SavedRuntimeAlphaSurvivesMapActorReconstruction(double mapAlpha)
    {
        var sim = Room(mapAlpha); var actor = sim.Actors.Single(); actor.Alpha = 0.75;
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        actor = sim.Actors.Single(a => !a.Destroyed);
        Assert.Equal(0.75, actor.Alpha); Assert.Equal(0.5, actor.DefaultAlpha);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ParsedAlphaReachesDefaultStartup()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { type = 58; alpha = 0.25; skill3 = true; single = true; }", out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        Assert.Equal(0.25, sim.Actors.Single().Alpha);
    }

    private static AuthoritySimulation Room(double alpha) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 58, Alpha = alpha }]
    });
}
