using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorKillMissileTests
{
    [Theory]
    [InlineData(false, false, false, 6, true)]
    [InlineData(true, false, false, 6, false)]
    [InlineData(true, false, false, 7, true)]
    [InlineData(false, true, false, 6, false)]
    [InlineData(false, true, false, 14, true)]
    [InlineData(true, true, false, 15, true)]
    [InlineData(false, false, true, 15, false)]
    public void KillMissilesHonorsProtectionEvenWithNoMonsters(bool invulnerable, bool buddha,
        bool noDamage, int flags, bool exploded)
    {
        var sim = Room(); var caller = sim.Players.Single();
        var missile = sim.SpawnProjectile(caller, ProjectileKind.Plasma);
        caller.MasterId = missile.Id;
        missile.Invulnerable = invulnerable; missile.Buddha = buddha; missile.NoDamage = noDamage;
        ActorHealthActions.KillMaster(caller, flags: flags);
        Assert.Equal(exploded, missile.Destroyed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void AllKillActionsSelectAndExplodeMissiles(int selection)
    {
        var sim = Room(); var caller = sim.Players.Single();
        var missile = sim.SpawnProjectile(caller, ProjectileKind.Plasma);
        switch (selection)
        {
            case 0: caller.SetTargetPointer(missile); ActorHealthActions.KillTarget(caller, flags: 6); break;
            case 1: caller.MasterId = missile.Id; ActorHealthActions.KillMaster(caller, flags: 6); break;
            case 2: caller.TracerId = missile.Id; ActorHealthActions.KillTracer(caller, flags: 6); break;
            case 3: missile.MasterId = caller.Id; ActorHealthActions.KillChildren(caller, flags: 6); break;
            case 4:
                var master = sim.AddBot(400, 0);
                caller.MasterId = missile.MasterId = master.Id;
                ActorHealthActions.KillSiblings(caller, flags: 6); break;
        }
        Assert.True(missile.Destroyed); Assert.Equal(100, caller.Health);
    }

    [Theory]
    [InlineData(6, false)]
    [InlineData(22, true)]
    public void ClassFilterRunsBeforeMissileExplosion(int flags, bool exploded)
    {
        var sim = Room(); var caller = sim.Players.Single();
        var missile = sim.SpawnProjectile(caller, ProjectileKind.Plasma);
        caller.TracerId = missile.Id;
        ActorHealthActions.KillTracer(caller, flags: flags, classFilter: "DoomImp");
        Assert.Equal(exploded, missile.Destroyed);
    }

    [Fact]
    public void KillingRocketRunsExplosionDamage()
    {
        var sim = Room(); var caller = sim.Players.Single();
        var missile = sim.SpawnProjectile(caller, ProjectileKind.Rocket);
        var victim = sim.AddBot(60, 0, 3001); caller.TracerId = missile.Id;
        var health = victim.Health;
        ActorHealthActions.KillTracer(caller, flags: 6);
        Assert.True(missile.Destroyed); Assert.True(victim.Health < health);
        Assert.Equal(caller.Id, victim.LastDamageSourceId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
