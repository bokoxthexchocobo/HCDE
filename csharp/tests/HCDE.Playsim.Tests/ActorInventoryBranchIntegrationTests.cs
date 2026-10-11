using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryBranchIntegrationTests
{
    [Theory]
    [InlineData(50, 2)]
    [InlineData(51, 1)]
    [InlineData(0, 1)]
    public void StateActionBranchesImmediatelyOnlyWhenThresholdMatches(int amount, int expected)
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = 50;
        var calls = 0;
        player.States.Configure(player, [new(-1, 0),
            new(8, 0, StateAction: self => ActorJumpActions.JumpIfInventory(self, "Clip", amount, 2)),
            new(3, 0, Action: _ => calls++)], 0);
        player.States.Enter(player, 1);
        Assert.Equal(expected, player.States.Current);
        Assert.Equal(expected == 2 ? 3 : 8, player.States.RemainingTics);
        Assert.Equal(expected == 2 ? 1 : 0, calls);
        Assert.Equal(50, player.Inventory.Bullets);
    }

    [Fact]
    public void TargetForwardingResolvesFromTargetButReturnsCallersState()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var caller = sim.AddBot(20, 0);
        var target = sim.AddBot(40, 0);
        caller.Brain!.SetSpecialTarget(target);
        target.MasterId = player.Id;
        caller.MasterId = target.Id;
        ActorInventoryActions.SetInventory(player, "MedPatch", 20);
        caller.States.Configure(caller, [new(-1, 0), new(8, 0,
            StateAction: self => ActorJumpActions.JumpIfInTargetInventory(self, "MedPatch", 0, 2, 4)),
            new(-1, 2)], 0);
        caller.States.Enter(caller, 1);
        Assert.Equal(2, caller.States.Current);
        Assert.Equal(0, target.States.Current);
        Assert.Equal(20, player.Inventory.CountHealthPickup("MedPatch"));
        Assert.Null(ActorJumpActions.JumpIfInTargetInventory(caller, "MedPatch", 1, 2));
    }

    [Fact]
    public void MissingAndDestroyedSubjectsReturnNoJump()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var caller = sim.AddBot(20, 0);
        Assert.Null(ActorJumpActions.JumpIfInTargetInventory(caller, "Pistol", 1, 0));
        caller.Brain!.SetSpecialTarget(player);
        Assert.Equal(0, ActorJumpActions.JumpIfInTargetInventory(caller, "Pistol", 1, 0));
        Assert.Null(ActorJumpActions.JumpIfInventory(player, null, 1, 0));
        player.Destroy();
        Assert.Null(ActorJumpActions.JumpIfInTargetInventory(caller, "Pistol", 1, 0));
    }

    [Fact]
    public void InvalidReturnedStateUsesExistingStateMachineValidation()
    {
        var player = new PlayerPawn();
        player.States.Configure(player, [new(-1, 0), new(4, 0,
            StateAction: self => ActorJumpActions.JumpIfInventory(self, "Pistol", 1, 99))], 0);
        Assert.Throws<InvalidOperationException>(() => player.States.Enter(player, 1));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
