using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseInfightingFlagsTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void RevivalRestoresLevelInfightingPolicy(bool archvile, bool blocked, bool force)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        sim.Infighting = force ? -1 : 0;
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoInfighting = !force;
        corpse.ForceInfighting = force;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked && !force, corpse.NoInfighting);
        Assert.Equal(blocked && force, corpse.ForceInfighting);
        if (!blocked)
        {
            var source = sim.AddBot(200, 0, 3001);
            Assert.Equal(force ? 0 : 5, ActorDamage.Apply(corpse, 5, source, inflictor: source).HealthLost);
            if (!force) Assert.Equal(source.Id, corpse.Brain!.TargetId);
            else
            {
                corpse.ForceInfighting = true;
                Assert.Equal(5, ActorDamage.Apply(corpse, 5, source, inflictor: source).HealthLost);
            }
        }
    }
}
