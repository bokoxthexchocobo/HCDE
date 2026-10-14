using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsMasterTidQueryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MonsterMasterCopiesFriendlinessAndClearsReferencesToMaster(bool friendly)
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.ThingId == 7);
        var master = sim.Actors.Single(a => a.ThingId == 8);
        master.Friendly = friendly; actor.FriendPlayer = 2;
        actor.SetTargetPointer(master); actor.Brain!.RememberHateTarget(master.Id); actor.LastHeardTargetId = master.Id;
        AcsActorProperties.Set(sim, actor, 0, 25, 8);
        Assert.Equal(master.Id, actor.MasterId); Assert.Equal(0, actor.FriendPlayer);
        Assert.Equal(friendly, actor.Friendly);
        Assert.Null(actor.TargetId); Assert.Null(actor.Brain.LastEnemyId); Assert.Null(actor.LastHeardTargetId);
        var saved = SimSavegame.Write(sim, true); actor.MasterId = null; actor.Friendly = !friendly;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(8, AcsActorProperties.Get(sim, actor, 0, 25)); Assert.Equal(friendly, actor.Friendly);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearingMasterRestoresDefaultFriendlinessAndDropsNonHostileMemories(bool defaultFriendly)
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.ThingId == 7);
        var other = sim.Actors.Single(a => a.ThingId == 8);
        actor.SpawnFriendly = defaultFriendly; other.Friendly = defaultFriendly;
        actor.Friendly = !defaultFriendly; actor.FriendPlayer = 1; actor.MasterId = other.Id;
        actor.SetTargetPointer(other); actor.Brain!.RememberHateTarget(other.Id); actor.LastHeardTargetId = other.Id;
        AcsActorProperties.Set(sim, actor, 0, 25, 0);
        Assert.Null(actor.MasterId); Assert.Equal(0, actor.FriendPlayer); Assert.Equal(defaultFriendly, actor.Friendly);
        Assert.Null(actor.TargetId); Assert.Null(actor.Brain.LastEnemyId); Assert.Null(actor.LastHeardTargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerMasterUsesOwnershipAndAdoptsHostileDamageSource(bool friendlyAttacker)
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.ThingId == 7);
        var attacker = sim.Actors.Single(a => a.ThingId == 8); var player = sim.Players.First();
        attacker.Friendly = friendlyAttacker; player.LastDamageSourceId = attacker.Id;
        actor.MasterId = attacker.Id; actor.SetTargetPointer(player);
        actor.Brain!.RememberHateTarget(player.Id); actor.LastHeardTargetId = player.Id;
        AcsActorProperties.Set(sim, actor, 0, 25, 11);
        Assert.Null(actor.MasterId); Assert.Equal(1, actor.FriendPlayer); Assert.True(actor.Friendly);
        Assert.Equal(friendlyAttacker ? (uint?)null : attacker.Id, actor.TargetId);
        Assert.Equal(friendlyAttacker ? (uint?)null : attacker.Id, actor.LastHeardTargetId);
        Assert.Null(actor.Brain.LastEnemyId);
        Assert.Equal(11, AcsActorProperties.Get(sim, actor, 0, 25));
    }

    [Fact]
    public void NonMonsterSubjectAndNonMonsterMasterLeaveStateUnchanged()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.ThingId == 7);
        var other = sim.Actors.Single(a => a.ThingId == 8); other.IsMonster = false;
        actor.FriendPlayer = 2;
        AcsActorProperties.Set(sim, actor, 0, 25, 8);
        Assert.Null(actor.MasterId); Assert.Equal(2, actor.FriendPlayer);
        AcsActorProperties.Set(sim, sim.Players.First(), 0, 25, 7);
        Assert.Null(sim.Players.First().MasterId);
    }

    [Fact]
    public void TidSelectionUpdatesEverySubjectAndMissingMasterClears()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.ThingId == 7);
        var other = sim.Actors.Single(a => a.ThingId == 8); other.ThingId = 7;
        AcsActorProperties.Set(sim, null, 7, 25, 22);
        Assert.All(new[] { actor, other }, a => Assert.Equal(2, a.FriendPlayer));
        AcsActorProperties.Set(sim, null, 7, 25, 999);
        Assert.All(new[] { actor, other }, a => { Assert.Equal(0, a.FriendPlayer); Assert.False(a.Friendly); });
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 11)]
    [InlineData(2, 22)]
    [InlineData(99, 0)]
    public void PlayerOwnershipQueriesPlayerTid(int owner, int expected)
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.ThingId == 7);
        actor.FriendPlayer = owner;
        Assert.Equal(expected, AcsActorProperties.Get(sim, actor, 0, 25));
        Assert.Equal(expected, AcsActorProperties.Get(sim, null, 7, 25));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, 25, expected));
        Assert.False(AcsActorProperties.Check(sim, actor, 0, 25, expected + 1));
    }

    [Fact]
    public void ExplicitMasterTakesPrecedenceAndSavedStateRestoresQuery()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a.ThingId == 7);
        var master = sim.Actors.Single(a => a.ThingId == 8);
        actor.FriendPlayer = 1; actor.MasterId = master.Id;
        Assert.Equal(8, AcsActorProperties.Get(sim, actor, 0, 25));
        var saved = SimSavegame.Write(sim, true);
        actor.MasterId = null; actor.FriendPlayer = 2;
        Assert.Equal(22, AcsActorProperties.Get(sim, actor, 0, 25));
        SimSavegame.Apply(sim, saved);
        Assert.Equal(8, AcsActorProperties.Get(sim, actor, 0, 25));
        master.ThingId = 80;
        Assert.Equal(80, AcsActorProperties.Get(sim, actor, 0, 25));
    }

    [Fact]
    public void MissingActorsReturnZeroButDoNotMatch()
    {
        var sim = Room();
        Assert.Equal(0, AcsActorProperties.Get(sim, null, 0, 25));
        Assert.False(AcsActorProperties.Check(sim, null, 0, 25, 0));
        Assert.Equal(0, AcsActorProperties.Get(sim, null, 999, 25));
        Assert.False(AcsActorProperties.Check(sim, null, 999, 25, 0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [
            new LevelThing { Type = 1, Id = 11 }, new LevelThing { Type = 2, Id = 22, X = 64 },
            new LevelThing { Type = 3004, Id = 7, X = 128 }, new LevelThing { Type = 3004, Id = 8, X = 192 }]
    });
}
