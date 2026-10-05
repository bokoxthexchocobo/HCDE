using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseScrollStateTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevivalClearsScrollStateUntilNormalContactUpdate(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.InScrollSector = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.InScrollSector);
        if (!blocked)
        {
            sim.SetSectorScroll(0, 4, 0);
            var x = corpse.X;
            ActorPhysics.ApplySectorScroll(sim, corpse);
            Assert.Equal(x, corpse.X);
            sim.MarkInScrollSectorActors();
            Assert.True(corpse.InScrollSector);
            ActorPhysics.ApplySectorScroll(sim, corpse);
            Assert.Equal(x.ToDouble() + 4, corpse.X.ToDouble());
        }
    }
}
