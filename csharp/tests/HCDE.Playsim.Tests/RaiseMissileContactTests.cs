using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseMissileContactTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRaiseRestoresDirectMissileContact(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NonShootable = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.NonShootable);
        if (!blocked)
        {
            var health = corpse.Health;
            var owner = sim.AddBot(-200, 0, 3001);
            owner.Brain = corpse.Brain = null;
            var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
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
