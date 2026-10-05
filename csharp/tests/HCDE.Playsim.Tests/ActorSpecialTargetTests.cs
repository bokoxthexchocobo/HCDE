using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialTargetTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(66)]
    [InlineData(68)]
    public void DeathFlagsSwitchMonsterTargetsBeforeChoosingActivator(int activation)
    {
        var sim = Room(); var victim = sim.Actors[1]; var killer = sim.Actors[2];
        victim.ActivationType = activation;
        ActorDamage.Apply(victim, 1000, killer);
        var suppressed = (activation & 64) != 0;
        Assert.Equal(!suppressed && (activation & 2) != 0 ? killer.Id : (uint?)null, victim.Brain!.TargetId);
        Assert.Equal(!suppressed && (activation & 4) != 0 ? victim.Id : (uint?)null, killer.Brain!.TargetId);
    }

    [Fact]
    public void ThingTargetsClearsExistingTargetWithNullDamageSource()
    {
        var sim = Room(); var victim = sim.Actors[1]; var killer = sim.Actors[2];
        victim.ActivationType = 2;
        ActorDamage.Apply(victim, 1000, killer);
        Assert.Equal(killer.Id, victim.Brain!.TargetId);
        victim.Health = 20;
        ActorDamage.Apply(victim, 1000, null);
        Assert.Null(victim.Brain.TargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        HexenHack = true, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20, Special = 19 },
            new LevelThing { Type = 3001, X = 100 }],
    });
}
