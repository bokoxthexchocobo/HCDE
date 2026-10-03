using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlockmapSpawnStompTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RespawnStompSkipsBlockmapExcludedMonster(bool excluded)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, Coop = true }],
        }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
        var player = Assert.Single(sim.Players);
        var monster = sim.AddBot(0, 0);
        monster.Brain!.Enabled = false; monster.Solid = false; monster.NoBlockmap = excluded;
        var health = monster.Health;
        ActorDamage.Apply(player, 1000);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
        Assert.False(player.IsDead);
        Assert.Equal(!excluded, monster.IsDead);
        if (excluded) Assert.Equal(health, monster.Health);
    }
}
