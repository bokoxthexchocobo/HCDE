using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseGoalSteeringTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FlagSelectsMovementDestinationWithoutReplacingEnemy(bool enabled)
    {
        var (sim, hunter, goal, enemy) = Room(); hunter.ChaseGoal = enabled;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        Assert.Equal(enemy.Id, hunter.TargetId);
        Assert.Equal(enabled, hunter.VelocityY.Raw > 0); Assert.Equal(!enabled, hunter.VelocityX.Raw > 0);
    }

    [Fact]
    public void MovingGoalRefreshesDestination()
    {
        var (sim, hunter, goal, _) = Room(); hunter.ChaseGoal = true;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        goal.X = Fixed.FromInt(-300); goal.Y = default;
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        Assert.True(hunter.VelocityX.Raw < 0); Assert.Equal(0, hunter.VelocityY.Raw);
    }

    [Fact]
    public void DestroyedGoalFallsBackToEnemy()
    {
        var (sim, hunter, goal, enemy) = Room(); hunter.ChaseGoal = true; goal.Destroy();
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        Assert.True(hunter.VelocityX.Raw > 0); Assert.Equal(0, hunter.VelocityY.Raw); Assert.Equal(enemy.Id, hunter.TargetId);
    }

    [Fact]
    public void DontMoveStillPreventsGoalMovement()
    {
        var (sim, hunter, _, _) = Room(); hunter.ChaseGoal = true;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.DontMove, false, false);
        Assert.Equal(0, hunter.VelocityX.Raw); Assert.Equal(0, hunter.VelocityY.Raw);
    }

    [Fact]
    public void ChaseGoalDoesNotSuppressEnemyAttack()
    {
        var (sim, hunter, _, enemy) = Room(); hunter.ChaseGoal = true; hunter.ReactionTime = 0;
        enemy.X = Fixed.FromInt(40);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(enemy.Id, hunter.TargetId); Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
    }

    [Fact]
    public void NearbyEnemyDoesNotStopGoalMovementWhenAttacksDisabled()
    {
        var (sim, hunter, _, enemy) = Room(); hunter.ChaseGoal = true; enemy.X = Fixed.FromInt(40);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        Assert.True(hunter.VelocityY.Raw > 0);
    }

    [Fact]
    public void SteeringContinuesAfterSaveRestore()
    {
        var (sim, hunter, _, _) = Room(); hunter.ChaseGoal = true; var saved = SimSavegame.Write(sim, true);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        var expected = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved);
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Hunter, Actor Goal, Actor Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 9024, X = 0, Y = 300 }, new LevelThing { Type = 1, X = 300 }] });
        var hunter = sim.Actors.Single(a => a.ClassDoomEdNum == 3004);
        var goal = sim.Actors.Single(a => a.ClassDoomEdNum == 9024); var enemy = sim.Players.Single();
        hunter.GoalId = goal.Id; hunter.SetTargetPointer(enemy);
        return (sim, hunter, goal, enemy);
    }
}
