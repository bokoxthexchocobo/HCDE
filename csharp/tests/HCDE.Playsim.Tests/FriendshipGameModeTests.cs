using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FriendshipGameModeTests
{
    [Theory]
    [InlineData(SpawnGameMode.Single, true)]
    [InlineData(SpawnGameMode.Cooperative, true)]
    [InlineData(SpawnGameMode.Deathmatch, false)]
    public void DifferentOwnersAreFriendsOutsideDeathmatchAndMemoryUsesThatPolicy(SpawnGameMode mode, bool friends)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }] }, spawnOptions: new SpawnOptions(Mode: mode));
        var first = sim.AddBot(0, 0); var second = sim.AddBot(100, 0);
        first.Friendly = second.Friendly = true; first.FriendPlayer = 1; second.FriendPlayer = 2;
        Assert.Equal(friends, first.IsFriend(second)); Assert.Equal(friends, second.IsFriend(first));
        Assert.Equal(!friends, first.IsHostile(second));
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        first.ReactionTime = 0;
        first.Brain!.RestoreTargetMemory(new(second.Id, null, null));
        first.Brain.Tick(sim, first);
        Assert.Equal(friends ? null : (uint?)second.Id, first.Brain.TargetId);
    }
}
