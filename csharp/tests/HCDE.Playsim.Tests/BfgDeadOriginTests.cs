using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BfgDeadOriginTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SprayCanTraceFromDeadOwnerOrMissile(bool missileOrigin)
    {
        var (sim, owner, target) = Setup();
        var missile = new Actor { Angle = BamAngle.FromDegrees(45), Health = 0 };
        owner.Health = 0;
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, fixedDamage: 25,
            flags: missileOrigin ? BfgSprayFlags.MissileOrigin : BfgSprayFlags.None);
        Assert.Equal(75, target.Health);
        Assert.Equal(owner.Id, target.LastDamageSourceId);
        Assert.Equal(0, owner.Health);
    }

    [Fact]
    public void OrdinaryCombatTraceStillRejectsDeadOrigin()
    {
        var (sim, owner, target) = Setup();
        owner.Health = 0;
        Assert.Null(CombatTrace.FindTarget(sim, owner, 1024));
        Assert.Null(CombatTrace.TraceLineAttack(sim, owner, default, default, 1024).Victim);
        Assert.Equal(100, target.Health);
    }

    private static (AuthoritySimulation, PlayerPawn, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var target = sim.AddBot(200, 0, 3001);
        target.Brain = null; target.Health = 100; target.PainChance = 0;
        return (sim, sim.Players.Single(), target);
    }
}
