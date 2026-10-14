using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapThingScaleTests
{
    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(2, 0, 2, 1)]
    [InlineData(0, -2, 1, -2)]
    [InlineData(0.5, 3, 0.5, 3)]
    public void StartupAppliesMapScaleWithoutChangingCollisionSize(double x, double y, double expectedX, double expectedY)
    {
        var sim = Room(x, y); var actor = sim.Actors.Single(); var baseline = Room(0, 0).Actors.Single();
        Assert.Equal(expectedX, actor.ScaleX); Assert.Equal(expectedY, actor.ScaleY);
        Assert.Equal(baseline.Radius, actor.Radius); Assert.Equal(baseline.Height, actor.Height);
    }

    [Fact]
    public void SavedRuntimeScaleSurvivesMapActorReconstruction()
    {
        var sim = Room(2, 3); var actor = sim.Actors.Single(); actor.ScaleX = 4; actor.ScaleY = -1;
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        actor = sim.Actors.Single(a => !a.Destroyed);
        Assert.Equal(4, actor.ScaleX); Assert.Equal(-1, actor.ScaleY); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ParsedScaleReachesDefaultStartup()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { type = 3004; scale = 2; scaley = 3; skill3 = true; single = true; }", out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01")); var actor = sim.Actors.Single();
        Assert.Equal(2, actor.ScaleX); Assert.Equal(3, actor.ScaleY);
    }

    private static AuthoritySimulation Room(double x, double y) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, ScaleX = x, ScaleY = y }]
    });
}
