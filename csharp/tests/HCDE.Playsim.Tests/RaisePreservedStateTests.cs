using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaisePreservedStateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RevivalPreservesFlagGroupNineAndRenderState(bool archvile)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoAutoOffSkullFly = corpse.FullBright = true;
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.True(corpse.NoAutoOffSkullFly);
        Assert.True(corpse.FullBright);
    }
}
