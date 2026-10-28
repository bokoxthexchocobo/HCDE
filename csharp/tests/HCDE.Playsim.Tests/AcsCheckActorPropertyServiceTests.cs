using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsCheckActorPropertyServiceTests
{
    [Fact]
    public void Check_MatchesGetForHealthAndFlags()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }],
        });
        var actor = sim.Actors.First();
        actor.Health = 88;
        actor.Frightened = true;
        Assert.True(AcsCheckActorPropertyService.Check(sim, actor, 0, AcsActorProperties.Health, 88));
        Assert.True(AcsCheckActorPropertyService.Check(sim, actor, 0, AcsActorProperties.Frightened, 1));
        Assert.False(AcsCheckActorPropertyService.Check(sim, actor, 0, AcsActorProperties.Health, 1));
    }
}
