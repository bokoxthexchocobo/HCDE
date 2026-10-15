using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseGoalAttackSuppressionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SeparateActiveGoalAllowsEnemyMeleeAndMissileSelection(bool melee)
    {
        var (sim, actor, goal) = Room();
        if (melee) actor.X = Fixed.FromInt(280);
        var enemy = actor.TargetId; actor.JustHit = true;
        Chase(sim, actor, melee);
        Assert.Equal(8, actor.Brain!.WindupTics);
        Assert.Equal(!melee, actor.JustAttacked); Assert.Equal(melee, actor.JustHit);
        Assert.Equal(enemy, actor.TargetId); Assert.Equal(goal.Id, actor.GoalId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedGoalDoesNotSuppressEnemyAttack(bool destroyed)
    {
        var (sim, actor, goal) = Room();
        if (destroyed) goal.Destroy(); else actor.GoalId = null;
        Chase(sim, actor);
        Assert.Equal(8, actor.Brain!.WindupTics);
    }

    [Fact]
    public void AssignedGoalWithoutChaseFlagDoesNotSuppressEnemyAttack()
    {
        var (sim, actor, _) = Room(); actor.ChaseGoal = false;
        Chase(sim, actor); Assert.Equal(8, actor.Brain!.WindupTics);
    }

    [Fact]
    public void SeparateActiveGoalPreservesPendingManagedWindup()
    {
        var (sim, actor, _) = Room(); actor.ChaseGoal = false;
        Chase(sim, actor); Assert.Equal(8, actor.Brain!.WindupTics);
        actor.ChaseGoal = true; Chase(sim, actor);
        Assert.Equal(7, actor.Brain.WindupTics);
    }

    [Fact]
    public void GoalAsTargetSuppressesEnemyAttackAndPreservesRetaliation()
    {
        var (sim, actor, goal) = Room(); actor.SetTargetPointer(goal); actor.JustHit = true;
        Chase(sim, actor);
        Assert.Equal(0, actor.Brain!.WindupTics); Assert.True(actor.JustHit);
        Assert.Equal(goal.Id, actor.TargetId);
    }

    [Fact]
    public void ReachingSeparateGoalReturnsBeforeEnemyAttack()
    {
        var (sim, actor, goal) = Room(); goal.X = actor.X; goal.Y = actor.Y;
        var enemy = actor.TargetId;
        Chase(sim, actor);
        Assert.Equal(0, actor.Brain!.WindupTics); Assert.True(actor.JustAttacked);
        Assert.Equal(enemy, actor.TargetId); Assert.Null(actor.GoalId);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor, bool melee = false) => actor.Brain!.Tick(sim, actor,
        MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPlayActive, useMelee: melee, nativeMovement: true);

    private static (AuthoritySimulation, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 300 }]
        });
        var actor = sim.AddBot(0, 0); var goal = sim.AddBot(0, 500); goal.Shootable = false;
        actor.Brain = new MonsterBrain(MonsterAttack.Hitscan); actor.ReactionTime = 0;
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        actor.SetTargetPointer(sim.Players.Single()); actor.GoalId = goal.Id; actor.ChaseGoal = true;
        return (sim, actor, goal);
    }
}
