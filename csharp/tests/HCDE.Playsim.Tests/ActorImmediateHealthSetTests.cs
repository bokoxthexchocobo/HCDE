using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorImmediateHealthSetTests
{
    [Theory]
    [InlineData("Health")]
    [InlineData("Stimpack")]
    [InlineData("Medikit")]
    [InlineData("HealthBonus")]
    [InlineData("Soulsphere")]
    [InlineData("MegasphereHealth")]
    public void SetterAssignsRequestedHealingRatherThanMultiplyingDefault(string name)
    {
        var player = Room().Players.Single(); player.Health = 30;
        Assert.True(ActorInventoryActions.SetInventory(player, name.ToLowerInvariant(), 3));
        Assert.Equal(33, player.Health); Assert.Empty(player.Inventory.HealthPickups);
        Assert.True(ActorInventoryActions.SetInventory(player, name, 0));
        Assert.True(ActorInventoryActions.SetInventory(player, name, -1)); Assert.Equal(33, player.Health);
    }

    [Fact]
    public void SetterUsesHealthCapsAndDoesNotTreatBeyondMaximumAsHealingOverride()
    {
        var player = Room().Players.Single(); player.Health = 95;
        Assert.True(ActorInventoryActions.SetInventory(player, "Medikit", 50, beyondMax: true));
        Assert.Equal(100, player.Health);
        Assert.False(ActorInventoryActions.SetInventory(player, "Medikit", 1));
        Assert.True(ActorInventoryActions.SetInventory(player, "Soulsphere", 500)); Assert.Equal(200, player.Health);
        Assert.False(ActorInventoryActions.SetInventory(player, "Soulsphere", 1));
        player.Health = 0; Assert.False(ActorInventoryActions.SetInventory(player, "Health", 1));
    }

    [Fact]
    public void StateCallbackUsesMasterAndHealthFactorThenSurvivesSave()
    {
        var sim = Room(2); var player = sim.Players.Single(); player.Health = 30;
        var caller = sim.AddBot(20, 0); caller.MasterId = player.Id; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "Stimpack", 3, 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result); Assert.Equal(36, player.Health);
        var saved = SimSavegame.Write(sim); player.Health = 10; SimSavegame.Apply(sim, saved); Assert.Equal(36, player.Health);
        Assert.False(ActorInventoryActions.SetInventory(player, "Health", 5, pointerSelector: 1));
        player.Destroy(); Assert.False(ActorInventoryActions.SetInventory(player, "Health", 5)); Assert.Equal(36, player.Health);
    }

    private static AuthoritySimulation Room(double factor = 1) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(HealthFactor: factor));
}
