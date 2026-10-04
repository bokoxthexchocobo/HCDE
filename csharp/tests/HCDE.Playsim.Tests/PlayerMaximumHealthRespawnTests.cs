using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerMaximumHealthRespawnTests
{
    [Theory]
    [InlineData(SpawnGameMode.Cooperative, 250)]
    [InlineData(SpawnGameMode.Cooperative, -7)]
    [InlineData(SpawnGameMode.Deathmatch, 250)]
    [InlineData(SpawnGameMode.Deathmatch, -7)]
    [InlineData(SpawnGameMode.Single, 250)]
    [InlineData(SpawnGameMode.Single, -7)]
    public void SuccessfulRespawnClearsOverrideAndUsesSpawnHealth(SpawnGameMode mode, int maximum)
    {
        var sim = Room(mode); sim.AllowSinglePlayerRespawn = true;
        var player = sim.Players.Single();
        player.ResurrectionHealth = 80;
        AcsActorProperties.Set(sim, player, 0, AcsActorProperties.SpawnHealth, maximum);
        ActorDamage.Apply(player, 1000);
        Assert.Equal(maximum, player.MaxHealth);
        PressRespawn(sim);
        Assert.False(player.IsDead);
        Assert.Equal(80, player.Health);
        Assert.Equal(80, player.ResurrectionHealth);
        Assert.Equal(0, player.MaxHealth);
        Assert.Equal(100, AcsPlayerInventory.Count(player, "Health", true));
        AcsPlayerInventory.Give(player, "Health", 100);
        Assert.Equal(100, player.Health);
    }

    [Fact]
    public void BlockedRespawnPreservesOverrideUntilRespawnSucceeds()
    {
        var sim = Room(SpawnGameMode.Cooperative); sim.NoRespawn = true;
        var player = sim.Players.Single(); player.MaxHealth = 250;
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);
        Assert.True(player.IsDead);
        Assert.Equal(250, player.MaxHealth);
        Assert.True(player.RespawnArmed);
        sim.NoRespawn = false;
        sim.Tick();
        Assert.False(player.IsDead);
        Assert.Equal(0, player.MaxHealth);
    }

    [Fact]
    public void SinglePlayerReloadRequestPreservesCorpseOverride()
    {
        var sim = Room(SpawnGameMode.Single);
        var player = sim.Players.Single(); player.MaxHealth = 250;
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);
        Assert.True(sim.ReloadRequested);
        Assert.True(player.IsDead);
        Assert.Equal(250, player.MaxHealth);
    }

    private static void PressRespawn(AuthoritySimulation sim)
    {
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
    }

    private static AuthoritySimulation Room(SpawnGameMode mode) => AuthoritySimulation.Start(new PlayLevel
    {
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, Single = true, Coop = true, Deathmatch = true }],
    }, spawnOptions: new SpawnOptions(Mode: mode));
}
