using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseDamageFlagsTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRevivalRestoresSupportedDamageFlagDefaults(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Buddha = corpse.FoilBuddha = corpse.ForcePain = corpse.NoPain = corpse.Painless = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.Buddha);
        Assert.Equal(blocked, corpse.FoilBuddha);
        Assert.Equal(blocked, corpse.ForcePain);
        Assert.Equal(blocked, corpse.NoPain);
        Assert.Equal(blocked, corpse.Painless);
        if (!blocked)
        {
            corpse.PainChance = 256;
            ActorDamage.Apply(corpse, 1);
            Assert.Equal(corpse.PainState, corpse.States.Current);
            Assert.True(ActorDamage.Apply(corpse, 1000).Killed);
        }
    }
}
