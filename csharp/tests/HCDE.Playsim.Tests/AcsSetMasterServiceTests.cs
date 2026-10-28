using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSetMasterServiceTests
{
    [Fact]
    public void SetMaster_LinksMonsterToPlayer()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things =
            [
                new LevelThing { Type = 3001, X = 0, Y = 0 },
                new LevelThing { Type = 1, X = 32, Y = 0 },
            ],
        };
        var sim = AuthoritySimulation.Start(level);
        var monster = sim.Actors.First(a => a.IsMonster);
        var player = sim.Players.First();
        player.ThingId = 42;
        Assert.Equal(1, AcsSetMasterService.TrySet(sim, monster, 42));
        Assert.Equal(player.PlayerNum + 1, monster.FriendPlayer);
        Assert.True(monster.Friendly);
    }
}
