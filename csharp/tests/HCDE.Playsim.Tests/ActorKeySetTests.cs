using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorKeySetTests
{
    [Theory]
    [InlineData("BlueCard")]
    [InlineData("BlueSkull")]
    [InlineData("RedCard")]
    [InlineData("RedSkull")]
    [InlineData("YellowCard")]
    [InlineData("YellowSkull")]
    public void SetterUsesRepresentedSingleKeyCreationAndDepletion(string name)
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, name, 0));
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.True(ActorInventoryActions.SetInventory(player, name.ToLowerInvariant(), 1));
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, false));
        Assert.False(ActorInventoryActions.SetInventory(player, name, 1));
        Assert.True(ActorInventoryActions.SetInventory(player, name, int.MaxValue));
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, false));
        Assert.Throws<NotSupportedException>(() => ActorInventoryActions.SetInventory(player, name, 2, beyondMax: true));
        Assert.True(ActorInventoryActions.SetInventory(player, name, int.MinValue));
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(50, player.Inventory.Bullets); Assert.Equal(100, player.Health);
    }

    [Fact]
    public void UpdatesPreserveOtherColorsAndSaveRestoresCreatedKey()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.BlueKey = true;
        Assert.True(ActorInventoryActions.SetInventory(player, "RedCard", 1));
        var saved = SimSavegame.Write(sim); player.Inventory.RedKey = false; SimSavegame.Apply(sim, saved);
        Assert.True(player.Inventory.RedKey); Assert.True(player.Inventory.BlueKey);
        Assert.True(ActorInventoryActions.SetInventory(player, "RedCard", -1));
        Assert.True(player.Inventory.BlueKey); Assert.False(player.Inventory.RedKey);
    }

    [Fact]
    public void StateCallbackUsesMasterAndInvalidReceiversPreserveKeys()
    {
        var sim = Room(); var player = sim.Players.Single(); var caller = sim.AddBot(20, 0);
        caller.MasterId = player.Id; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "YellowCard", 1, 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result); Assert.True(player.Inventory.YellowKey);
        Assert.False(ActorInventoryActions.SetInventory(player, "YellowCard", 0, pointerSelector: 1));
        Assert.False(ActorInventoryActions.SetInventory(caller, "YellowCard", 1));
        player.Destroy(); Assert.False(ActorInventoryActions.SetInventory(player, "YellowCard", 0));
        Assert.True(player.Inventory.YellowKey);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
