using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFlagJumpTests
{
    [Theory]
    [InlineData("NOVERTICALMELEERANGE", true)]
    [InlineData("actor.noverticalmeleerange", true)]
    [InlineData("NoTeleport", false)]
    [InlineData("UnknownFlag", false)]
    [InlineData("", false)]
    public void FlagJumpUsesSharedNamesAndCurrentFlagValue(string name, bool jumps)
    {
        var actor = new Actor { NoVerticalMeleeRange = true };
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.CheckFlag(self, name, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(jumps ? 2 : 1, actor.States.Current);
    }

    [Fact]
    public void SelectedActorFlagDoesNotReadCallerFlag()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
        });
        var actor = sim.AddBot(64, 0); sim.Players.Single().NoTeleport = true;
        Assert.Equal(2, ActorJumpActions.CheckFlag(actor, "NoTeleport", 2, AcsActorPointer.Player1));
        Assert.Null(ActorJumpActions.CheckFlag(actor, "NoTeleport", 2));
    }

    [Fact]
    public void NullAndDestroyedSubjectsDoNotJump()
    {
        var actor = new Actor { NoTeleport = true };
        Assert.Null(ActorJumpActions.CheckFlag(actor, "NoTeleport", 2, AcsActorPointer.Null));
        actor.Destroy();
        Assert.Null(ActorJumpActions.CheckFlag(actor, "NoTeleport", 2));
    }

    [Fact]
    public void SuppressionDoesNotEvaluateFlagCallback()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.CheckFlag(self, "NoTeleport", 2, AcsActorPointer.Player1)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1, noFunction: true);
        Assert.Equal(1, actor.States.Current);
        Assert.Throws<InvalidOperationException>(() => actor.States.Enter(actor, 1));
    }
}
