using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeadPlayerDamageFadeTests
{
    [Theory]
    [InlineData(0, 99)]
    [InlineData(9, 99)]
    [InlineData(10.01, 100)]
    [InlineData(90, 100)]
    public void FadeChecksKillerAngleBeforeTurning(double angle, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); var attacker = sim.AddBot(100, 0);
        ActorDamage.Apply(player, 200, attacker, DamageFlags.Forced);
        player.Angle = BamAngle.FromDegrees(angle); player.Tick();
        Assert.Equal(expected, player.DamageCount); Assert.True(player.IsDead);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingAndSelfAttackersFadeEveryDeathTic(bool self)
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorDamage.Apply(player, 200, self ? player : null, DamageFlags.Forced);
        player.Tick(); Assert.Equal(99, player.DamageCount);
        player.Tick(); Assert.Equal(98, player.DamageCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }] });
}
