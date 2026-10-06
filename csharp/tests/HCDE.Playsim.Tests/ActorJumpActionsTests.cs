using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorJumpActionsTests
{
    [Theory]
    [InlineData(49, true)]
    [InlineData(50, false)]
    [InlineData(51, false)]
    [InlineData(-1, true)]
    public void HealthJumpUsesStrictComparisonInStateExecution(int health, bool jumps)
    {
        var actor = new Actor { Health = health };
        actor.States.Configure(actor, [new(-1, 0),
            new(5, 0, StateAction: self => ActorJumpActions.JumpIfHealthLower(self, 50, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(jumps ? 2 : 1, actor.States.Current);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConditionalJumpExecutesThroughFrameCallback(bool condition)
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0),
            new(5, 0, StateAction: _ => ActorJumpActions.JumpIf(condition, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(condition ? 2 : 1, actor.States.Current);
    }

    [Fact]
    public void HealthJumpReadsSelectedPlayerAndNullNeverJumps()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
        });
        var actor = sim.AddBot(64, 0); actor.Health = 100;
        sim.Players.Single().Health = 1;
        Assert.Equal(2, ActorJumpActions.JumpIfHealthLower(actor, 50, 2, AcsActorPointer.Player1));
        Assert.Null(ActorJumpActions.JumpIfHealthLower(actor, 50, 2));
        Assert.Null(ActorJumpActions.JumpIfHealthLower(actor, int.MaxValue, 2, AcsActorPointer.Null));
    }

    [Fact]
    public void NonlocalPointerWithoutSimulationFailsExplicitly()
    {
        Assert.Throws<InvalidOperationException>(() => ActorJumpActions.JumpIfHealthLower(
            new Actor(), 50, 2, AcsActorPointer.Target));
    }
}
