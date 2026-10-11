using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class EndLevelDamageSurvivalTests
{
    [Theory]
    [InlineData(100, DamageFlags.None)]
    [InlineData(100, DamageFlags.Forced)]
    [InlineData(ActorDamage.TelefragDamage, DamageFlags.None)]
    public void EndLevelSectorCapsPlayerDamageBeforeDeath(int damage, DamageFlags flags)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50;
        var result = ActorDamage.Apply(player, damage, flags: flags);
        Assert.Equal(49, result.HealthLost); Assert.Equal(1, player.Health);
        Assert.False(result.Killed); Assert.Equal(0, player.DeathCount);
    }

    [Fact]
    public void CapDoesNotProtectNonPlayers()
    {
        var sim = Room(); var target = sim.AddBot(10, 0); target.Health = 50;
        Assert.True(ActorDamage.Apply(target, 100).Killed);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512, DamageEndsLevel = true }],
        Things = [new LevelThing { Type = 1 }] });
}
