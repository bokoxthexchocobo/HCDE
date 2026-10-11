using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseGoalRetentionTests
{
    [Theory]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    public void OnlyAssignedGoalsSurviveFriendshipAndHealthRejection(bool friendly, bool dead, bool goal)
    {
        var (sim, hunter, target) = Room();
        if (goal) hunter.GoalId = target.Id;
        if (friendly) hunter.Friendly = target.Friendly = true;
        if (dead) target.Health = 0;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        Assert.Equal(goal ? target.Id : (uint?)null, hunter.Brain.TargetId);
        Assert.Equal(goal ? MonsterMode.Chase : MonsterMode.Idle, hunter.Brain.Mode);
        Assert.Equal(goal, hunter.VelocityX.Raw > 0);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    [Fact]
    public void DestroyedGoalCannotRemainATarget()
    {
        var (sim, hunter, target) = Room();
        hunter.GoalId = target.Id;
        target.Destroy();
        hunter.Brain!.Tick(sim, hunter, useMelee: false, useMissile: false);
        Assert.Null(hunter.Brain.TargetId);
    }

    [Fact]
    public void ChangingGoalRestoresOrdinaryTargetRejection()
    {
        var (sim, hunter, target) = Room();
        hunter.GoalId = target.Id;
        hunter.Friendly = target.Friendly = true;
        hunter.Brain!.Tick(sim, hunter, useMelee: false, useMissile: false);
        Assert.Equal(target.Id, hunter.Brain.TargetId);
        hunter.GoalId = null;
        hunter.Brain.Tick(sim, hunter, useMelee: false, useMissile: false);
        Assert.Null(hunter.Brain.TargetId);
    }

    private static (AuthoritySimulation, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200, Id = 7 }],
        });
        var hunter = sim.Actors[0]; var target = sim.Actors[1];
        hunter.ReactionTime = 0;
        hunter.Brain!.SetTargetThingId(sim, 7);
        return (sim, hunter, target);
    }
}
