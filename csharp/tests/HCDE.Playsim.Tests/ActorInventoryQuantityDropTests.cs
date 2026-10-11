using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryQuantityDropTests
{
    [Theory]
    [InlineData(3, 3, 7)]
    [InlineData(100, 10, 0)]
    [InlineData(-5, 1, 9)]
    [InlineData(int.MinValue, 1, 9)]
    public void RequestedAmmoQuantityClampsToHeldAmount(int amount, int tossed, int remaining)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Bullets = 10;
        Assert.True(ActorInventoryActions.DropInventory(player, "Clip", amount));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
        Assert.Equal(tossed, drop.PickupAmount); Assert.Equal(remaining, player.Inventory.Bullets);
        Assert.True(drop.IgnoreAmmoSkill); Assert.Equal(30, drop.PickupDelay);
        var bytes = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(tossed, Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip).PickupAmount);
        Assert.Equal(remaining, player.Inventory.Bullets);
    }

    [Fact]
    public void HealthStackQuantityDropsWithoutUsingHealth()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Health = 30; ActorInventoryActions.SetInventory(player, "MedPatch", 5);
        Assert.True(ActorInventoryActions.DropInventory(player, "MedPatch", 3));
        Assert.Equal(2, player.Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(30, player.Health);
        Assert.Equal(3, Assert.Single(sim.Actors, actor => actor.PickupAmount == 3).PickupAmount);
    }

    [Fact]
    public void StateActionSpawnsQuantityThatCanBeCollectedAfterDelay()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Bullets = 10;
        player.States.Configure(player, [new(-1, 0), new(4, 0,
            Action: self => ActorInventoryActions.DropInventory(self, "Clip", 4))], 0);
        player.States.Enter(player, 1);
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
        drop.VelocityX = drop.VelocityY = drop.VelocityZ = default; drop.NoGravity = true;
        Assert.Equal(6, player.Inventory.Bullets);
        for (var i = 0; i < 30; i++) sim.Tick();
        Assert.Equal(10, player.Inventory.Bullets);
        Assert.DoesNotContain(drop, sim.Actors);
    }

    [Fact]
    public void ZeroAndFailedSpawnsLeaveInventoryUnchanged()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.False(ActorInventoryActions.DropInventory(player, "Clip", 0));
        Assert.False(ActorInventoryActions.DropInventory(new PlayerPawn(), "Clip", 3));
        Assert.False(ActorInventoryActions.DropInventory(player, "Fist", 3));
        Assert.False(ActorInventoryActions.DropInventory(player, "Unknown", 3));
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Single(sim.Actors);
    }

    [Fact]
    public void NativeUntossablePowerupsAreRetainedWithoutSpawning()
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorInventoryActions.GiveInventory(player, "PowerDamage");
        var duration = player.PowerDamageTics;
        Assert.False(ActorInventoryActions.DropInventory(player, "PowerDamage", 5));
        Assert.Equal(duration, player.PowerDamageTics);
        Assert.Single(sim.Actors);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(StrifeHealthPickups: true, InfiniteStoredInventoryUse: true));
}
