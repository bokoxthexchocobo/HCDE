using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorHeightJumpTests
{
    [Theory]
    [InlineData(21, true, 2)]
    [InlineData(20, true, null)]
    [InlineData(-31, true, 3)]
    [InlineData(-30, true, null)]
    [InlineData(1, false, 2)]
    [InlineData(-1, false, 3)]
    [InlineData(0, false, null)]
    public void HeightComparisonUsesStrictDirectionalBodyBounds(int z, bool includeHeight, int? expected)
    {
        var (actor, target) = Setup(); target.Z = Fixed.FromInt(z);
        Assert.Equal(expected, ActorJumpActions.JumpIfHigherOrLower(actor, 2, 3, includeHeight: includeHeight));
    }

    [Fact]
    public void HighBranchTakesPriorityAndMissingHighAllowsLowBranch()
    {
        var (actor, target) = Setup(); target.Z = Fixed.FromInt(10);
        Assert.Equal(2, ActorJumpActions.JumpIfHigherOrLower(actor, 2, 3, -20, 50));
        Assert.Equal(3, ActorJumpActions.JumpIfHigherOrLower(actor, null, 3, -20, 50));
    }

    [Fact]
    public void SelfAndNullPointersDoNotJump()
    {
        var (actor, _) = Setup();
        Assert.Null(ActorJumpActions.JumpIfHigherOrLower(actor, 2, 3, -100, 100,
            pointerSelector: AcsActorPointer.Default));
        Assert.Null(ActorJumpActions.JumpIfHigherOrLower(actor, 2, 3, pointerSelector: AcsActorPointer.Null));
    }

    [Fact]
    public void ReturnedHeightBranchExecutesImmediately()
    {
        var (actor, target) = Setup(); target.Z = Fixed.FromInt(21);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.JumpIfHigherOrLower(self, 2, 3)), new(-1, 2), new(-1, 3)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
    }

    private static (Actor Actor, Actor Target) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1, X = 100 }],
        });
        var actor = sim.AddBot(0, 0); var target = sim.Players.Single();
        actor.Height = Fixed.FromInt(20); target.Height = Fixed.FromInt(30);
        actor.Brain!.SetSpecialTarget(target);
        return (actor, target);
    }
}
