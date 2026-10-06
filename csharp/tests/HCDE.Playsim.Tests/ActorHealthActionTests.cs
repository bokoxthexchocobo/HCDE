using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorHealthActionTests
{
    [Theory]
    [InlineData(-100, 1)]
    [InlineData(0, 1)]
    [InlineData(25, 25)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void SetHealthClampsWithoutDamageOrStateChange(int requested, int expected)
    {
        var actor = new Actor(); var state = actor.States.Current;
        ActorHealthActions.SetHealth(actor, requested);
        Assert.Equal(expected, actor.Health); Assert.Equal(state, actor.States.Current);
        Assert.Equal(0, actor.DeathCount); Assert.False(actor.Killed);
    }

    [Fact]
    public void SettingDeadActorHealthDoesNotResurrectOrClearKilled()
    {
        var sim = Room(); var target = sim.AddBot(20, 0, 3001); ActorDamage.Apply(target, 1000);
        var state = target.States.Current; var tics = target.States.RemainingTics;
        ActorHealthActions.SetHealth(target, 40);
        Assert.Equal(40, target.Health); Assert.True(target.Killed);
        Assert.Equal(state, target.States.Current); Assert.Equal(tics, target.States.RemainingTics);
    }

    [Fact]
    public void ResetUsesMonsterSpawnHealthAndSkipsDeadActors()
    {
        var sim = Room(); var target = sim.AddBot(20, 0, 3001); target.Health = 10;
        ActorHealthActions.ResetHealth(target); Assert.Equal(target.SpawnHealth(), target.Health);
        target.Health = -10; var state = target.States.Current;
        ActorHealthActions.ResetHealth(target); Assert.Equal(-10, target.Health); Assert.Equal(state, target.States.Current);
    }

    [Fact]
    public void PlayerResetUsesDefaultHealthRatherThanUpgradeMaximum()
    {
        var sim = Room(); var player = sim.Players.Single(); player.MaxHealth = 200; player.Stamina = 50;
        ActorHealthActions.SetHealth(player, 150); ActorHealthActions.ResetHealth(player);
        Assert.Equal(100, player.Health); Assert.Equal(200, player.MaxHealth); Assert.Equal(50, player.Stamina);
    }

    [Fact]
    public void PointerActionChangesTargetWithoutChangingCallerFrame()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var player = sim.Players.Single();
        caller.Brain!.SetSpecialTarget(player);
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => ActorHealthActions.SetHealth(self, 42, 2))], 0);
        caller.States.Enter(caller, 1); Assert.Equal(42, player.Health); Assert.Equal(1, caller.States.Current);
        ActorHealthActions.ResetHealth(caller, 2); Assert.Equal(100, player.Health);
        ActorHealthActions.SetHealth(caller, 1, 1); Assert.Equal(100, player.Health);
        player.Destroy(); ActorHealthActions.SetHealth(caller, 1, 2); Assert.Equal(100, player.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
