using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorBackpackSetTests
{
    [Fact]
    public void SetterCreationIgnoresSkillAmmoScaling()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            spawnOptions: new SpawnOptions(Skill: 0));
        var player = sim.Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "Backpack", 1));
        Assert.Equal(60, player.Inventory.Bullets); Assert.Equal(4, player.Inventory.Shells);
    }
    [Fact]
    public void MissingPackNonpositiveRequestsSucceedWithoutAmmoOrCapacityChanges()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "Backpack", 0));
        Assert.True(ActorInventoryActions.SetInventory(player, "Backpack", -1));
        Assert.False(player.Inventory.HasBackpack); Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(200, player.Inventory.MaxBullets);
    }

    [Fact]
    public void NewPackUsesPickupAndExistingGrowthDoesNotRepeatAmmoGrant()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "bAcKpAcK", 100));
        Assert.True(player.Inventory.HasBackpack); Assert.Equal(60, player.Inventory.Bullets);
        Assert.False(ActorInventoryActions.SetInventory(player, "Backpack", 1));
        Assert.True(ActorInventoryActions.SetInventory(player, "Backpack", int.MaxValue));
        Assert.Equal(60, player.Inventory.Bullets);
        Assert.Throws<NotSupportedException>(() => ActorInventoryActions.SetInventory(player, "Backpack", 2, beyondMax: true));
        Assert.True(player.Inventory.HasBackpack); Assert.Equal(60, player.Inventory.Bullets);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public void SetterRemovalResetsCapsAndClampsAmmo(int amount)
    {
        var player = Room().Players.Single(); player.Inventory.GiveBackpack(); player.Inventory.Bullets = 350;
        Assert.True(ActorInventoryActions.SetInventory(player, "Backpack", amount));
        Assert.False(player.Inventory.HasBackpack); Assert.Equal(200, player.Inventory.Bullets);
        Assert.Equal(200, player.Inventory.MaxBullets);
    }

    [Fact]
    public void StateCallbackSelectsMasterAndRestorationKeepsPack()
    {
        var sim = Room(); var player = sim.Players.Single(); var caller = sim.AddBot(20, 0);
        caller.MasterId = player.Id; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "Backpack", 1, 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result);
        var saved = SimSavegame.Write(sim); player.Inventory.RemoveBackpack(); SimSavegame.Apply(sim, saved);
        Assert.True(player.Inventory.HasBackpack); Assert.Equal(400, player.Inventory.MaxBullets);
        Assert.False(ActorInventoryActions.SetInventory(player, "Backpack", 0, pointerSelector: 1));
        Assert.False(ActorInventoryActions.SetInventory(caller, "Backpack", 1));
        player.Destroy(); Assert.False(ActorInventoryActions.SetInventory(player, "Backpack", 0));
        Assert.True(player.Inventory.HasBackpack);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
