using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseGhostSpectralTests
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
    public void RevivalRestoresSupportedGhostAndSpectralDefaults(bool archvile, bool blocked, bool spectral)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Ghost = !spectral;
        corpse.Spectral = spectral;
        corpse.ThruGhost = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked && !spectral, corpse.Ghost);
        Assert.Equal(blocked && spectral, corpse.Spectral);
        Assert.Equal(blocked, corpse.ThruGhost);
        if (!blocked)
        {
            var health = corpse.Health;
            var owner = sim.AddBot(-200, 0, 3001);
            owner.Brain = corpse.Brain = null;
            var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
            missile.ThruGhost = !spectral;
            missile.X = missile.Y = default;
            missile.Z = Fixed.FromInt(20);
            missile.VelocityX = Fixed.FromInt(30);
            missile.VelocityY = missile.VelocityZ = default;
            sim.Tick();
            Assert.True(missile.Destroyed);
            Assert.True(corpse.Health < health);
        }
    }
}
