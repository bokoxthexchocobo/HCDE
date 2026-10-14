using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerInverseLightTests
{
    [Fact]
    public void InverseActionUsesUnclampedNativeMarkerAsStateCallback()
    {
        var player = new PlayerPawn();
        player.States.Configure(player, [new(-1, 0, ActorWeaponLightActions.LightInverse)], 0);
        Assert.Equal(int.MinValue, player.ExtraLight);
        ActorWeaponLightActions.Light(player, int.MinValue); Assert.Equal(-20, player.ExtraLight);
        ActorWeaponLightActions.LightInverse(new Actor());
    }

    [Fact]
    public void InverseMarkerRoundTripsAndCanBeCleared()
    {
        var sim = Room(SpawnGameMode.Cooperative); var player = sim.Players.Single();
        ActorWeaponLightActions.LightInverse(player); var saved = SimSavegame.Write(sim, true);
        ActorWeaponLightActions.Light0(player); SimSavegame.Apply(sim, saved);
        Assert.Equal(int.MinValue, player.ExtraLight); Assert.Equal(saved, SimSavegame.Write(sim, true));
        ActorWeaponLightActions.Light0(player); Assert.Equal(0, player.ExtraLight);
    }

    [Theory]
    [InlineData(SpawnGameMode.Cooperative, 2)]
    [InlineData(SpawnGameMode.Cooperative, int.MinValue)]
    [InlineData(SpawnGameMode.Deathmatch, -20)]
    public void RespawnClearsPreviousLifeLight(SpawnGameMode mode, int light)
    {
        var sim = Room(mode); var player = sim.Players.Single(); player.ExtraLight = light;
        ActorDamage.Apply(player, 1000); Assert.True(player.IsDead);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        Assert.Equal(light, player.ExtraLight);
        var saved = SimSavegame.Write(sim, true);
        void Respawn() { sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick(); }
        Respawn(); Assert.False(player.IsDead); Assert.Equal(0, player.ExtraLight);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Assert.Equal(light, player.ExtraLight);
        Respawn(); Assert.Equal(0, player.ExtraLight);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room(SpawnGameMode mode) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, Single = true, Coop = true, Deathmatch = true }]
    }, spawnOptions: new SpawnOptions(Mode: mode));
}
