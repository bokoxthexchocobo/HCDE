using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class OwnDeathSpecialTests
{
    [Theory]
    [InlineData(false, 0, false)]
    [InlineData(true, 0, true)]
    [InlineData(true, 128, false)]
    [InlineData(true, 129, true)]
    public void LevelPolicySelectsActivatorWithFlagOverrides(bool own, int activation, bool victimActs)
    {
        var sim = Room(own); var player = sim.Players.Single(); var victim = sim.Actors[1];
        victim.ActivationType = activation;
        player.VelocityX = Fixed.FromInt(8); victim.VelocityX = Fixed.FromInt(6);
        ActorDamage.Apply(victim, 1000, player);
        Assert.Equal(victimActs ? 8 : 0, player.VelocityX.ToDouble());
        Assert.Equal(victimActs ? 0 : 6, victim.VelocityX.ToDouble());
        Assert.Equal(own, sim.Level.ActivateOwnDeathSpecials);
    }

    [Fact]
    public void LevelPolicyAffectsChecksum()
    {
        var first = Room(false); var second = Room(true);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool own) => AuthoritySimulation.Start(new PlayLevel
    {
        ActivateOwnDeathSpecials = own, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20, Special = 19 }],
    });
}
