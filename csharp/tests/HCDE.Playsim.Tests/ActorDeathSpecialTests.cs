using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDeathSpecialTests
{
    [Fact]
    public void DeathClearPolicyAffectsChecksumBeforeAnyDeath()
    {
        var modern = Room(false); var original = Room(true);
        Assert.NotEqual(modern.Checksum, original.Checksum);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LethalDamageRunsKillerSpecialWithNativeClearRule(bool hexenHack)
    {
        var sim = Room(hexenHack); var player = sim.Players.Single(); var victim = sim.Actors[1];
        player.VelocityX = Fixed.FromInt(8);
        ActorDamage.Apply(victim, 1000, player);
        Assert.Equal(0, player.VelocityX.Raw);
        Assert.Equal(hexenHack ? 19 : 0, victim.Special);
        player.VelocityX = Fixed.FromInt(8);
        ActorDamage.Apply(victim, 1000, player);
        Assert.Equal(Fixed.FromInt(8), player.VelocityX);
        victim.Health = victim.ResurrectionHealth;
        ActorDamage.Apply(victim, 1000, player);
        Assert.Equal(hexenHack ? 0 : 8, player.VelocityX.ToDouble());
    }

    [Fact]
    public void SurvivingDamageDoesNotExecuteSpecial()
    {
        var sim = Room(false); var player = sim.Players.Single();
        player.VelocityX = Fixed.FromInt(8);
        ActorDamage.Apply(sim.Actors[1], 1, player);
        Assert.Equal(Fixed.FromInt(8), player.VelocityX);
        Assert.Equal(19, sim.Actors[1].Special);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullKillerStillRunsTidTargetAndConsumesSpecial(bool hasTarget)
    {
        var sim = Room(false); var victim = sim.Actors[1];
        victim.SpecialArgs[0] = hasTarget ? 7 : 99;
        var target = sim.AddBot(200, 0); target.ThingId = 7;
        target.VelocityX = Fixed.FromInt(8);
        ActorDamage.Apply(victim, 1000);
        Assert.Equal(hasTarget ? 0 : 8, target.VelocityX.ToDouble());
        Assert.Equal(0, victim.Special);
    }

    [Fact]
    public void NonMonsterPickupDoesNotRunDeathSpecial()
    {
        var sim = Room(false); var victim = sim.Actors[1]; var player = sim.Players.Single();
        victim.SpecialPickup = true; victim.IsMonster = false;
        player.VelocityX = Fixed.FromInt(8);
        ActorDamage.Apply(victim, 1000, player);
        Assert.Equal(Fixed.FromInt(8), player.VelocityX);
        Assert.Equal(19, victim.Special);
    }

    private static AuthoritySimulation Room(bool hexenHack) => AuthoritySimulation.Start(new PlayLevel
    {
        HexenHack = hexenHack, Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20, Special = 19 }],
    });
}
