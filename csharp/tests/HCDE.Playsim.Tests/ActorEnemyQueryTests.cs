using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorEnemyQueryTests
{
    [Fact]
    public void QueryAppendsAndDeduplicatesActorsOverlappingSeveralCells()
    {
        var sim = Room(); var observer = sim.Actors[0]; var enemy = sim.Actors[1];
        enemy.Radius = Fixed.FromInt(100);
        var targets = new List<Actor> { observer };
        Assert.Equal(2, ActorEnemySearchActions.LookForEnemiesEx(observer, targets, 512, noPlayers: true));
        Assert.Equal(new[] { observer, enemy }, targets);
        Assert.Null(observer.TargetId);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(2, true)]
    public void DefaultRangeUsesFriendlySeeBlocks(int blocks, bool found)
    {
        var sim = Room(); var observer = sim.Actors[0]; observer.FriendlySeeBlocks = blocks;
        var targets = new List<Actor>();
        Assert.Equal(found ? 1 : 0, ActorEnemySearchActions.LookForEnemiesEx(observer, targets, noPlayers: true));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public void PlayerSelectionHonorsFieldOfViewAndExclusion(bool noPlayers, bool allAround, bool includesPlayer)
    {
        var sim = Room(); var targets = new List<Actor>();
        ActorEnemySearchActions.LookForEnemiesEx(sim.Actors[0], targets, 512, noPlayers, allAround);
        Assert.Equal(includesPlayer, targets.Contains(sim.Players.Single()));
        Assert.Contains(sim.Actors[1], targets);
    }

    [Fact]
    public void PlayerNotargetAndBlockedVisibilityAreSharedWithAcquisition()
    {
        var sim = Room(); var player = sim.Players.Single(); var observer = sim.Actors[0];
        foreach (var blocked in new[] { false, true })
        {
            player.NoTarget = !blocked; player.MVisBlocked = blocked;
            var targets = new List<Actor>();
            ActorEnemySearchActions.LookForEnemiesEx(observer, targets, 512, allAround: true);
            Assert.DoesNotContain(player, targets);
        }
    }

    [Fact]
    public void InvalidRangeAndMissingArrayFailExplicitly()
    {
        var observer = Room().Actors[0];
        Assert.Throws<ArgumentNullException>(() => ActorEnemySearchActions.LookForEnemiesEx(observer, null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorEnemySearchActions.LookForEnemiesEx(observer, new List<Actor>(), double.NaN));
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(-512, -512, 10, 10), []),
            Things = [new LevelThing { Type = 3004, X = 64, Y = 64, Friendly = true },
                new LevelThing { Type = 3001, X = 320, Y = 64 },
                new LevelThing { Type = 1, X = -100, Y = 64 }]
        });
        sim.Players.Single().Friendly = false;
        return sim;
    }
}
