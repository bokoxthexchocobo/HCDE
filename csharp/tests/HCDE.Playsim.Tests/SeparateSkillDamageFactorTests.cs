using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SeparateSkillDamageFactorTests
{
    [Theory]
    [InlineData(7, false, 7)]
    [InlineData(1, false, 1)]
    [InlineData(7, true, 7)]
    public void SkillAndServerMultiplyBeforeSingleTruncation(int damage, bool forced, int expected)
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.Equal(expected, ActorDamage.Apply(player, damage, flags: forced ? DamageFlags.Forced : DamageFlags.None).HealthLost);
    }

    [Fact]
    public void SkillFactorDoesNotScaleNonPlayerTargets()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0); actor.Brain = null;
        Assert.Equal(7, ActorDamage.Apply(actor, 7).HealthLost);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(-1)]
    public void InvalidSkillFactorIsRejected(double factor) => Assert.Throws<ArgumentOutOfRangeException>(() =>
        AuthoritySimulation.Start(new PlayLevel(), spawnOptions: new SpawnOptions(SkillDamageFactor: factor)));

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(SkillDamageFactor: 0.5, PlayerDamageFactor: 2));
}
