using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GlobalDamageTypeFactorTests
{
    [Theory]
    [InlineData(false, false, 10)]
    [InlineData(false, true, 5)]
    [InlineData(true, true, 10)]
    [InlineData(true, false, 10)]
    public void GlobalFactorMultipliesOrReplacesActorFallback(bool replace, bool fallback, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single();
        sim.DamageTypes.Define("Fire", 0.5, replace);
        if (fallback) actor.SetDamageFactor("None", 0.5);
        Assert.Equal(expected, ActorDamage.Apply(actor, 20, damageType: "fIrE").HealthLost);
    }

    [Fact]
    public void ExactActorFactorWinsOverGlobalReplacement()
    {
        var sim = Room(); var actor = sim.Players.Single();
        sim.DamageTypes.Define("Fire", 0, true); actor.SetDamageFactor("Fire", 2);
        Assert.Equal(40, ActorDamage.Apply(actor, 20, damageType: "Fire").HealthLost);
    }

    [Fact]
    public void UntypedDamageIgnoresGlobalNoneDefinition()
    {
        var sim = Room(); sim.DamageTypes.Define("None", 0);
        Assert.Equal(20, ActorDamage.Apply(sim.Players.Single(), 20).HealthLost);
    }

    [Fact]
    public void NoFactorBypassesGlobalImmunity()
    {
        var sim = Room(); sim.DamageTypes.Define("Fire", 0);
        var actor = sim.Players.Single();
        Assert.Equal(0, ActorDamage.Apply(actor, 20, damageType: "Fire").HealthLost);
        Assert.Equal(20, ActorDamage.Apply(actor, 20, flags: DamageFlags.NoFactor, damageType: "Fire").HealthLost);
    }

    [Fact]
    public void DefinitionsAreIsolatedBetweenSimulations()
    {
        var left = Room(); var right = Room(); left.DamageTypes.Define("Fire", 0);
        Assert.Equal(0, ActorDamage.Apply(left.Players.Single(), 20, damageType: "Fire").HealthLost);
        Assert.Equal(20, ActorDamage.Apply(right.Players.Single(), 20, damageType: "Fire").HealthLost);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
