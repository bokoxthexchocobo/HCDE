using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryJumpTests
{
    [Theory]
    [InlineData(50, 50, true)]
    [InlineData(50, 51, false)]
    [InlineData(50, 0, false)]
    [InlineData(200, 0, true)]
    [InlineData(200, -1, true)]
    public void AmmoThresholdAndFullInventoryRulesExecuteThroughFrames(int count, int amount, bool jumps)
    {
        var actor = Room().Players.Single(); actor.Inventory.Bullets = count;
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.JumpIfInventory(self, "Clip", amount, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(jumps ? 2 : 1, actor.States.Current);
        Assert.Equal(count, actor.Inventory.Bullets);
    }

    [Theory]
    [InlineData("Health")]
    [InlineData("Unknown")]
    [InlineData("")]
    public void MissingItemsAndHealthAliasDoNotJump(string type)
    {
        Assert.Null(ActorJumpActions.JumpIfInventory(Room().Players.Single(), type, 0, 2));
    }

    [Fact]
    public void PointerSelectsInventoryOwner()
    {
        var sim = Room(); var actor = sim.AddBot(64, 0);
        Assert.Equal(2, ActorJumpActions.JumpIfInventory(actor, "Clip", 50, 2, AcsActorPointer.Player1));
        Assert.Null(ActorJumpActions.JumpIfInventory(actor, "Clip", 50, 2));
        Assert.Null(ActorJumpActions.JumpIfInventory(actor, "Clip", 0, 2, AcsActorPointer.Null));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
