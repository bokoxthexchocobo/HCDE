using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseTargetabilityTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRaiseRestoresRetaliationTargetability(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 },
                new LevelThing { Type = 3001, X = 200 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoTarget = corpse.NeverTarget = true;
        if (blocked) sim.Ceilings[0] = 10;

        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.NoTarget);
        Assert.Equal(blocked, corpse.NeverTarget);
        if (!blocked)
        {
            var enemy = sim.Actors[2];
            ActorDamage.Apply(enemy, 1, corpse);
            Assert.Equal(corpse.Id, enemy.Brain!.TargetId);
        }
    }
}
