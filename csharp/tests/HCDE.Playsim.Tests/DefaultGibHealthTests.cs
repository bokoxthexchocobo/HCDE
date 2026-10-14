using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DefaultGibHealthTests
{
    [Theory]
    [InlineData(20)]
    [InlineData(100)]
    [InlineData(4000)]
    public void SentinelUsesSpawnHealthInsteadOfCurrentHealth(int spawnHealth)
    {
        var actor = new Actor { ResurrectionHealth = spawnHealth, GibHealth = int.MinValue, Health = 1 };
        Assert.Equal(-spawnHealth, actor.GetGibHealth()); Assert.Equal(int.MinValue, actor.GibHealth);
    }

    [Theory]
    [InlineData(200, ActorStateMachine.Death)]
    [InlineData(201, ActorStateMachine.Corpse)]
    public void SentinelExtremeDeathKeepsStrictSpawnHealthBoundary(int damage, int state)
    {
        var actor = new Actor { ResurrectionHealth = 100, GibHealth = int.MinValue, ExtremeDeathState = ActorStateMachine.Corpse };
        ActorDamage.Apply(actor, damage); Assert.Equal(state, actor.States.Current);
    }

    [Fact]
    public void ForcedExtremeSentinelDeathStaysDeadInsteadOfOverflowingHealth()
    {
        var actor = new Actor { ResurrectionHealth = 100, GibHealth = int.MinValue, ExtremeDeathState = ActorStateMachine.Corpse };
        ActorDamage.Apply(actor, 100, damageType: "Extreme");
        Assert.Equal(-101, actor.Health); Assert.True(actor.IsDead);
    }

    [Fact]
    public void LaterSpawnHealthChangesAreObservedBySentinel()
    {
        var actor = new Actor { ResurrectionHealth = 100, GibHealth = int.MinValue };
        Assert.Equal(-100, actor.GetGibHealth()); actor.ResurrectionHealth = 250;
        Assert.Equal(-250, actor.GetGibHealth()); actor.GibHealth = 10;
        Assert.Equal(-10, actor.GetGibHealth());
    }

    [Theory]
    [InlineData(3004, 20)]
    [InlineData(3001, 60)]
    [InlineData(3003, 1000)]
    public void MapActorSentinelUsesRepresentedClassSpawnHealth(int type, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = type }]
        });
        var actor = sim.Actors.Single(); actor.GibHealth = int.MinValue;
        Assert.Equal(-expected, actor.GetGibHealth());
    }
}
