using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FriendshipBaselineRestoreTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BaselineRestoreUndoesLaterPoliciesAndPreservesSpawnFriendliness(bool friendly)
    {
        var sim = Room(); var actor = sim.Actors[1];
        actor.SpawnFriendly = actor.Friendly = friendly;
        var bytes = SimSavegame.Write(sim);
        actor.Friendly = !friendly; actor.FriendPlayer = 3;
        actor.TidToHate = 77; actor.NoHatePlayers = true;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(0, actor.FriendPlayer);
        Assert.Equal(0, actor.TidToHate); Assert.False(actor.NoHatePlayers);
        Assert.False(actor.HasFriendshipOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void BaselineRestoreRemovesLaterPlayerRetaliationBlock()
    {
        var sim = Room(); var actor = sim.Actors[1];
        var bytes = SimSavegame.Write(sim);
        actor.NoHatePlayers = true;
        SimSavegame.Apply(sim, bytes);
        var player = sim.Players.Single();
        ActorDamage.Apply(actor, 1, player, DamageFlags.NoPain);
        Assert.Equal(player.Id, actor.Brain!.TargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }],
    });
}
