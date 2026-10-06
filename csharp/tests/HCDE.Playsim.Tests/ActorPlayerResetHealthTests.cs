using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPlayerResetHealthTests
{
    [Theory]
    [InlineData(50)]
    [InlineData(150)]
    [InlineData(300)]
    public void ResetUsesRetainedPlayerDefaultWithoutUpgradeScaling(int health)
    {
        var sim = Room(); var player = sim.Players.Single(); player.ResurrectionHealth = health;
        player.MaxHealth = 200; player.Stamina = 50; player.Health = 10;
        ActorHealthActions.ResetHealth(player);
        Assert.Equal(health, player.Health); Assert.Equal(200, player.MaxHealth); Assert.Equal(50, player.Stamina);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        player.Health = 20; sim.RestoreState(state); Assert.Equal(health, player.Health);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void SelectedPlayerResetsFromFrameWithoutResettingCaller()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ResurrectionHealth = 150; player.Health = 10;
        var caller = sim.AddBot(100, 0); caller.Brain!.SetSpecialTarget(player); var oldHealth = caller.Health;
        caller.States.Configure(caller, [new(-1, 0), new(-1, 0, Action: self => ActorHealthActions.ResetHealth(self, 2))], 0);
        caller.States.Enter(caller, 1); Assert.Equal(150, player.Health); Assert.Equal(oldHealth, caller.Health);
    }

    [Fact]
    public void DeadPlayerIsNotRevivedByReset()
    {
        var player = Room().Players.Single(); player.ResurrectionHealth = 150; player.Health = 0;
        var state = player.States.Current;
        ActorHealthActions.ResetHealth(player); Assert.Equal(0, player.Health); Assert.Equal(state, player.States.Current);
    }

    [Fact]
    public void DetachedPlayerWithoutRetainedDefaultsUsesVanillaDefault()
    {
        var player = new PlayerPawn { Health = 10 };
        ActorHealthActions.ResetHealth(player); Assert.Equal(100, player.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
