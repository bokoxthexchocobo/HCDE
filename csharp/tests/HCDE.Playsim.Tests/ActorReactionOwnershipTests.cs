using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorReactionOwnershipTests
{
    [Fact]
    public void RemovingBrainKeepsLiveCountdownAndReattachingKeepsActorState()
    {
        var sim = Room(); var actor = Monster(sim); var brain = actor.Brain!;
        actor.ReactionTime = 3; sim.Tick(); Assert.Equal(2, actor.ReactionTime);
        actor.Brain = null; Assert.Equal(2, actor.ReactionTime); Assert.Equal(2, brain.ReactionTics);
        actor.ReactionTime = 7; actor.Brain = brain; Assert.Equal(7, brain.ReactionTics);
        sim.Tick(); Assert.Equal(6, actor.ReactionTime);
    }

    [Fact]
    public void ReplacementBrainUsesActorCounterInsteadOfItsDefault()
    {
        var sim = Room(); var actor = Monster(sim); actor.ReactionTime = 123;
        actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        Assert.Equal(123, actor.ReactionTime); Assert.Equal(123, actor.Brain.ReactionTics);
        sim.Tick(); Assert.Equal(122, actor.ReactionTime);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-7)]
    [InlineData(123)]
    public void ExplicitCounterBeforeFirstBrainAttachmentIsPreserved(int value)
    {
        var actor = new Actor { ReactionTime = value, Brain = new MonsterBrain(MonsterAttack.Melee) };
        Assert.Equal(value, actor.ReactionTime); Assert.Equal(value, actor.Brain.ReactionTics);
    }

    [Fact]
    public void FirstAttachmentRetainsExistingManagedDefault()
    {
        var actor = new Actor { Brain = new MonsterBrain(MonsterAttack.Melee) };
        Assert.Equal(10, actor.ReactionTime); Assert.Equal(10, actor.Brain.ReactionTics);
    }

    [Fact]
    public void SharingBrainIsRejectedWithoutDetachingEitherExistingBrain()
    {
        var first = new Actor { Brain = new MonsterBrain(MonsterAttack.Melee) };
        var second = new Actor { Brain = new MonsterBrain(MonsterAttack.Melee), ReactionTime = 123 };
        var previous = second.Brain;
        Assert.Throws<InvalidOperationException>(() => second.Brain = first.Brain);
        Assert.Same(previous, second.Brain); Assert.Equal(123, second.ReactionTime);
        Assert.Equal(10, first.ReactionTime);
    }

    [Fact]
    public void DetachedBrainCannotChangeItsFormerActorsCounter()
    {
        var actor = new Actor { Brain = new MonsterBrain(MonsterAttack.Melee), ReactionTime = 3 };
        var brain = actor.Brain!; actor.Brain = null; brain.SetReactionTime(99);
        Assert.Equal(3, actor.ReactionTime); Assert.Equal(99, brain.ReactionTics);
    }

    [Fact]
    public void ExplicitZeroInitializationParticipatesInChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().ReactionTime = 0;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.DoomEdNum == 3002);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 3002 }, new LevelThing { Type = 1, X = 128 }],
    });
}
