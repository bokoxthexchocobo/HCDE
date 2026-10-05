using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileDamageExpressionTests
{
    [Theory]
    [InlineData(-10, 30)]
    [InlineData(-100, 60)]
    [InlineData(0, 20)]
    [InlineData(7, 13)]
    public void ExpressionResultDamagesOrHealsWithoutDice(int amount, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 20;
        target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        var calls = 0;
        missile.DamageExpression = actor => { Assert.Same(missile, actor); calls++; return amount; };
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        var random = sim.CombatRandomState;
        missile.Tick();
        Assert.True(missile.Destroyed); Assert.Equal(1, calls);
        Assert.Equal(expected, target.Health);
        Assert.Equal(random, sim.CombatRandomState);
    }
}
