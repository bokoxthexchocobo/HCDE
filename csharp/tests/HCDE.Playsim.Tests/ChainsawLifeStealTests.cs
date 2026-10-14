using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChainsawLifeStealTests
{
    [Theory]
    [InlineData(0.5, 0, 50, 55)]
    [InlineData(0.25, 0, 50, 52)]
    [InlineData(2, 60, 50, 60)]
    [InlineData(1, 0, 98, 100)]
    [InlineData(0, 0, 50, 50)]
    public void HealthStealTruncatesAndHonorsMaximum(double strength, int maximum, int health, int expected)
    {
        var (_, player, _) = Room(); player.Health = health;
        Attack(player, strength, maximum); Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void ArmorReducesStolenHealth()
    {
        var (_, player, target) = Room(); target.Armor = 100; target.ArmorSavePercent = 50;
        Attack(player, 1); Assert.Equal(55, player.Health);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void DrainImmuneInvulnerableAndMissedTargetsDoNotHeal(bool dontDrain, bool invulnerable, bool miss)
    {
        var (_, player, target) = Room(); target.DontDrain = dontDrain; target.Invulnerable = invulnerable;
        if (miss) target.X = Fixed.FromInt(500);
        Attack(player, 1); Assert.Equal(50, player.Health);
    }

    [Fact]
    public void NonFiniteStrengthFailsBeforeAttack()
    {
        var (sim, player, _) = Room(); var before = SimSavegame.Write(sim, true);
        Assert.Throws<ArgumentOutOfRangeException>(() => Attack(player, double.NaN));
        Assert.Equal(before, SimSavegame.Write(sim, true)); Assert.Empty(sim.DrainSoundRequests());
    }

    private static void Attack(PlayerPawn player, double strength, int maximum = 0)
        => ChainsawActions.Attack(player, 10, spread: 0, flags: ChainsawAttackFlags.NoRandom,
            lifeSteal: strength, lifeStealMax: maximum);

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 48 }]
        });
        var player = sim.Players.Single(); player.Health = 50;
        sim.Actors[1].Brain!.Enabled = false; sim.DrainSoundRequests();
        return (sim, player, sim.Actors[1]);
    }
}
