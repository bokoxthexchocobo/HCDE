using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ResurrectionFriendshipTests
{
    [Theory]
    [InlineData(false, true, 2, 42, true)]
    [InlineData(true, false, 0, 0, false)]
    [InlineData(true, true, 1, 77, false)]
    public void RevivedActorCopiesSupportedFriendshipAndHateFields(bool oldFriendly, bool friendly, int friendPlayer, int hateTid, bool noHatePlayers)
    {
        var sim = Room(); var vile = sim.Actors.Single(actor => actor.DoomEdNum == 64);
        var corpse = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        vile.Friendly = friendly; vile.FriendPlayer = friendPlayer; vile.TidToHate = hateTid; vile.NoHatePlayers = noHatePlayers;
        corpse.Friendly = oldFriendly; corpse.FriendPlayer = 99; corpse.TidToHate = 999; corpse.NoHatePlayers = !noHatePlayers;
        corpse.Brain!.SetTargetThingId(sim, 3); corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        Assert.True(ArchvileActions.TryRaise(sim, vile, sim.Players.Single()));
        Assert.Equal(friendly, corpse.Friendly); Assert.Equal(friendPlayer, corpse.FriendPlayer);
        Assert.Equal(hateTid, corpse.TidToHate); Assert.Equal(noHatePlayers, corpse.NoHatePlayers);
        Assert.Null(corpse.Brain.TargetId); Assert.Null(corpse.Brain.LastEnemyId);
        Assert.Equal(MonsterMode.Raise, corpse.Brain.Mode);
    }

    [Fact]
    public void RaiseWaitDoesNotAcquireTheArchvilesTarget()
    {
        var sim = Room(); var actor = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        actor.ReactionTime = 0; actor.RaiseDuration = 3; actor.Brain!.SetTargetThingId(sim, 3);
        actor.Brain.Revive(actor);
        for (var i = 0; i < 3; i++) { actor.Brain.Tick(sim, actor); Assert.Null(actor.Brain.TargetId); }
        actor.Brain.Tick(sim, actor); Assert.Equal(sim.Players.Single().Id, actor.Brain.TargetId);
    }

    [Fact]
    public void ReviveDoesNotClearTheIndependentReactionCounter()
    {
        var sim = Room(); var actor = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        actor.ReactionTime = 123; actor.Brain!.SetTargetThingId(sim, 3); actor.Brain.Revive(actor);
        Assert.Equal(123, actor.ReactionTime); Assert.Null(actor.Brain.TargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Sides = [new LevelSide { Sector = 0 }],
        Things = [new LevelThing { Type = 64 }, new LevelThing { Type = 3001, X = 50 },
            new LevelThing { Type = 1, Id = 3, X = 400 }],
    });
}
