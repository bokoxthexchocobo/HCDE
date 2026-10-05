using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ForcedAttackerEligibilityTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void RawTelefragBypassesSameSpeciesProtection(int infighting)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        sim.Infighting = infighting;
        var source = sim.AddBot(64, 0); var target = sim.AddBot(128, 0);
        Assert.True(ActorDamage.Apply(target, ActorDamage.TelefragDamage, source).Killed);
    }

    [Fact]
    public void RawTelefragBypassesFriendlyProtection()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var source = sim.AddBot(64, 0); var target = sim.AddBot(128, 0);
        source.Friendly = target.Friendly = true;
        Assert.True(ActorDamage.Apply(target, ActorDamage.TelefragDamage, source).Killed);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ForcedDamageBypassesSameSpeciesInfightingProtection(int infighting)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        sim.Infighting = infighting;
        var source = sim.AddBot(64, 0); var target = sim.AddBot(128, 0);
        Assert.Equal(0, ActorDamage.Apply(target, 10, source, DamageFlags.NoPain).HealthLost);
        Assert.Equal(10, ActorDamage.Apply(target, 10, source, DamageFlags.Forced | DamageFlags.NoPain).HealthLost);
    }

    [Fact]
    public void ForcedDamageBypassesFriendlyMonsterProtection()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var source = sim.AddBot(64, 0); var target = sim.AddBot(128, 0);
        source.Friendly = target.Friendly = true;
        Assert.Equal(0, ActorDamage.Apply(target, 10, source, DamageFlags.NoPain).HealthLost);
        Assert.Equal(10, ActorDamage.Apply(target, 10, source, DamageFlags.Forced | DamageFlags.NoPain).HealthLost);
    }
}
