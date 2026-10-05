using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseIceShatterPermissionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevivalRestoresIceShatterPermission(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.IceShatter = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.IceShatter);
        if (!blocked)
        {
            var frozen = sim.AddBot(200, 0, 3001);
            frozen.Health = 0;
            frozen.IceCorpse = true;
            ActorDamage.Apply(frozen, 5, inflictor: corpse, damageType: "Ice");
            Assert.False(frozen.Shattering);
            corpse.IceShatter = true;
            ActorDamage.Apply(frozen, 5, inflictor: corpse, damageType: "Ice");
            Assert.True(frozen.Shattering);
        }
    }
}
