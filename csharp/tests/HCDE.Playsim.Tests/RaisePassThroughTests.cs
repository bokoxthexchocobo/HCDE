using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaisePassThroughTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRaiseRestoresActorContactBlocking(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.ThruActors = corpse.ThruSpecies = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.ThruActors);
        Assert.Equal(blocked, corpse.ThruSpecies);
        if (!blocked)
        {
            var player = sim.Players.Single();
            player.X = corpse.X; player.Y = corpse.Y;
            Assert.False(ActorPhysics.CanOccupy(sim, corpse));
        }
    }
}
