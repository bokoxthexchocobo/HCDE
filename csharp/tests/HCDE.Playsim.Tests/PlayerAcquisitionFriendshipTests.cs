using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerAcquisitionFriendshipTests
{
    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(1, 0, false)]
    [InlineData(0, 2, false)]
    [InlineData(1, 1, false)]
    [InlineData(1, 2, true)]
    public void VisualAcquisitionUsesExistingFriendshipPredicate(int ownerPlayer, int targetPlayer, bool expectedTarget)
    {
        var sim = Room();
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        var player = sim.Players.Single();
        monster.Friendly = player.Friendly = true;
        monster.FriendPlayer = ownerPlayer; player.FriendPlayer = targetPlayer;
        monster.Brain!.Tick(sim, monster);
        Assert.Equal(expectedTarget ? (uint?)player.Id : null, monster.Brain.TargetId);
        Assert.Equal(expectedTarget ? MonsterMode.Windup : MonsterMode.Idle, monster.Brain.Mode);
    }

    [Fact]
    public void VisualAcquisitionSkipsNearestFriendAndChoosesFartherEnemy()
    {
        var sim = Room(twoPlayers: true);
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        var players = sim.Players.OrderBy(player => player.X.Raw).ToArray();
        monster.Friendly = players[0].Friendly = true;
        monster.Brain!.Tick(sim, monster);
        Assert.Equal(players[1].Id, monster.Brain.TargetId);
    }

    [Fact]
    public void HeardFriendCannotBeReacquiredThroughVisualFallback()
    {
        var sim = Room();
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        var player = sim.Players.Single();
        monster.Friendly = player.Friendly = true;
        SoundPropagation.Alert(sim, player);
        Assert.Equal(player.Id, monster.LastHeardTargetId);
        for (var i = 0; i < 15; i++) monster.Brain!.Tick(sim, monster);
        Assert.Null(monster.Brain!.TargetId);
        Assert.Equal(MonsterMode.Idle, monster.Brain.Mode);
    }

    [Fact]
    public void PlayerBecomingFriendIsDroppedWithoutVisualReacquisition()
    {
        var sim = Room();
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        var player = sim.Players.Single();
        monster.Brain!.Tick(sim, monster);
        Assert.Equal(player.Id, monster.Brain.TargetId);
        monster.Friendly = player.Friendly = true;
        for (var i = 0; i < 15; i++) monster.Brain.Tick(sim, monster);
        Assert.Null(monster.Brain.TargetId);
        Assert.Equal(100, player.Health);
    }

    private static AuthoritySimulation Room(bool twoPlayers = false)
    {
        var things = new List<LevelThing> { new() { Type = 1, X = 128 }, new() { Type = 3004 } };
        if (twoPlayers) things.Add(new() { Type = 2, X = 256 });
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = things,
        });
        sim.Actors.Single(actor => actor.DoomEdNum == 3004).ReactionTime = 0;
        return sim;
    }
}
