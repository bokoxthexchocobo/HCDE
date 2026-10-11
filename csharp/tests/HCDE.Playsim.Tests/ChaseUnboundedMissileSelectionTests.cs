using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseUnboundedMissileSelectionTests
{
    [Theory]
    [InlineData(3004, 1024)]
    [InlineData(3004, 1025)]
    [InlineData(3004, 4096)]
    [InlineData(3001, 4096)]
    [InlineData(66, 4096)]
    [InlineData(16, 4096)]
    public void ProfilesWithoutMaxTargetRangeCanSelectDistantMissiles(int type, int distance)
    {
        var (sim, hunter) = Room(type, distance);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(sim.Players.Single().Id, hunter.Brain.TargetId);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
    }

    [Theory]
    [InlineData(MonsterAttack.Hitscan)]
    [InlineData(MonsterAttack.Fireball)]
    public void GenericMissileSelectionHasNoArtificialDistanceLimit(MonsterAttack attack)
    {
        var (sim, hunter) = Room(3004, 4096);
        hunter.Brain = new MonsterBrain(attack);
        hunter.ReactionTime = 0;
        hunter.Brain.Tick(sim, hunter);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DistantAcquisitionStillHonorsAttackEnableAndReactionGate(bool missileEnabled)
    {
        var (sim, hunter) = Room(3004, 4096);
        if (missileEnabled) hunter.ReactionTime = 10;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, missileEnabled);
        Assert.Equal(sim.Players.Single().Id, hunter.Brain.TargetId);
        Assert.Equal(MonsterMode.Chase, hunter.Brain.Mode);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    private static (AuthoritySimulation, Actor) Room(int type, int distance)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = distance }, new LevelThing { Type = type }],
        });
        var hunter = sim.Actors.Single(actor => actor.DoomEdNum == type);
        hunter.ReactionTime = 0;
        return (sim, hunter);
    }
}
