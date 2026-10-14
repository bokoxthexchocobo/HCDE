using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseNoTargetTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RememberedPlayerNoiseHonorsNoTarget(bool directHear)
    {
        var (sim, hunter, player) = Room();
        player.NoTarget = true;
        hunter.LastHeardTargetId = player.Id;
        if (directHear) hunter.Brain!.Hear(sim, hunter, player);
        hunter.Brain!.Tick(sim, hunter);
        Assert.Null(hunter.Brain.TargetId);
        Assert.Equal(MonsterMode.Idle, hunter.Brain.Mode);
    }

    [Fact]
    public void TogglingNoTargetCancelsAttackAndClearsMemory()
    {
        var (sim, hunter, player) = Room();
        hunter.Brain!.Hear(sim, hunter, player);
        hunter.Brain.Tick(sim, hunter);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
        hunter.LastHeardTargetId = player.Id;
        hunter.Brain.SetChaseThreshold(100, false);
        player.NoTarget = true;
        var health = player.Health;
        for (var tic = 0; tic < 16; tic++) hunter.Brain.Tick(sim, hunter);
        Assert.Null(hunter.Brain.TargetId);
        Assert.Null(hunter.Brain.LastEnemyId);
        Assert.Null(hunter.LastHeardTargetId);
        Assert.Equal(0, hunter.Brain.Threshold);
        Assert.Equal(0, hunter.Brain.WindupTics);
        Assert.Equal(health, player.Health);
        player.NoTarget = false;
        hunter.Brain.Tick(sim, hunter);
        Assert.Equal(player.Id, hunter.Brain.TargetId);
    }

    [Fact]
    public void ExistingGoalIsExemptFromPursuitNoTargetCheck()
    {
        var (sim, hunter, player) = Room();
        hunter.Brain!.Hear(sim, hunter, player);
        hunter.GoalId = player.Id;
        player.X = Fixed.FromInt(512);
        player.NoTarget = true;
        hunter.Brain.Tick(sim, hunter, useMelee: false, useMissile: false);
        Assert.Equal(player.Id, hunter.Brain.TargetId);
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 64 }] });
        var hunter = new Actor { Simulation = sim, Brain = new MonsterBrain(MonsterAttack.Melee), ReactionTime = 0,
            MeleeRange = Fixed.FromInt(128) };
        hunter.States.Configure(hunter, [new(-1, 0)], 0);
        return (sim, hunter, sim.Players.Single());
    }
}
