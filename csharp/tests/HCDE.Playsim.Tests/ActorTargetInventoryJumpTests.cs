using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTargetInventoryJumpTests
{
    [Fact]
    public void TargetInventoryJumpExecutesOnCallerFrame()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var player = sim.Players.Single();
        actor.Brain!.SetSpecialTarget(player);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.JumpIfInTargetInventory(self, "Clip", 50, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
        Assert.Equal(ActorStateMachine.Spawn, player.States.Current);
        Assert.Equal(50, player.Inventory.Bullets);
    }

    [Fact]
    public void ForwardPointerIsResolvedRelativeToTarget()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var intermediate = sim.AddBot(40, 0);
        caller.Brain!.SetSpecialTarget(intermediate);
        intermediate.Brain!.SetSpecialTarget(sim.Players.Single());
        Assert.Null(ActorJumpActions.JumpIfInTargetInventory(caller, "Clip", 50, 2));
        Assert.Equal(2, ActorJumpActions.JumpIfInTargetInventory(caller, "Clip", 50, 2, AcsActorPointer.Target));
        Assert.Null(ActorJumpActions.JumpIfInTargetInventory(caller, "Clip", 51, 2, AcsActorPointer.Target));
        Assert.Null(ActorJumpActions.JumpIfInTargetInventory(caller, "Clip", 1, 2, AcsActorPointer.Null));
    }

    [Fact]
    public void MissingOrDestroyedTargetDoesNotJump()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.Players.Single();
        actor.Brain!.SetSpecialTarget(null);
        Assert.Null(ActorJumpActions.JumpIfInTargetInventory(actor, "Clip", 1, 2));
        actor.Brain.SetSpecialTarget(target); target.Destroy();
        Assert.Null(ActorJumpActions.JumpIfInTargetInventory(actor, "Clip", 1, 2));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 100 }],
    });
}
