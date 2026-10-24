using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPoisonReceivedTests
{
    private static AuthoritySimulation Room()
        => AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { FloorHeight = 0, CeilingHeight = 128 }],
            Things =
            [
                new LevelThing { Type = 1 },
                new LevelThing { Type = 3001, X = 64, Y = 64 },
            ],
        });

    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.First(actor => actor is not PlayerPawn);

    [Fact]
    public void PoisonMobj_RespectsInvulnerabilityAndFoilInvul()
    {
        var target = new Actor { Invulnerable = true };
        var inflictor = new Actor();
        ActorPoisonReceived.Apply(target, inflictor, null, 4, 3, 35, "Fire");
        Assert.Equal(0, target.PoisonDurationReceived);
        inflictor.FoilInvul = true;
        ActorPoisonReceived.Apply(target, inflictor, null, 4, 3, 35, "Fire");
        Assert.Equal(3, target.PoisonDurationReceived);
        Assert.Equal(4, target.PoisonDamageReceived);
        Assert.Equal("Fire", target.PoisonDamageTypeReceived);
    }

    [Fact]
    public void PoisonMobj_AdditiveDamageAndDurationAccumulate()
    {
        var target = new Actor();
        var inflictor = new Actor { AdditivePoisonDamage = true, AdditivePoisonDuration = true };
        ActorPoisonReceived.Apply(target, inflictor, null, 2, 1, 10, null);
        ActorPoisonReceived.Apply(target, inflictor, null, 3, 2, 10, null);
        Assert.Equal(5, target.PoisonDamageReceived);
        Assert.Equal(3, target.PoisonDurationReceived);
    }

    [Fact]
    public void Tick_AppliesDamageOnPeriodAndClearsDamageWhenDurationEnds()
    {
        var sim = Room();
        var monster = Monster(sim);
        monster.Health = 10;
        var source = sim.Actors.First();
        ActorPoisonReceived.Apply(monster, new Actor(), source, 5, 2, 35, null);
        sim.Thinkers.Clock.Restore(0);
        monster.Tick();
        Assert.Equal(5, monster.Health);
        Assert.Equal(1, monster.PoisonDurationReceived);
        Assert.Equal(5, monster.PoisonDamageReceived);
        sim.Thinkers.Clock.Restore(35);
        monster.Tick();
        Assert.True(monster.IsDead);
        Assert.Equal(0, monster.PoisonDurationReceived);
        Assert.Equal(0, monster.PoisonDamageReceived);
    }

    [Fact]
    public void Tick_SkipsOffCadenceTics()
    {
        var sim = Room();
        var monster = Monster(sim);
        monster.Health = 40;
        ActorPoisonReceived.Apply(monster, new Actor(), null, 4, 3, 35, null);
        sim.Thinkers.Clock.Restore(17);
        monster.Tick();
        Assert.Equal(40, monster.Health);
        Assert.Equal(3, monster.PoisonDurationReceived);
    }
}
