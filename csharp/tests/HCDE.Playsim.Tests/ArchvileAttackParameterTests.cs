using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileAttackParameterTests
{
    [Theory]
    [InlineData(false, false, 0, 976)]
    [InlineData(true, false, 0, 1000)]
    [InlineData(false, true, 0, 976)]
    [InlineData(true, true, 0, 1000)]
    [InlineData(false, true, 0.5, 945)]
    [InlineData(true, true, 0.5, 957)]
    public void CustomTypeAndDirectFlagSelectDamageContext(bool typedDirect, bool fire, double factor, int expected)
    {
        var (sim, attacker, target) = Room();
        target.SetDamageFactor("Ice", factor); target.SetDamageFactor("Fire", 0);
        ArchvileActions.Attack(sim, attacker, target, fire, initialDamage: 24,
            damageType: "Ice", flags: typedDirect ? 1 : 0);
        Assert.Equal(expected, target.Health);
        Assert.Equal(10, target.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(-1, 100, -10)]
    [InlineData(0, 100, 0)]
    [InlineData(0.25, 100, 2.5)]
    [InlineData(2, 200, 10)]
    [InlineData(0.25, 0, 250)]
    [InlineData(0.25, -10, 250)]
    public void CustomThrustUsesSignedMultiplierAndClampedMass(double thrust, int mass, double expected)
    {
        var (sim, attacker, target) = Room(); target.Mass = mass;
        target.VelocityZ = Fixed.FromInt(17);
        ArchvileActions.Attack(sim, attacker, target, false, initialDamage: 35, thrust: thrust);
        Assert.Equal(965, target.Health);
        Assert.Equal(expected, target.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidThrustIsRejectedBeforeMutation(double thrust)
    {
        var (sim, attacker, target) = Room(); attacker.Ambush = true;
        Assert.Throws<ArgumentOutOfRangeException>(() => ArchvileActions.Attack(sim, attacker, target, true, thrust: thrust));
        Assert.Equal(1000, target.Health); Assert.True(attacker.Ambush);
        Assert.Equal(default, target.VelocityZ);
    }

    private static (AuthoritySimulation Sim, Actor Attacker, PlayerPawn Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1, X = 300 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: 42);
        var attacker = sim.AddBot(0, 0, 64); attacker.Brain = null;
        var target = sim.Players.Single(); target.Health = 1000; target.NoPain = true;
        return (sim, attacker, target);
    }
}
