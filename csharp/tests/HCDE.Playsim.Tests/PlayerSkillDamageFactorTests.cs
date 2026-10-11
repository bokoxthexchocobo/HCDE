using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerSkillDamageFactorTests
{
    [Theory]
    [InlineData(1, false, false, 1)]
    [InlineData(7, false, false, 3)]
    [InlineData(7, true, false, 7)]
    [InlineData(ActorDamage.TelefragDamage, false, false, ActorDamage.TelefragDamage)]
    [InlineData(ActorDamage.TelefragDamage, false, true, ActorDamage.TelefragDamage / 2)]
    public void ScalingPrecedesSpecialDamageWithNativeExceptions(int damage, bool forced, bool lax, int expected)
    {
        var sim = Room(0.5); var player = sim.Players.Single(); player.Health = 2_000_000;
        player.LaxTelefragDamage = lax;
        var inflictor = new ObservingInflictor();
        var result = ActorDamage.Apply(player, damage, flags: forced ? DamageFlags.Forced : DamageFlags.None,
            inflictor: inflictor);
        Assert.Equal(expected, result.HealthLost);
        if (!forced && (damage < ActorDamage.TelefragDamage || lax)) Assert.Equal(expected, inflictor.Observed);
        else Assert.Null(inflictor.Observed);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-1)]
    public void InvalidConfigurationIsRejected(double factor) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Room(factor));

    private sealed class ObservingInflictor : Actor
    {
        public int? Observed { get; private set; }
        public override int DoSpecialDamage(Actor target, int damage, string? damageType, DamageFlags flags)
        { Observed = damage; return damage; }
    }

    private static AuthoritySimulation Room(double factor) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(PlayerDamageFactor: factor));
}
