namespace HCDE.Playsim.Tests;

public class ActorAmmoCapacityActionTests
{
    [Theory]
    [InlineData("Clip", 200)]
    [InlineData("Shell", 50)]
    [InlineData("RocketAmmo", 50)]
    [InlineData("Cell", 300)]
    public void ParentCapacityCanChangeWithoutDepletingAmmo(string typeName, int initial)
    {
        var player = new PlayerPawn();
        Assert.Equal(initial, ActorInventoryActions.GetAmmoCapacity(player, typeName));
        Assert.True(ActorInventoryActions.SetInventory(player, typeName, 40));
        ActorInventoryActions.SetAmmoCapacity(player, typeName, 7);
        Assert.Equal(7, ActorInventoryActions.GetAmmoCapacity(player, typeName));
        Assert.True(ActorInventoryActions.CheckInventory(player, typeName, 40));
        ActorInventoryActions.SetAmmoCapacity(player, typeName, -1);
        Assert.Equal(-1, ActorInventoryActions.GetAmmoCapacity(player, typeName));
        Assert.True(ActorInventoryActions.CheckInventory(player, typeName, 40));
    }

    [Theory]
    [InlineData("ClipBox", "Clip", 200)]
    [InlineData("ShellBox", "Shell", 50)]
    [InlineData("RocketBox", "RocketAmmo", 50)]
    [InlineData("CellPack", "Cell", 300)]
    public void UnheldBoxQueryUsesClassDefaultRatherThanParentCapacity(string box, string parent, int maximum)
    {
        var player = new PlayerPawn();
        ActorInventoryActions.SetAmmoCapacity(player, parent, 17);
        Assert.Equal(maximum, ActorInventoryActions.GetAmmoCapacity(player, box));
        Assert.Throws<NotSupportedException>(() => ActorInventoryActions.SetAmmoCapacity(player, box, 9));
        Assert.Equal(17, ActorInventoryActions.GetAmmoCapacity(player, parent));
        Assert.Equal(maximum, ActorInventoryActions.GetAmmoCapacity(new Actor(), box));
    }

    [Fact]
    public void StateCallbackSetsCapacityWithoutChangingFrameOrAmmo()
    {
        var player = new PlayerPawn();
        player.States.Configure(player, [new(-1, 0), new(4, 0,
            Action: self => ActorInventoryActions.SetAmmoCapacity(self, "Clip", 20))], 0);
        player.States.Enter(player, 1);
        Assert.Equal(1, player.States.Current);
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(20, player.Inventory.MaxBullets);
        Assert.False(ActorInventoryActions.GiveAmmo(player, "Clip", 1));
    }

    [Fact]
    public void InvalidClassesAndDestroyedReceiversDoNotChangeCapacity()
    {
        var player = new PlayerPawn();
        ActorInventoryActions.SetAmmoCapacity(player, "Pistol", 1);
        ActorInventoryActions.SetAmmoCapacity(player, null, 1);
        Assert.Equal(0, ActorInventoryActions.GetAmmoCapacity(player, "Pistol"));
        Assert.Equal(200, player.Inventory.MaxBullets);
        player.Destroy();
        ActorInventoryActions.SetAmmoCapacity(player, "Clip", 1);
        Assert.Equal(200, player.Inventory.MaxBullets);
        Assert.Equal(0, ActorInventoryActions.GetAmmoCapacity(player, "ClipBox"));
    }
}
