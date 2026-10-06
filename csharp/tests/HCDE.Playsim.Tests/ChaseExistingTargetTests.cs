using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseExistingTargetTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LivingExistingTargetRemainsAfterShootableFlagClears(bool noTarget)
    {
        var (sim, hunter, target) = Room();
        target.Shootable = false; target.NoTarget = noTarget;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(target.Id, hunter.Brain.TargetId);
        Assert.NotEqual(MonsterMode.Idle, hunter.Brain.Mode);
    }

    [Fact]
    public void DeadExistingTargetIsDroppedEvenWhenStillShootable()
    {
        var (sim, hunter, target) = Room();
        target.Health = 0; target.Shootable = true;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Null(hunter.Brain.TargetId);
        Assert.Equal(MonsterMode.Idle, hunter.Brain.Mode);
    }

    [Fact]
    public void ExistingTargetThatBecomesFriendlyIsDropped()
    {
        var (sim, hunter, target) = Room();
        hunter.Friendly = target.Friendly = true;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Null(hunter.Brain.TargetId);
        Assert.Equal(MonsterMode.Idle, hunter.Brain.Mode);
    }

    [Fact]
    public void UnshootableHeardActorCannotBecomeNewTarget()
    {
        var (sim, hunter, target) = Room();
        hunter.Brain!.ClearTarget(); target.Shootable = false;
        hunter.LastHeardTargetId = target.Id;
        hunter.Brain.Tick(sim, hunter);
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
