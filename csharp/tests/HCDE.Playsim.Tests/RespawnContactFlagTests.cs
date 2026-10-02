using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RespawnContactFlagTests
{
    [Theory]
    [InlineData(SpawnGameMode.Single)]
    [InlineData(SpawnGameMode.Cooperative)]
    [InlineData(SpawnGameMode.Deathmatch)]
    public void RebirthRestoresPlayerContactDefaultsAndAllowsCollection(SpawnGameMode mode)
    {
        var sim = Room(mode); sim.AllowSinglePlayerRespawn = true;
        var player = sim.Players.Single();
        player.CanPickupItems = false; player.SpecialPickup = true;
        player.Health = 0;
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.False(player.IsDead); Assert.True(player.CanPickupItems); Assert.False(player.SpecialPickup);
        var pickup = sim.Actors.Single(actor => actor.DoomEdNum == PickupCatalog.Clip);
        var before = player.Inventory.Bullets; player.X = pickup.X; player.Y = pickup.Y;
        sim.Tick(); Assert.True(pickup.Destroyed); Assert.Equal(before + 10, player.Inventory.Bullets);
    }

    [Fact]
    public void BlockedRespawnPreservesExistingFlags()
    {
        var sim = Room(SpawnGameMode.Cooperative); sim.NoRespawn = true;
        var player = sim.Players.Single(); player.CanPickupItems = false; player.SpecialPickup = true;
        player.Health = 0;
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.True(player.IsDead); Assert.False(player.CanPickupItems); Assert.True(player.SpecialPickup);
    }

    private static AuthoritySimulation Room(SpawnGameMode mode) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, Single = true, Coop = true, Deathmatch = true },
            new LevelThing { Type = PickupCatalog.Clip, X = 100, Single = true, Coop = true, Deathmatch = true }],
    }, spawnOptions: new SpawnOptions(Mode: mode));
}
