using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseDeathControlTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevivalRestoresSupportedDeathControlDefaults(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.States.Configure(corpse,
            [new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(-1, 4)], 0);
        corpse.GenericFreezeDeath = 4;
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoIceDeath = corpse.ExtremeDeath = corpse.NoExtremeDeath = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.NoIceDeath);
        Assert.Equal(blocked, corpse.ExtremeDeath);
        Assert.Equal(blocked, corpse.NoExtremeDeath);
        if (!blocked)
        {
            Assert.True(ActorDamage.Apply(corpse, 1000, damageType: "Ice").Killed);
            Assert.Equal(4, corpse.States.Current);
        }
    }
}
