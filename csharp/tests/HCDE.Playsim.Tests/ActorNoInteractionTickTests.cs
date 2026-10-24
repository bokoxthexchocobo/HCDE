using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorNoInteractionTickTests
{
    [Fact]
    public void Tick_AppliesVelocityAndForcesNoBlockmap()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = [new LevelThing { Type = 3001, X = 64, Y = 64 }],
        });
        var actor = sim.Actors.First(a => a is not PlayerPawn);
        var startX = actor.X.ToDouble();
        actor.NoInteraction = true;
        actor.NoBlockmap = false;
        actor.VelocityX = Fixed.FromDouble(4);
        sim.Tick();
        Assert.True(actor.NoBlockmap);
        Assert.True(actor.X.ToDouble() > startX);
    }
}
