using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseFriendshipTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FriendshipChangeDropsCurrentTargetAndCancelsAttack(bool rememberedNoise)
    {
        var (sim, owner, other) = Room();
        owner.Brain!.SetTargetThingId(sim, other.ThingId);
        owner.Brain.Tick(sim, owner);
        Assert.Equal(MonsterMode.Windup, owner.Brain.Mode);
        if (rememberedNoise) owner.LastHeardTargetId = other.Id;
        owner.Friendly = other.Friendly = true;
        var health = other.Health;
        for (var tic = 0; tic < 15; tic++) owner.Brain.Tick(sim, owner);
        Assert.Null(owner.Brain.TargetId);
        Assert.Equal(MonsterMode.Idle, owner.Brain.Mode);
        Assert.Equal(0, owner.Brain.WindupTics);
        Assert.Equal(health, other.Health);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FriendlyNoiseDoesNotAcquireTargetImmediatelyOrOnLaterTick(bool enabled)
    {
        var (sim, owner, other) = Room();
        owner.Friendly = other.Friendly = true;
        owner.Brain!.Enabled = enabled;
        owner.LastHeardTargetId = other.Id;
        owner.Brain.Hear(sim, owner, other);
        Assert.Null(owner.Brain.TargetId);
        owner.Brain.Enabled = true;
        owner.Brain.Tick(sim, owner);
        Assert.Null(owner.Brain.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HostileNoiseAndCurrentTargetsRemainValid(bool bothFriendlyWithDifferentOwners)
    {
        var (sim, owner, other) = Room();
        if (bothFriendlyWithDifferentOwners)
        {
            owner.Friendly = other.Friendly = true;
            owner.FriendPlayer = 1; other.FriendPlayer = 2;
        }
        owner.Brain!.Hear(sim, owner, other);
        if (bothFriendlyWithDifferentOwners)
        {
            Assert.Null(owner.Brain.TargetId);
            owner.Brain.Tick(sim, owner);
            Assert.Null(owner.Brain.TargetId);
            Assert.Equal(MonsterMode.Idle, owner.Brain.Mode);
            return;
        }
        Assert.Equal(other.Id, owner.Brain.TargetId);
        owner.Brain.Tick(sim, owner);
        Assert.Equal(other.Id, owner.Brain.TargetId);
        Assert.Equal(MonsterMode.Windup, owner.Brain.Mode);
    }

    private static (AuthoritySimulation Sim, Actor Owner, Actor Other) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 4096 },
                new LevelThing { Type = 3004, X = 64 },
                new LevelThing { Type = 3001, Id = 7, X = 128 }],
        });
        var owner = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        var other = sim.Actors.Single(actor => actor.DoomEdNum == 3001);
        owner.ReactionTime = 0;
        other.Brain!.Enabled = false;
        return (sim, owner, other);
    }
}
