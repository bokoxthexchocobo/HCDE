using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GeometryMissileDamageHelperTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    public void GeometryUsesNativeDiceAndOneRollPerPart(int part, bool strife)
    {
        for (var seed = 1; seed <= 16; seed++)
        {
            var (sim, missile) = Setup(seed); var (prediction, _) = Setup(seed);
            missile.StrifeDamage = strife;
            var expected = 5 * (1 + ((int)prediction.NextCombatRandom() & (strife ? 3 : 7)));
            Hit(sim, missile, part);
            Assert.Equal(1000 - expected, Health(sim, part));
            Assert.Equal(prediction.CombatRandomState, sim.CombatRandomState);
        }
    }

    [Theory]
    [InlineData(-1, -7)]
    [InlineData(-1, 0)]
    [InlineData(-1, 19)]
    [InlineData(0, -7)]
    [InlineData(0, 0)]
    [InlineData(0, 19)]
    [InlineData(1, -7)]
    [InlineData(1, 0)]
    [InlineData(1, 19)]
    public void GeometryEvaluatesExpressionOnceWithoutDice(int part, int amount)
    {
        var (sim, missile) = Setup(42); var calls = 0;
        missile.DamageExpression = actor => { Assert.Same(missile, actor); calls++; return amount; };
        var random = sim.CombatRandomState;
        Hit(sim, missile, part);
        Assert.Equal(1000 - Math.Max(0, amount), Health(sim, part));
        Assert.Equal(1, calls); Assert.Equal(random, sim.CombatRandomState);
    }

    private static void Hit(AuthoritySimulation sim, ProjectileActor missile, int part)
    {
        if (part < 0) GeometryProjectileImpact.Apply(sim, missile, sim.Level.Lines[0]);
        else GeometryProjectileImpact.ApplyPlane(sim, missile, 0, part);
    }

    private static int Health(AuthoritySimulation sim, int part) => part switch
    {
        -1 => sim.Level.Lines[0].Health,
        0 => sim.Level.Sectors[0].HealthFloor,
        _ => sim.Level.Sectors[0].HealthCeiling,
    };

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Setup(int seed)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 1000, HealthCeiling = 1000 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = 64, Y1 = 128, X2 = 64, Y2 = -128,
                SideFront = 0, SideBack = -1, Health = 1000 }],
        }, rngSeed: seed);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default;
        return (sim, missile);
    }
}
