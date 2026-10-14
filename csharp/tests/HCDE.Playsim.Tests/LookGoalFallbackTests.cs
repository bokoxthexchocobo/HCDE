using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LookGoalFallbackTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingHateeResumesGoalBeforePlayerFallback(bool huntPlayers)
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0);
        var goal = sim.AddBot(-300, 0, 3001); goal.Shootable = false;
        hunter.GoalId = goal.Id; hunter.ReactionTime = 0; hunter.TidToHate = 99; hunter.HuntPlayers = huntPlayers;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.DontLookAllAround);
        Assert.Equal(goal.Id, hunter.TargetId);
    }

    [Fact]
    public void VisibleHateeTakesPriorityOverUnacquiredGoal()
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0);
        var goal = sim.AddBot(-300, 0, 3001); hunter.GoalId = goal.Id; hunter.ReactionTime = 0;
        var hatee = sim.AddBot(200, 0, 3001); hatee.ThingId = 99; hunter.TidToHate = 99;
        hunter.Brain!.Tick(sim, hunter); Assert.Equal(hatee.Id, hunter.TargetId);
    }

    [Fact]
    public void DestroyedGoalAllowsPlayerFallback()
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0);
        var goal = sim.AddBot(-300, 0, 3001); hunter.GoalId = goal.Id; hunter.ReactionTime = 0; goal.Destroy();
        hunter.TidToHate = 99; hunter.HuntPlayers = true;
        hunter.Brain!.Tick(sim, hunter); Assert.Equal(sim.Players.Single().Id, hunter.TargetId);
    }

    [Fact]
    public void OrdinaryLookResumesGoalAfterPlayerSearchFails()
    {
        var sim = Room(); sim.Players.Single().NoTarget = true;
        var hunter = sim.AddBot(0, 0); var goal = sim.AddBot(-300, 0, 3001);
        hunter.GoalId = goal.Id; hunter.ReactionTime = 0; goal.Shootable = false;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.DontLookAllAround);
        Assert.Equal(goal.Id, hunter.TargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
