using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TriggerPainChanceActionTests
{
    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public void ForcedActionReportsAnimationWithoutDamageOrRetaliation(bool noPain, bool missing, bool expected)
    {
        var actor = new Actor { Health = 100, PainChance = 0, PainThreshold = 1000,
            NoPain = noPain, PainState = missing ? -1 : 1, ReactionTime = 12, Invulnerable = true };
        Assert.Equal(expected, actor.TriggerPainChance(forcedPain: true));
        Assert.Equal(100, actor.Health); Assert.Equal(12, actor.ReactionTime);
        Assert.False(actor.JustHit); Assert.Null(actor.LastDamageSourceId);
    }

    [Fact]
    public void DeadActorDoesNotRollOrEnterPain()
    {
        var sim = Room(42); var actor = sim.AddBot(100, 0); actor.Health = 0;
        var random = sim.CombatRandomState; var state = actor.States.Current;
        Assert.False(actor.TriggerPainChance(forcedPain: true));
        Assert.Equal(state, actor.States.Current); Assert.Equal(random, sim.CombatRandomState);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(256)]
    public void TypedChanceUsesActionRollAndDoesNotWakeActor(int chance)
    {
        var sim = Room(42); var prediction = Room(42);
        var actor = sim.AddBot(100, 0); actor.Brain = null;
        actor.PainChance = chance == 0 ? 256 : 0; actor.ReactionTime = 12;
        actor.SetTypedPain("Ice", 1, chance);
        prediction.NextCombatRandom();
        Assert.Equal(chance == 256, actor.TriggerPainChance("ice"));
        Assert.Equal(prediction.CombatRandomState, sim.CombatRandomState);
        Assert.False(actor.JustHit); Assert.Equal(12, actor.ReactionTime);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ElectricReturnMeansAnimationRatherThanHitReaction(bool missing)
    {
        var flinches = 0; var shines = 0;
        for (var seed = 0; seed < 64; seed++)
        {
            var sim = Room(seed * 7919); var prediction = Room(seed * 7919);
            var actor = sim.AddBot(100, 0); actor.Brain = null; actor.PainState = missing ? -1 : 1;
            var reacted = prediction.NextFlickerRandom() < 96;
            if (!reacted) prediction.NextFlickerRandom();
            var combat = sim.CombatRandomState;
            Assert.Equal(reacted && !missing, actor.TriggerPainChance("Electric", true));
            Assert.Equal(!reacted, actor.FullBright); Assert.False(actor.JustHit);
            Assert.Equal(combat, sim.CombatRandomState);
            Assert.Equal(prediction.NextFlickerRandom(), sim.NextFlickerRandom());
            if (reacted) flinches++; else shines++;
        }
        Assert.True(flinches > 0); Assert.True(shines > 0);
    }

    private static AuthoritySimulation Room(int seed) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: seed);
}
