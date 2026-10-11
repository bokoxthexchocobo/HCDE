using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPowerupTakeTests
{
    [Theory]
    [InlineData("PowerDamage", 0)]
    [InlineData("PowerBuddha", int.MinValue)]
    [InlineData("PowerProtection", -1)]
    public void RemovalEndsOnlyRequestedPowerAndReportsPriorOwnership(string name, int amount)
    {
        var player = Room().Players.First(); Prepare(player);
        Assert.True(ActorInventoryActions.TakeInventory(player, name.ToLowerInvariant(), amount, flags: 1));
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, true));
        Assert.Equal(200, player.PowerDamageTics + player.PowerBuddhaTics + player.PowerProtectionTics);
        Assert.False(ActorInventoryActions.TakeInventory(player, name));
        Assert.Equal(100, player.Health);
    }

    [Fact]
    public void TargetStateCallbackUsesMasterAndRejectsInvalidReceivers()
    {
        var sim = Room(); var player = sim.Players.First(); Prepare(player);
        var caller = sim.AddBot(20, 0); var target = sim.AddBot(40, 0);
        caller.Brain!.SetSpecialTarget(target); target.MasterId = player.Id; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.TakeFromTarget(self, "PowerBuddha", pointerSelector: 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result); Assert.Equal(0, player.PowerBuddhaTics);
        Assert.False(ActorInventoryActions.TakeInventory(player, "PowerDamage", pointerSelector: 1));
        Assert.False(ActorInventoryActions.TakeInventory(caller, "PowerDamage"));
        player.Destroy(); Assert.False(ActorInventoryActions.TakeInventory(player, "PowerDamage"));
        Assert.Equal(100, player.PowerDamageTics);
    }

    [Fact]
    public void FamilyRemovalCountsActivePowersAndExcludesCallerAndUnrelatedPlayers()
    {
        var sim = Room(); var players = sim.Players.ToArray(); var parent = sim.AddBot(20, 0);
        players[0].MasterId = players[1].MasterId = parent.Id;
        foreach (var player in players) Prepare(player);
        Assert.Equal(1, ActorInventoryActions.TakeFromSiblings(players[0], "PowerProtection"));
        Assert.Equal(100, players[0].PowerProtectionTics); Assert.Equal(100, players[2].PowerProtectionTics);
        Assert.Equal(1, ActorInventoryActions.TakeFromChildren(parent, "PowerProtection"));
        Assert.Equal(0, ActorInventoryActions.TakeFromChildren(parent, "PowerProtection"));
    }

    [Fact]
    public void RemovedPowerRemainsInactiveAfterSave()
    {
        var sim = Room(); var player = sim.Players.First(); Prepare(player);
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerDamage"));
        var saved = SimSavegame.Write(sim); player.PowerDamageTics = 100; SimSavegame.Apply(sim, saved);
        Assert.Equal(0, player.PowerDamageTics); Assert.Equal(100, player.PowerBuddhaTics);
        Assert.Equal(100, player.PowerProtectionTics);
    }

    private static void Prepare(PlayerPawn player) =>
        player.PowerDamageTics = player.PowerBuddhaTics = player.PowerProtectionTics = 100;

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative, InfiniteStoredInventoryUse: true));
}
