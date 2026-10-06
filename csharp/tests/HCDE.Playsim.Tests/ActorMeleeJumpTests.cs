using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorMeleeJumpTests
{
    [Theory]
    [InlineData(73, true)]
    [InlineData(74, false)]
    [InlineData(75, false)]
    public void InsideAndOutsideAreComplementaryAtNativeBoundary(int x, bool inside)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.Players.Single();
        actor.MeleeRange = Fixed.FromInt(64); target.Radius = Fixed.FromInt(10);
        target.X = Fixed.FromInt(x); actor.Brain!.SetSpecialTarget(target);
        Assert.Equal(inside ? 2 : (int?)null, ActorJumpActions.JumpIfTargetInsideMeleeRange(actor, 2));
        Assert.Equal(inside ? (int?)null : 2, ActorJumpActions.JumpIfTargetOutsideMeleeRange(actor, 2));
    }

    [Fact]
    public void VerticalBypassFlagSurvivesSaveAndPreservesHorizontalGate()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.Players.Single();
        target.X = Fixed.FromInt(20); target.Z = Fixed.FromInt(150);
        actor.Brain!.SetSpecialTarget(target);
        Assert.True(AcsActorFlags.TrySet(actor, "NOVERTICALMELEERANGE", true));
        Assert.Equal(2, ActorJumpActions.JumpIfTargetInsideMeleeRange(actor, 2));
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(); restored.AddBot(0, 0); restored.RestoreState(state);
        Assert.True(restored.Actors.Single(a => a.Id == actor.Id).NoVerticalMeleeRange);
        target.X = Fixed.FromInt(500);
        Assert.Null(ActorJumpActions.JumpIfTargetInsideMeleeRange(actor, 2));
        Assert.True(AcsActorFlags.TrySet(actor, "NOVERTICALMELEERANGE", false));
        Assert.False(actor.NoVerticalMeleeRange);
    }

    [Fact]
    public void MissingTargetTriggersOutsideJumpThroughFrameAction()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.Brain!.SetSpecialTarget(null);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.JumpIfTargetOutsideMeleeRange(self, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
        Assert.Null(ActorJumpActions.JumpIfTargetInsideMeleeRange(actor, 2));
    }

    [Fact]
    public void TargetAboveBodyCannotTriggerInsideJump()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.Players.Single();
        target.X = Fixed.FromInt(20);
        target.Z = Fixed.FromDouble(actor.Z.ToDouble() + actor.Height.ToDouble() + 1);
        actor.Brain!.SetSpecialTarget(target);
        Assert.Null(ActorJumpActions.JumpIfTargetInsideMeleeRange(actor, 2));
        Assert.Equal(2, ActorJumpActions.JumpIfTargetOutsideMeleeRange(actor, 2));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1, X = 100 }],
    });
}
