using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterRaiseReactionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(123)]
    [InlineData(-7)]
    public void RaiseDurationDoesNotReplaceOrDecrementReactionCounter(int reaction)
    {
        var sim = Room(); var actor = Monster(sim); actor.ReactionTime = reaction;
        actor.RaiseDuration = 3; actor.Brain!.Revive(actor, sim.Players.Single());
        Assert.Equal(3, actor.Brain.RaiseTics); Assert.Equal(reaction, actor.ReactionTime);
        for (var remaining = 2; remaining >= 0; remaining--)
        {
            actor.Brain.Tick(sim, actor); Assert.Equal(remaining, actor.Brain.RaiseTics);
            Assert.Equal(reaction, actor.ReactionTime); Assert.Equal(MonsterMode.Raise, actor.Brain.Mode);
        }
        actor.Brain.Tick(sim, actor);
        Assert.Equal(reaction > 0 ? reaction - 1 : reaction, actor.ReactionTime);
    }

    [Fact]
    public void ClearingReactionDuringRaiseDoesNotCancelAnimationWait()
    {
        var sim = Room(); var actor = Monster(sim); actor.RaiseDuration = 3;
        actor.Brain!.Revive(actor, sim.Players.Single()); actor.ReactionTime = 0;
        actor.Brain.Tick(sim, actor);
        Assert.Equal(2, actor.Brain.RaiseTics); Assert.Equal(MonsterMode.Raise, actor.Brain.Mode);
        Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void DeathClearsRaiseWaitWithoutReplacingReactionCounter()
    {
        var sim = Room(); var actor = Monster(sim); actor.RaiseDuration = 3; actor.ReactionTime = 123;
        actor.Brain!.Revive(actor, sim.Players.Single()); actor.Health = 0; actor.Brain.Tick(sim, actor);
        Assert.Equal(0, actor.Brain.RaiseTics); Assert.Equal(123, actor.ReactionTime);
        Assert.Equal(MonsterMode.Dead, actor.Brain.Mode);
    }

    [Fact]
    public void RaiseWaitParticipatesInBrainChecksum()
    {
        var first = Room(); var second = Room(); var left = Monster(first); var right = Monster(second);
        left.RaiseDuration = 3; right.RaiseDuration = 4;
        left.Brain!.Revive(left, first.Players.Single()); right.Brain!.Revive(right, second.Players.Single());
        Assert.NotEqual(left.Brain.StateChecksum, right.Brain.StateChecksum);
        Assert.Equal(left.ReactionTime, right.ReactionTime);
    }

    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.DoomEdNum == 3002);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 3002 }, new LevelThing { Type = 1, X = 50 }],
    });
}
