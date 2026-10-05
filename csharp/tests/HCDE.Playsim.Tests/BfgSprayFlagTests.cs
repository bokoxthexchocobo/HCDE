using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BfgSprayFlagTests
{
    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, 75)]
    public void MissileOriginChangesTracePositionAndSupportsRemovedMissile(bool missileOrigin, int health)
    {
        var (sim, owner) = Room();
        var target = sim.AddBot(200, 100, 3001);
        target.Brain = null;
        target.Health = 100; target.PainChance = 0;
        var missile = new Actor { Y = Fixed.FromInt(100), Angle = BamAngle.FromDegrees(45) };
        missile.Destroy();
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, fixedDamage: 25,
            flags: missileOrigin ? BfgSprayFlags.MissileOrigin : BfgSprayFlags.None);
        Assert.Equal(health, target.Health);
    }

    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, 75)]
    public void MissileOriginHurtsOwnerOnlyWhenRequested(bool hurtSource, int health)
    {
        var (sim, owner) = Room();
        var missile = new Actor { X = Fixed.FromInt(-200), Height = Fixed.FromInt(8), Angle = BamAngle.FromDegrees(45) };
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, fixedDamage: 25,
            flags: BfgSprayFlags.MissileOrigin | (hurtSource ? BfgSprayFlags.HurtSource : BfgSprayFlags.None));
        Assert.Equal(health, owner.Health);
    }

    private static (AuthoritySimulation, PlayerPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1 }],
        });
        return (sim, sim.Players.Single());
    }
}
