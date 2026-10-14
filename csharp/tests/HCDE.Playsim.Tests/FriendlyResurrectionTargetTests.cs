using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FriendlyResurrectionTargetTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void FriendlyRaiserClearsOnlyTheResurrectedTargetBeforeHealAction(bool brain, bool friendly, bool targetsCorpse)
    {
        var sim = Room(); var player = sim.Players.Single(); var corpse = sim.Actors.Single(a => a.IsMonster);
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        var raiser = new Actor { Simulation = sim, Friendly = friendly,
            Brain = brain ? new MonsterBrain(MonsterAttack.Hitscan) : null, HealState = 1 };
        var original = targetsCorpse ? corpse : player; raiser.SetTargetPointer(original);
        uint? seen = null;
        raiser.States.Configure(raiser, [new(-1, 0), new(-1, 1, _ => seen = raiser.TargetId)], 0);
        Assert.True(ArchvileActions.TryRaise(sim, raiser, player));
        var expected = friendly && targetsCorpse ? (uint?)null : original.Id;
        Assert.Equal(expected, seen); Assert.Equal(expected, raiser.TargetId);
    }

    [Fact]
    public void StaleStoredFallbackCannotReappearAfterBrainTargetIsCleared()
    {
        var sim = Room(); var corpse = sim.Actors.Single(a => a.IsMonster);
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        var raiser = new Actor { Simulation = sim, Friendly = true, Brain = new MonsterBrain(MonsterAttack.Hitscan) };
        raiser.SetTargetPointer(corpse); raiser.StoredTargetId = corpse.Id;
        Assert.True(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single()));
        Assert.Null(raiser.TargetId); Assert.Null(raiser.StoredTargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }]
    });
}
