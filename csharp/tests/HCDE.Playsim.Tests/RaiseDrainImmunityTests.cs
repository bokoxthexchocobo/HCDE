using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseDrainImmunityTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevivalRestoresDrainEligibility(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.DontDrain = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.DontDrain);
        if (!blocked)
        {
            var player = sim.Players.Single();
            player.Health = 50;
            player.DrainStrength = 1;
            Assert.Equal(5, ActorDamage.Apply(corpse, 5, player).HealthLost);
            Assert.Equal(55, player.Health);
            corpse.DontDrain = true;
            Assert.Equal(5, ActorDamage.Apply(corpse, 5, player).HealthLost);
            Assert.Equal(55, player.Health);
        }
    }
}
