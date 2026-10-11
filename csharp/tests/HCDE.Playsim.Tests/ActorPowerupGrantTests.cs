using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPowerupGrantTests
{
    [Theory]
    [InlineData("PowerDamage", PlayerPawn.DamagePowerDuration)]
    [InlineData("PowerProtection", PlayerPawn.DamagePowerDuration)]
    [InlineData("PowerBuddha", PlayerPawn.PowerBuddhaDuration)]
    public void GrantUsesDefaultDurationAndNativeBlinkReplenishment(string name, int duration)
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, name.ToLowerInvariant(), 100));
        Assert.Equal(duration, Duration(player, name));
        Assert.False(ActorInventoryActions.GiveInventory(player, name, 0));
        SetDuration(player, name, PlayerPawn.PowerBuddhaBlinkThreshold + 1);
        Assert.False(ActorInventoryActions.GiveInventory(player, name, 1));
        SetDuration(player, name, PlayerPawn.PowerBuddhaBlinkThreshold);
        Assert.True(ActorInventoryActions.GiveInventory(player, name, -1));
        Assert.Equal(duration, Duration(player, name));
    }

    [Fact]
    public void TargetStateCallbackAndSavePreserveGrantedPower()
    {
        var sim = Room(); var player = sim.Players.Single(); var caller = sim.AddBot(20, 0);
        caller.Brain!.SetSpecialTarget(player); var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.GiveToTarget(self, "PowerDamage"))], 0);
        caller.States.Enter(caller, 1); Assert.True(result);
        var saved = SimSavegame.Write(sim); player.PowerDamageTics = 0; SimSavegame.Apply(sim, saved);
        Assert.Equal(PlayerPawn.DamagePowerDuration, player.PowerDamageTics);
        Assert.False(ActorInventoryActions.GiveInventory(player, "PowerProtection", pointerSelector: 1));
        Assert.False(ActorInventoryActions.GiveInventory(caller, "PowerProtection"));
        player.Destroy(); Assert.False(ActorInventoryActions.GiveInventory(player, "PowerProtection"));
        Assert.Equal(0, player.PowerProtectionTics);
    }

    [Fact]
    public void FamilyGrantCountsEligiblePowersAndExcludesCaller()
    {
        var sim = Room(); var player = sim.Players.Single(); var parent = sim.AddBot(20, 0);
        var sibling = sim.AddBot(40, 0); player.MasterId = sibling.MasterId = parent.Id;
        Assert.Equal(1, ActorInventoryActions.GiveToChildren(parent, "PowerBuddha"));
        Assert.Equal(0, ActorInventoryActions.GiveToSiblings(sibling, "PowerBuddha"));
        player.PowerBuddhaTics = 1;
        Assert.Equal(1, ActorInventoryActions.GiveToSiblings(sibling, "PowerBuddha"));
        Assert.Equal(PlayerPawn.PowerBuddhaDuration, player.PowerBuddhaTics);
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
