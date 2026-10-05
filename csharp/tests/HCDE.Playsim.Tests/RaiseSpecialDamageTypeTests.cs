using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseSpecialDamageTypeTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RaiseRestoresSupportedClassDamageDefaultsOnlyOnSuccess(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.FoilInvul = true;
        corpse.PierceArmor = true;
        corpse.SpecialFireDamage = true;
        corpse.DamageType = "Fire";
        corpse.DeathType = "Ice";
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.FoilInvul);
        Assert.Equal(blocked, corpse.PierceArmor);
        Assert.Equal(blocked, corpse.SpecialFireDamage);
        Assert.Equal(blocked ? "Fire" : null, corpse.DamageType);
        // Native Revive restores DamageType and flag words, but does not reset DeathType.
        Assert.Equal("Ice", corpse.DeathType);
        if (!blocked)
        {
            var protectedTarget = new Actor { Health = 100, Invulnerable = true };
            Assert.Equal(0, ActorDamage.Apply(protectedTarget, 20, inflictor: corpse).HealthLost);
        }
    }
}
