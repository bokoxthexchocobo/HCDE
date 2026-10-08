using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileBlastFalloffTests
{
    [Theory]
    [InlineData(60, 60, 0, 26)]
    [InlineData(-60, -60, 0, 26)]
    [InlineData(60, 0, 48, 4)]
    [InlineData(0, 0, 100, 0)]
    public void BlastUsesSquareHorizontalDistanceAndTruncatedScaledDamage(int x, int y, int z, int damage)
    {
        var (sim, attacker, target) = Room();
        var victim = sim.AddBot(276 + x, y, 3001); victim.Brain = null;
        victim.Health = 1000; victim.Radius = Fixed.FromInt(16); victim.Z = Fixed.FromInt(z);
        victim.NoPain = true;
        ArchvileActions.Attack(sim, attacker, target, true);
        Assert.Equal(1000 - damage, victim.Health);
        Assert.Equal(918, target.Health);
    }

    [Theory]
    [InlineData(100, 50, 84)]
    [InlineData(25, 100, 23)]
    [InlineData(70, 8, 0)]
    [InlineData(70, 0, 62)]
    [InlineData(70, -1, 62)]
    [InlineData(0, 70, 0)]
    public void BlastDamageAndRadiusAreIndependentOfDirectDamage(int damage, int radius, int expectedBlast)
    {
        var (sim, attacker, target) = Room();
        ArchvileActions.Attack(sim, attacker, target, true, blastDamage: damage, blastRadius: radius);
        Assert.Equal(980 - expectedBlast, target.Health);
        Assert.Equal(10, target.VelocityZ.ToDouble());
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
