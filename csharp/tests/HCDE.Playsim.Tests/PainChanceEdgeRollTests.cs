using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PainChanceEdgeRollTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(128)]
    [InlineData(255)]
    [InlineData(256)]
    [InlineData(1000)]
    public void EligiblePainAlwaysRollsIncludingImpossibleAndGuaranteedChances(int chance)
    {
        var sim = Room(); var prediction = Room();
        var target = sim.AddBot(100, 0); target.Brain = null; target.PainChance = chance;
        var initial = target.States.Current; var health = target.Health;
        var succeeds = prediction.NextCombatRandom() % 256 < chance;
        ActorDamage.Apply(target, 1);
        Assert.Equal(health - 1, target.Health);
        Assert.Equal(succeeds ? target.PainState : initial, target.States.Current);
        Assert.Equal(prediction.CombatRandomState, sim.CombatRandomState);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void SuppressedOrForcedPainDoesNotConsumeOrdinaryRoll(int mode)
    {
        var sim = Room(); var target = sim.AddBot(100, 0); target.Brain = null;
        target.NoPain = mode == 0; target.PainThreshold = mode == 1 ? 100 : 0;
        var random = sim.CombatRandomState;
        ActorDamage.Apply(target, 1, inflictor: mode == 2 ? new Actor { ForcePain = true } : null);
        Assert.Equal(random, sim.CombatRandomState);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: 42);
}
