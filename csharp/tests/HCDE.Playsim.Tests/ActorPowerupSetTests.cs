using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPowerupSetTests
{
    [Theory]
    [InlineData("PowerDamage", PlayerPawn.DamagePowerDuration)]
    [InlineData("PowerProtection", PlayerPawn.DamagePowerDuration)]
    [InlineData("PowerBuddha", PlayerPawn.PowerBuddhaDuration)]
    public void SetterChangesRepresentedOwnershipWithoutReplenishingDuration(string name, int duration)
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, name, 0));
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.True(ActorInventoryActions.SetInventory(player, name.ToLowerInvariant(), 100));
        Assert.Equal(duration, Duration(player, name));
        SetDuration(player, name, 1);
        Assert.False(ActorInventoryActions.SetInventory(player, name, 1));
        Assert.True(ActorInventoryActions.SetInventory(player, name, 100));
        Assert.Equal(1, Duration(player, name));
        Assert.Throws<NotSupportedException>(() => ActorInventoryActions.SetInventory(player, name, 2, beyondMax: true));
        Assert.Equal(1, Duration(player, name));
        Assert.True(ActorInventoryActions.SetInventory(player, name, int.MinValue));
        Assert.Equal(0, Duration(player, name));
    }

    [Fact]
    public void StateCallbackUsesMasterAndSavePreservesPower()
    {
        var sim = Room(); var player = sim.Players.Single(); var caller = sim.AddBot(20, 0);
        caller.MasterId = player.Id; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "PowerBuddha", 1, 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result);
        var saved = SimSavegame.Write(sim); player.PowerBuddhaTics = 0; SimSavegame.Apply(sim, saved);
        Assert.Equal(PlayerPawn.PowerBuddhaDuration, player.PowerBuddhaTics);
        Assert.False(ActorInventoryActions.SetInventory(player, "PowerBuddha", 0, AcsActorPointer.Null));
        Assert.False(ActorInventoryActions.SetInventory(caller, "PowerBuddha", 1));
        player.Destroy(); Assert.False(ActorInventoryActions.SetInventory(player, "PowerBuddha", 0));
        Assert.Equal(PlayerPawn.PowerBuddhaDuration, player.PowerBuddhaTics);
    }

    [Fact]
    public void DepletionPreservesOtherPowersAndPlayerHealth()
    {
        var player = Room().Players.Single(); player.GivePowerBuddha(); player.GivePowerProtection();
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerBuddha", -1));
        Assert.Equal(0, player.PowerBuddhaTics); Assert.Equal(PlayerPawn.DamagePowerDuration, player.PowerProtectionTics);
        Assert.Equal(100, player.Health);
    }

    private static int Duration(PlayerPawn player, string name) => name switch
    { "PowerDamage" => player.PowerDamageTics, "PowerProtection" => player.PowerProtectionTics, _ => player.PowerBuddhaTics };
    private static void SetDuration(PlayerPawn player, string name, int value)
    {
        if (name == "PowerDamage") player.PowerDamageTics = value;
        else if (name == "PowerProtection") player.PowerProtectionTics = value;
        else player.PowerBuddhaTics = value;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
