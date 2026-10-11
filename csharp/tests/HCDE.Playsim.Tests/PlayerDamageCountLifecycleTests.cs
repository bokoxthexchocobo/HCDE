using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerDamageCountLifecycleTests
{
    [Theory]
    [InlineData(SpawnGameMode.Cooperative)]
    [InlineData(SpawnGameMode.Deathmatch)]
    public void RebornPlayerClearsDamageFlash(SpawnGameMode mode)
    {
        var sim = Room(mode); var player = sim.Players.Single();
        ActorDamage.Apply(player, 200, flags: DamageFlags.Forced);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        Assert.True(player.DamageCount > 0);
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.False(player.IsDead); Assert.Equal(0, player.DamageCount);
    }

    [Fact]
    public void FreezeDeathClearsDamageFlashAndSaveKeepsItCleared()
    {
        var sim = Room(SpawnGameMode.Cooperative); var player = sim.Players.Single();
        ActorDamage.Apply(player, 7); Assert.Equal(7, player.DamageCount);
        ActorFreezeActions.FreezeDeath(player); Assert.Equal(0, player.DamageCount);
        var bytes = SimSavegame.Write(sim); player.DamageCount = 50;
        SimSavegame.Apply(sim, bytes); Assert.Equal(0, player.DamageCount);
    }

    private static AuthoritySimulation Room(SpawnGameMode mode) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, Coop = true, Deathmatch = true }] },
        spawnOptions: new SpawnOptions(Mode: mode));
}
