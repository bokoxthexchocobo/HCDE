using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSwapActorTeleFogServiceTests
{
    [Fact]
    public void SwapsDistinctSourceAndDest()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }],
        });
        var actor = sim.Actors.First();
        ActorPropertyActions.SetTeleFog(actor, "Src", "Dst");
        Assert.Equal(1, AcsSwapActorTeleFogService.Swap(sim, actor, 0));
        Assert.Equal("Dst", actor.TeleFogSource);
        Assert.Equal("Src", actor.TeleFogDest);
    }
}
