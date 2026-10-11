using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NonPlayerDamageScalingTests
{
    [Theory]
    [InlineData(false, 7, false, false, 3)]
    [InlineData(true, 7, false, false, 14)]
    [InlineData(false, 1, false, false, 1)]
    [InlineData(true, 1, false, false, 1)]
    [InlineData(false, 7, true, false, 7)]
    [InlineData(true, 7, true, false, 7)]
    [InlineData(false, ActorDamage.TelefragDamage, false, false, ActorDamage.TelefragDamage)]
    [InlineData(false, ActorDamage.TelefragDamage, false, true, ActorDamage.TelefragDamage / 2)]
    public void TargetFriendlinessSelectsScalingWithNativeExceptions(bool friendly, int damage, bool forced, bool lax, int expected)
    {
        var sim = Room(); var target = sim.AddBot(100, 0); target.Brain = null;
        target.Health = 3_000_000; target.Friendly = friendly; target.LaxTelefragDamage = lax;
        Assert.Equal(expected, ActorDamage.Apply(target, damage, flags: forced ? DamageFlags.Forced : DamageFlags.None).HealthLost);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidFactorsAreRejected(bool friendly) => Assert.Throws<ArgumentOutOfRangeException>(() =>
        AuthoritySimulation.Start(new PlayLevel(), spawnOptions: friendly
            ? new SpawnOptions(FriendlyDamageFactor: double.NaN)
            : new SpawnOptions(MonsterDamageFactor: -1)));

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }] },
        spawnOptions: new SpawnOptions(MonsterDamageFactor: 0.5, FriendlyDamageFactor: 2));
}
