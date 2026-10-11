using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDirectAmmoGrantTests
{
    [Fact]
    public void NegativeNativeAmountsAreExplicitlyUnsupportedWithoutMutation()
    {
        var player = new PlayerPawn();
        Assert.Throws<NotSupportedException>(() => ActorInventoryActions.GiveAmmo(player, "Clip", -1));
        Assert.Equal(50, player.Inventory.Bullets);
    }

    [Theory]
    [InlineData("Clip", AmmoKind.Bullets)]
    [InlineData("ClipBox", AmmoKind.Bullets)]
    [InlineData("Shell", AmmoKind.Shells)]
    [InlineData("ShellBox", AmmoKind.Shells)]
    [InlineData("RocketAmmo", AmmoKind.Rockets)]
    [InlineData("RocketBox", AmmoKind.Rockets)]
    [InlineData("Cell", AmmoKind.Cells)]
    [InlineData("CellPack", AmmoKind.Cells)]
    public void QuantityIsAssignedInsteadOfUsingClassPickupDefault(string typeName, AmmoKind kind)
    {
        var player = new PlayerPawn();
        var before = player.Inventory.Ammo(kind);
        Assert.True(ActorInventoryActions.GiveAmmo(player, typeName, 3));
        Assert.Equal(before + 3, player.Inventory.Ammo(kind));
    }

    [Fact]
    public void ZeroQuantityDoesNotBecomeDefaultAndCapacityStillApplies()
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = 199;
        Assert.True(ActorInventoryActions.GiveAmmo(player, "ClipBox", 0));
        Assert.Equal(199, player.Inventory.Bullets);
        Assert.True(ActorInventoryActions.GiveAmmo(player, "Clip", 7));
        Assert.Equal(200, player.Inventory.Bullets);
        Assert.False(ActorInventoryActions.GiveAmmo(player, "Clip", 0));
        Assert.False(ActorInventoryActions.GiveAmmo(player, "Clip", 3));
    }

    [Fact]
    public void StateCallbackAppliesSkillScalingOnce()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            spawnOptions: new SpawnOptions(Skill: 0));
        var player = sim.Players.Single();
        var result = false;
        player.States.Configure(player, [new(-1, 0), new(4, 0,
            Action: self => result = ActorInventoryActions.GiveAmmo(self, "CellPack", 3))], 0);
        player.States.Enter(player, 1);
        Assert.True(result);
        Assert.Equal(6, player.Inventory.Cells);
        Assert.Equal(1, player.States.Current);
    }

    [Fact]
    public void InvalidClassAndUnsupportedOrDestroyedReceiverFail()
    {
        var player = new PlayerPawn();
        Assert.False(ActorInventoryActions.GiveAmmo(player, "Pistol", 3));
        Assert.False(ActorInventoryActions.GiveAmmo(player, null, 3));
        Assert.False(ActorInventoryActions.GiveAmmo(new Actor(), "Clip", 3));
        player.Destroy();
        Assert.False(ActorInventoryActions.GiveAmmo(player, "Clip", 3));
        Assert.Equal(50, player.Inventory.Bullets);
    }
}
