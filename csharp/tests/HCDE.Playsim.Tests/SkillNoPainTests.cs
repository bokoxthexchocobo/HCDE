using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkillNoPainTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SkillSuppressesNonPlayerPainButPreservesWakeup(bool playerTarget, bool forcedPain)
    {
        var sim = Room(true); var source = sim.Players.Single();
        Actor target = playerTarget ? source : sim.AddBot(100, 0);
        target.PainChance = 256; target.ReactionTime = 12;
        var state = target.States.Current;
        var random = sim.CombatRandomState;
        ActorDamage.Apply(target, 1, source, inflictor: new Actor { ForcePain = forcedPain });
        Assert.Equal(playerTarget ? target.PainState : state, target.States.Current);
        if (!playerTarget)
        {
            Assert.Equal(0, target.ReactionTime);
            Assert.Equal(source.Id, target.Brain!.TargetId);
            Assert.Equal(random, sim.CombatRandomState);
        }
    }

    [Fact]
    public void DirectPainActionStillWorksAndCloudSuppressionDoesNotDrawPoison()
    {
        var sim = Room(true); var actor = sim.AddBot(100, 0); actor.Brain = null;
        ActorDamage.Apply(actor, 1, damageType: "PoisonCloud", inflictor: new Actor { ForcePain = true });
        Assert.Null(sim.CaptureState().PoisonDamageRandomState);
        Assert.True(actor.TriggerPainChance("PoisonCloud", true));
        Assert.NotNull(sim.CaptureState().PoisonDamageRandomState);
    }

    private static AuthoritySimulation Room(bool noPain) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(NoMonsterPain: noPain));
}
