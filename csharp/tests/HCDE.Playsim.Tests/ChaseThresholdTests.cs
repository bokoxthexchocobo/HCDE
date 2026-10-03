using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseThresholdTests
{
    [Theory]
    [InlineData(100, 99)]
    [InlineData(1, 0)]
    [InlineData(0, 0)]
    [InlineData(-1, -2)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void LivingTargetCountsDownAnyNonzeroThreshold(int initial, int expected)
    {
        var (sim, owner, target) = Room(initial);
        owner.Brain!.Tick(sim, owner);
        Assert.Equal(expected, owner.Brain.Threshold);
        Assert.Equal(target.Id, owner.Brain.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDeadTargetImmediatelyClearsThreshold(bool dead)
    {
        var (sim, owner, target) = Room(100);
        if (dead) target.Health = 0;
        else owner.Brain!.ClearTarget();
        owner.Brain!.Tick(sim, owner);
        Assert.Equal(0, owner.Brain.Threshold);
        var replacement = sim.Players.Single();
        ActorDamage.Apply(owner, 1, source: replacement, inflictor: replacement);
        Assert.Equal(replacement.Id, owner.Brain.TargetId);
        Assert.Equal(100, owner.Brain.Threshold);
    }

    [Fact]
    public void LivingNonShootableTargetDoesNotClearThreshold()
    {
        var (sim, owner, target) = Room(100);
        target.Shootable = false;
        owner.Brain!.Tick(sim, owner);
        Assert.Equal(99, owner.Brain.Threshold);
    }

    [Fact]
    public void RemovedTargetClearsThreshold()
    {
        var (sim, owner, target) = Room(100);
        target.Destroy();
        owner.Brain!.Tick(sim, owner);
        Assert.Equal(0, owner.Brain.Threshold);
    }

    private static (AuthoritySimulation Sim, BotPawn Owner, BotPawn Target) Room(int threshold)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 4096 }],
        });
        var owner = sim.AddBot(64, 64, doomEdNum: 3004);
        var target = sim.AddBot(96, 64, doomEdNum: 3001);
        owner.Health = target.Health = 100;
        owner.PainChance = 0;
        owner.Brain!.DefThreshold = threshold;
        ActorDamage.Apply(owner, 1, source: target, inflictor: target);
        Assert.Equal(target.Id, owner.Brain.TargetId);
        Assert.Equal(threshold, owner.Brain.Threshold);
        owner.ReactionTime = 10;
        return (sim, owner, target);
    }
}
