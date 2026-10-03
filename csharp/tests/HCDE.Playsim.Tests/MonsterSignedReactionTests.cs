using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterSignedReactionTests
{
    [Theory]
    [InlineData(-1, -2)]
    [InlineData(-5, -6)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void NegativeReactionCountsDownWithoutAcquiringTarget(int initial, int expected)
    {
        var (sim, monster) = Room();
        monster.ReactionTime = initial;
        monster.Brain!.Tick(sim, monster);
        Assert.Equal(expected, monster.ReactionTime);
        Assert.Equal(expected, monster.Brain.ReactionTics);
        Assert.Null(monster.Brain.TargetId);
    }

    [Fact]
    public void ClearingNegativeReactionAllowsManagedTargetAcquisition()
    {
        var (sim, monster) = Room();
        monster.ReactionTime = -5;
        monster.Brain!.Tick(sim, monster);
        Assert.Null(monster.Brain.TargetId);
        monster.ReactionTime = 0;
        monster.Brain.Tick(sim, monster);
        Assert.Equal(sim.Players.Single().Id, monster.Brain.TargetId);
    }

    private static (AuthoritySimulation Sim, Actor Monster) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 400 }, new LevelThing { Type = 3004 }],
        });
        return (sim, sim.Actors.Single(actor => actor.DoomEdNum == 3004));
    }
}
