using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissingPainStateReactionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void ForcedOrdinaryPainMarksJustHitWithoutAnAnimation(int damage)
    {
        var target = new Actor { Health = 100, PainState = -1, PainChance = 0, ReactionTime = 12 };
        var state = target.States.Current;
        var source = new Actor();
        ActorDamage.Apply(target, damage, source, inflictor: new Actor { ForcePain = true });
        Assert.True(target.JustHit); Assert.Equal(state, target.States.Current);
        Assert.Equal(100 - damage, target.Health); Assert.Equal(0, target.ReactionTime);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForcedElectricPainRollsAndMarksHitEvenWithoutAnimation(bool monster)
    {
        for (var seed = 0; seed < 64; seed++)
        {
            var sim = Room(seed * 7919); var prediction = Room(seed * 7919);
            var target = sim.AddBot(100, 0); target.Brain = null;
            target.PainState = -1; target.PainChance = 0; target.IsMonster = monster;
            var state = target.States.Current; var health = target.Health;
            var reacted = prediction.NextFlickerRandom() < 96;
            if (!reacted && monster) prediction.NextFlickerRandom();
            var combat = sim.CombatRandomState;
            ActorDamage.Apply(target, 0, new Actor(), damageType: "Electric", inflictor: new Actor { ForcePain = true });
            Assert.Equal(reacted, target.JustHit); Assert.Equal(!reacted, target.FullBright);
            Assert.Equal(state, target.States.Current); Assert.Equal(health, target.Health);
            Assert.Equal(combat, sim.CombatRandomState);
            Assert.Equal(prediction.NextFlickerRandom(), sim.NextFlickerRandom());
        }
    }

    [Fact]
    public void NoPainStillPreventsHitReactionWithoutAnimation()
    {
        var target = new Actor { PainState = -1, NoPain = true };
        ActorDamage.Apply(target, 0, new Actor(), inflictor: new Actor { ForcePain = true });
        Assert.False(target.JustHit); Assert.False(target.FullBright);
    }

    private static AuthoritySimulation Room(int seed) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: seed);
}
