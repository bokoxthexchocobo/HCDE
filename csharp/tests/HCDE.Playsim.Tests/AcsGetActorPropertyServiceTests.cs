using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsGetActorPropertyServiceTests
{
    [Fact]
    public void Get_ReturnsMassAndScore()
    {
        var level = new PlayLevel { MapName = "MAP01", Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }] };
        var sim = AuthoritySimulation.Start(level);
        var actor = sim.Actors.First();
        actor.Mass = 400;
        actor.Score = 12;
        Assert.Equal(400, AcsGetActorPropertyService.Get(sim, actor, 0, AcsActorProperties.Mass));
        Assert.Equal(12, AcsGetActorPropertyService.Get(sim, actor, 0, AcsActorProperties.Score));
    }

    [Fact]
    public void Check_ComparesBooleanAmbush()
    {
        var level = new PlayLevel { MapName = "MAP01", Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }] };
        var sim = AuthoritySimulation.Start(level);
        var actor = sim.Actors.First();
        actor.Ambush = true;
        Assert.True(AcsGetActorPropertyService.Check(sim, actor, 0, AcsActorProperties.Ambush, 1));
        Assert.False(AcsGetActorPropertyService.Check(sim, actor, 0, AcsActorProperties.Ambush, 0));
    }
}
