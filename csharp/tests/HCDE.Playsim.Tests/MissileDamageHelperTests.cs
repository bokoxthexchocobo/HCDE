using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileDamageHelperTests
{
    [Theory]
    [InlineData(0, 2)]
    [InlineData(3, 2)]
    [InlineData(7, 1)]
    [InlineData(15, 0)]
    public void NativeMaskAndAddCalculation(int mask, int add)
    {
        for (var seed = 1; seed <= 16; seed++)
        {
            var (sim, missile, _) = Setup(seed); var (prediction, _, _) = Setup(seed);
            var random = mask == 0 ? 0 : (int)prediction.NextCombatRandom();
            Assert.Equal(((random & mask) + add) * 5, missile.GetMissileDamage(mask, add));
            Assert.Equal(prediction.CombatRandomState, sim.CombatRandomState);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RipperDiceUseTwoThroughFiveRegardlessOfStrife(bool strife)
    {
        var (sim, missile, target) = Setup(7); missile.StrifeDamage = strife;
        var (prediction, _, _) = Setup(7);
        var expected = 5 * (2 + (int)(prediction.NextCombatRandom() & 3));
        Assert.Equal(expected, missile.DoMissileDamage(target, ripper: true).HealthLost);
        Assert.Equal(1000 - expected, target.Health); Assert.False(missile.Destroyed);
        Assert.Equal(prediction.CombatRandomState, sim.CombatRandomState);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(9)]
    public void ExpressionBypassesRipperDice(int amount)
    {
        var (sim, missile, target) = Setup(7); target.Health = 20;
        var calls = 0; missile.DamageExpression = actor => { Assert.Same(missile, actor); calls++; return amount; };
        var random = sim.CombatRandomState;
        missile.DoMissileDamage(target, ripper: true);
        Assert.Equal(20 - amount, target.Health); Assert.Equal(1, calls);
        Assert.Equal(random, sim.CombatRandomState);
    }

    [Fact]
    public void ConstantAndExpressionPathsWorkWithoutSimulation()
    {
        var missile = new ProjectileActor(new Actor(), ProjectileKind.Plasma);
        Assert.Equal(10, missile.GetMissileDamage(0, 2));
        Assert.Throws<InvalidOperationException>(() => missile.GetMissileDamage(7, 1));
        missile.DamageExpression = _ => -7; Assert.Equal(-7, missile.GetMissileDamage(7, 1));
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup(int seed)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: seed);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        return (sim, sim.SpawnProjectile(owner, ProjectileKind.Plasma), target);
    }
}
