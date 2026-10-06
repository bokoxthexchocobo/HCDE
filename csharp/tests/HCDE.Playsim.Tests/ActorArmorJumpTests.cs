using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorArmorJumpTests
{
    [Theory]
    [InlineData("GreenArmor", 49, true)]
    [InlineData("greenarmor", 50, true)]
    [InlineData("GreenArmor", 51, false)]
    [InlineData("BlueArmor", 1, false)]
    [InlineData("GreenArmor", 0, true)]
    [InlineData("GreenArmor", -1, true)]
    public void ArmorJumpUsesWornTypeAndInclusiveAmount(string type, int amount, bool jumps)
    {
        var actor = Room().Players.Single(); actor.Inventory.Armor = 50;
        actor.Inventory.ArmorType = "GreenArmor";
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.JumpIfArmorType(self, type, 2, amount)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(jumps ? 2 : 1, actor.States.Current);
        Assert.Equal(50, actor.Inventory.Armor);
    }

    [Fact]
    public void DestroyedPlayersAndNonplayersDoNotMatch()
    {
        Assert.False(ActorJumpActions.CheckArmorType(new Actor(), "None", 0));
        var actor = Room().Players.Single(); actor.Destroy();
        Assert.False(ActorJumpActions.CheckArmorType(actor, "None", 0));
    }

    [Fact]
    public void DepletedArmorCannotMeetPositiveThreshold()
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.Inventory.Armor = 50; actor.Inventory.ArmorType = "GreenArmor";
        Assert.Equal(2, ActorJumpActions.JumpIfArmorType(actor, "GreenArmor", 2, 50));
        actor.Inventory.Armor = 0;
        Assert.Null(ActorJumpActions.JumpIfArmorType(actor, "GreenArmor", 2));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
