using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseReactionAttackTests
{
    [Theory]
    [InlineData(3, 2)]
    [InlineData(-1, -2)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void NonzeroReactionBlocksMissilesButAllowsChase(int initial, int expected)
    {
        var (sim, hunter, player) = Room(200, MonsterAttack.Hitscan);
        hunter.ReactionTime = initial;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(expected, hunter.ReactionTime);
        Assert.Equal(player.Id, hunter.Brain.TargetId);
        Assert.Equal(MonsterMode.Chase, hunter.Brain.Mode);
        Assert.True(hunter.VelocityX.Raw > 0);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    public void MissileCanStartWhenCountdownReachesZero(int reaction)
    {
        var (sim, hunter, _) = Room(200, MonsterAttack.Hitscan);
        hunter.ReactionTime = reaction;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(0, hunter.ReactionTime);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
    }

    [Theory]
    [InlineData(MonsterAttack.Melee)]
    [InlineData(MonsterAttack.Hitscan)]
    [InlineData(MonsterAttack.Fireball)]
    public void MeleeSelectionDoesNotWaitForReactionTimer(MonsterAttack attack)
    {
        var (sim, hunter, _) = Room(40, attack);
        hunter.ReactionTime = 10;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(9, hunter.ReactionTime);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
    }

    [Fact]
    public void DisablingMeleeStillRequiresMissileReactionTimer()
    {
        var (sim, hunter, _) = Room(40, MonsterAttack.Hitscan);
        hunter.ReactionTime = 10;
        hunter.Brain!.Tick(sim, hunter, useMelee: false);
        Assert.Equal(9, hunter.ReactionTime);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Room(int distance, MonsterAttack attack)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = distance }] });
        var hunter = new Actor { Simulation = sim, Brain = new MonsterBrain(attack), ReactionTime = 0,
            MeleeRange = Fixed.FromInt(64) };
        hunter.States.Configure(hunter, [new(-1, 0)], 0);
        return (sim, hunter, sim.Players.Single());
    }
}
