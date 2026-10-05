using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DamagePowerRemovalTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InventoryClearAndPistolStartStopBothEffects(bool pistolStart)
    {
        var player = Powered();
        if (pistolStart) player.Inventory.ResetToPistolStart(); else AcsPlayerInventory.Clear(player);
        AssertCleared(player);
        Assert.Equal(20, ActorDamage.Apply(new Actor { Health = 100 }, 20, player).HealthLost);
        Assert.Equal(20, ActorDamage.Apply(player, 20).HealthLost);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeathClearsPowersForDamageAndDirectHealthChanges(bool direct)
    {
        var player = Powered();
        if (direct) player.Health = 0;
        else ActorDamage.Apply(player, 1000, flags: DamageFlags.Forced);
        Assert.True(player.IsDead);
        AssertCleared(player);
    }

    [Theory]
    [InlineData(SpawnGameMode.Cooperative)]
    [InlineData(SpawnGameMode.Deathmatch)]
    public void RespawnCannotRestoreExpiredDeathPowers(SpawnGameMode mode)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, Single = true, Coop = true, Deathmatch = true }],
        }, spawnOptions: new SpawnOptions(Mode: mode));
        var player = sim.Players.Single();
        player.GivePowerDamage(); player.GivePowerProtection();
        ActorDamage.Apply(player, 1000, flags: DamageFlags.Forced);
        AssertCleared(player);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.False(player.IsDead);
        AssertCleared(player);
    }

    private static PlayerPawn Powered()
    {
        var player = new PlayerPawn { Health = 100 };
        player.GivePowerDamage(); player.GivePowerProtection();
        return player;
    }

    private static void AssertCleared(PlayerPawn player)
    {
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, "PowerDamage"));
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, "PowerProtection"));
    }
}
