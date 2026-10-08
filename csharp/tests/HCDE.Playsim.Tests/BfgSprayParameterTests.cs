using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BfgSprayParameterTests
{
    [Fact]
    public void FixedDamageIgnoresRollCountAndConsumesOnlyTargetPainRoll()
    {
        var (sim, owner, target, missile) = Setup();
        var (prediction, _, _, _) = Setup(); prediction.NextCombatRandom();
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, damageCount: 100, fixedDamage: 25);
        Assert.Equal(975, target.Health);
        Assert.Equal(prediction.CombatRandomState, sim.CombatRandomState);
    }

    [Fact]
    public void ConfiguredDistanceLimitsTargeting()
    {
        var (sim, owner, target, missile) = Setup();
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, distance: 50, fixedDamage: 25);
        Assert.Equal(1000, target.Health);
    }

    [Fact]
    public void NonpositiveParametersResolveToNativeDefaults()
    {
        var (left, owner, target, missile) = Setup();
        var (right, otherOwner, otherTarget, otherMissile) = Setup();
        BfgSprayActions.Apply(left, missile, owner);
        BfgSprayActions.Apply(right, otherMissile, otherOwner, numRays: -1, damageCount: 0, spread: 0, distance: -1);
        Assert.True(target.Health < 1000);
        Assert.Equal(target.Health, otherTarget.Health);
        Assert.Equal(left.CombatRandomState, right.CombatRandomState);
    }

    [Fact]
    public void MissingOwnerSkipsSpray()
    {
        var (sim, owner, target, missile) = Setup();
        var random = sim.CombatRandomState;
        BfgSprayActions.Apply(sim, missile, null);
        Assert.Equal(1000, target.Health);
        Assert.Equal(random, sim.CombatRandomState);
    }

    private static (AuthoritySimulation, PlayerPawn, Actor, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1 }],
        }, rngSeed: 42);
        var owner = sim.Players.Single();
        var target = sim.AddBot(200, 0, 3001);
        target.Brain = null; target.Health = 1000; target.PainChance = 0;
        return (sim, owner, target, new Actor { Angle = BamAngle.FromDegrees(45) });
    }
}
