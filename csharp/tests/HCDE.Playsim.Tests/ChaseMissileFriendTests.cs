using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseMissileFriendTests
{
    [Theory]
    [InlineData(3004, true, true, true)]
    [InlineData(3001, true, true, true)]
    [InlineData(3004, false, true, false)]
    [InlineData(3004, true, false, false)]
    [InlineData(3001, false, true, false)]
    [InlineData(3001, true, false, false)]
    public void OrdinaryMissileSelectionChecksForInterveningFriend(int type, bool friendlyShooter, bool friendlyBlocker, bool blocked)
    {
        var (sim, hunter, blocker) = Room(type);
        hunter.Friendly = friendlyShooter;
        blocker.Friendly = friendlyBlocker;
        Assert.Equal(blocked, MonsterBrain.HitFriend(sim, hunter, sim.Players.Single()));
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(blocked ? MonsterMode.Chase : MonsterMode.Windup, hunter.Brain.Mode);
        Assert.Equal(sim.Players.Single().Id, hunter.Brain.TargetId);
    }

    [Fact]
    public void MovingFriendAsideAllowsFirstMissileAttack()
    {
        var (sim, hunter, blocker) = Room(3004);
        hunter.Friendly = blocker.Friendly = true;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(MonsterMode.Chase, hunter.Brain.Mode);
        blocker.Y = Fixed.FromInt(100);
        hunter.Brain.Tick(sim, hunter);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
    }

    [Fact]
    public void JustHitRetaliationPrecedesFriendInLineCheck()
    {
        var (sim, hunter, blocker) = Room(3004);
        hunter.Friendly = blocker.Friendly = true;
        hunter.ReactionTime = 10;
        hunter.JustHit = true;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
        Assert.False(hunter.JustHit);
    }

    [Theory]
    [InlineData(MonsterAttack.Hitscan)]
    [InlineData(MonsterAttack.Fireball)]
    public void GenericMissileSelectionUsesSameFriendGate(MonsterAttack attack)
    {
        var (sim, hunter, blocker) = Room(3004);
        hunter.Brain = new MonsterBrain(attack);
        hunter.ReactionTime = 0;
        hunter.Friendly = blocker.Friendly = true;
        hunter.Brain.Tick(sim, hunter);
        Assert.Equal(MonsterMode.Chase, hunter.Brain.Mode);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    private static (AuthoritySimulation, Actor, Actor) Room(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = type },
                new LevelThing { Type = 3002, X = 250 }],
        });
        var hunter = sim.Actors.Single(actor => actor.DoomEdNum == type);
        hunter.ReactionTime = 0;
        sim.Players.Single().Friendly = false;
        return (sim, hunter, sim.Actors.Single(actor => actor.DoomEdNum == 3002));
    }
}
