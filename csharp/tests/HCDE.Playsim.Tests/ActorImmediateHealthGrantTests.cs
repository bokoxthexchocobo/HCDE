using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorImmediateHealthGrantTests
{
    [Theory]
    [InlineData("Health", 2, 32)]
    [InlineData("Stimpack", 2, 50)]
    [InlineData("Medikit", 2, 80)]
    [InlineData("Stimpack", 0, 40)]
    [InlineData("Medikit", -2, 55)]
    public void ImmediateClassesMultiplyDefaultHealingRatherThanStoreItems(string name, int amount, int expected)
    {
        var player = Room().Players.Single(); player.Health = 30;
        Assert.True(ActorInventoryActions.GiveInventory(player, name.ToLowerInvariant(), amount));
        Assert.Equal(expected, player.Health); Assert.Empty(player.Inventory.HealthPickups);
        player.Health = 100; Assert.False(ActorInventoryActions.GiveInventory(player, name));
    }

    [Fact]
    public void NativeSignedMultiplicationCanBecomePercentageHealing()
    {
        var player = Room().Players.Single(); player.Health = 30;
        Assert.True(ActorInventoryActions.GiveInventory(player, "Stimpack", 429496720));
        Assert.Equal(96, player.Health);
    }

    [Fact]
    public void MonsterRecipientsUseTheirOwnHealthMaximumAndFamilyCountsSuccesses()
    {
        var sim = Room(); var parent = sim.AddBot(20, 0); var injured = sim.AddBot(40, 0, 3001);
        var full = sim.AddBot(60, 0, 3001); injured.MasterId = parent.Id; full.MasterId = parent.Id;
        var maximum = full.Health; injured.Health = 1;
        Assert.Equal(1, ActorInventoryActions.GiveToChildren(parent, "Stimpack", 2));
        Assert.Equal(Math.Min(21, maximum), injured.Health); Assert.Equal(maximum, full.Health);
        Assert.Equal(1, ActorInventoryActions.GiveToChildren(parent, "Stimpack", 100));
        Assert.Equal(maximum, injured.Health);
        Assert.Equal(0, ActorInventoryActions.GiveToChildren(parent, "Stimpack"));
    }

    [Fact]
    public void StateCallbackUsesHealthFactorAndClassIdentityUnderStrifeMapping()
    {
        var sim = Room(strife: true, healthFactor: 2); var caller = sim.AddBot(20, 0);
        var player = sim.Players.Single(); player.Health = 30; caller.Brain!.SetSpecialTarget(player);
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.GiveToTarget(self, "Medikit", 1))], 0);
        caller.States.Enter(caller, 1);
        Assert.True(result); Assert.Equal(80, player.Health); Assert.Empty(player.Inventory.HealthPickups);
        Assert.True(ActorInventoryActions.GiveToTarget(caller, "MedicalKit", 1));
        Assert.Equal(80, player.Health); Assert.Equal(1, player.Inventory.CountHealthPickup("MedicalKit"));
        player.Health = 0; Assert.False(ActorInventoryActions.GiveToTarget(caller, "Medikit"));
    }

    private static AuthoritySimulation Room(bool strife = false, double healthFactor = 1) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(StrifeHealthPickups: strife, HealthFactor: healthFactor));
}
