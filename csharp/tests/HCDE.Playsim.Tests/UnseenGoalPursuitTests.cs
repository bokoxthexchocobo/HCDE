using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UnseenGoalPursuitTests
{
    [Fact]
    public void NewlyAcquiredUnseenGoalSuppliesMovementCoordinates()
    {
        var (sim, hunter, goal) = Room();
        Assert.False(CombatTrace.HasLineOfSight(sim, hunter, goal));
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(goal.Id, hunter.TargetId);
        Assert.True(hunter.VelocityX.Raw > 0);
        Assert.Equal(0, hunter.VelocityY.Raw);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    [Fact]
    public void MovingUnseenGoalRefreshesDirection()
    {
        var (sim, hunter, goal) = Room();
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        goal.Y = Fixed.FromInt(100);
        Assert.False(CombatTrace.HasLineOfSight(sim, hunter, goal));
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.True(hunter.VelocityX.Raw > 0);
        Assert.True(hunter.VelocityY.Raw > 0);
    }

    [Fact]
    public void UnseenGoalPursuitContinuesAfterRestore()
    {
        var (sim, hunter, goal) = Room();
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        var saved = SimSavegame.Write(sim, true);
        goal.Y = Fixed.FromInt(100);
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        goal.Y = Fixed.FromInt(100);
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Hunter, Actor Goal) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Lines = [new LevelLine { X1 = 100, Y1 = -256, X2 = 100, Y2 = 256, SideBack = -1 }]
        });
        var hunter = sim.AddBot(0, 0); var goal = sim.AddBot(300, 0, 3001);
        goal.Shootable = false; hunter.GoalId = goal.Id; hunter.ReactionTime = 0; hunter.TidToHate = 99;
        return (sim, hunter, goal);
    }
}
