using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeChaseMovementTests
{
    [Fact]
    public void ChaseSelectsDirectionAndMovesWithoutOverwritingMomentum()
    {
        var (sim, actor) = Room(); actor.MoveDirection = 2; actor.VelocityY = Fixed.FromInt(3);
        Chase(sim, actor);
        Assert.Equal(Fixed.FromInt(8), actor.X); Assert.Equal(default, actor.Y);
        Assert.Equal(Fixed.FromInt(3), actor.VelocityY); Assert.Equal(0, actor.MoveDirection);
        Assert.NotNull(sim.CaptureState().TryWalkRandomState);
    }

    [Fact]
    public void NoRandomTurnKeepsExpiredDirectionAndDoesNotConsumeSelectionStreams()
    {
        var (sim, actor) = Room(); actor.MoveDirection = 2;
        Chase(sim, actor, MonsterChaseFlags.NoRandomTurn);
        Assert.Equal(Fixed.FromInt(8), actor.Y); Assert.Equal(-1, actor.MoveCount);
        Assert.Null(sim.CaptureState().NewChaseDirRandomState); Assert.Null(sim.CaptureState().TryWalkRandomState);
    }

    [Theory]
    [InlineData(5, 4)]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    public void DontMoveOnlyDecrementsPositiveMoveCount(int initial, int expected)
    {
        var (sim, actor) = Room(); actor.MoveCount = initial;
        Chase(sim, actor, MonsterChaseFlags.DontMove);
        Assert.Equal(expected, actor.MoveCount); Assert.Equal(default, actor.X);
        Assert.Null(sim.CaptureState().TryWalkRandomState);
    }

    [Fact]
    public void ChaseGoalDrivesDirectionWhileEnemyRemainsTarget()
    {
        var (sim, actor) = Room(); var enemy = sim.Players.Single();
        var goal = sim.AddBot(0, 300); goal.Shootable = false;
        actor.GoalId = goal.Id; actor.ChaseGoal = true;
        Chase(sim, actor);
        Assert.Equal(Fixed.FromInt(8), actor.Y); Assert.Equal(default, actor.X);
        Assert.Equal(enemy.Id, actor.TargetId);
    }

    [Fact]
    public void TurningUsesMovementDirectionInFortyFiveDegreeSteps()
    {
        var (sim, actor) = Room(); actor.MoveDirection = 2; actor.Angle = BamAngle.FromDegrees(0);
        Chase(sim, actor, MonsterChaseFlags.NoRandomTurn);
        Assert.Equal(BamAngle.FromDegrees(45), actor.Angle);
    }

    [Fact]
    public void SavedNativeMovementReplaysPositionsCountersAndRandomStreams()
    {
        var (sim, actor) = Room(); Chase(sim, actor);
        var saved = SimSavegame.Write(sim, true); Chase(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Chase(sim, actor);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static void Chase(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags = 0)
        => actor.Brain!.Tick(sim, actor, flags | MonsterChaseFlags.NoPlayActive,
            useMelee: false, useMissile: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 300 }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.MovementSpeed = Fixed.FromInt(8);
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
