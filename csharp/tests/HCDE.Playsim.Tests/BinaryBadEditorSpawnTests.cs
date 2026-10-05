using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BinaryBadEditorSpawnTests
{
    [Theory]
    [InlineData(SpawnGameMode.Cooperative, 0x62, false)]
    [InlineData(SpawnGameMode.Cooperative, 0x162, true)]
    [InlineData(SpawnGameMode.Deathmatch, 0x62, false)]
    [InlineData(SpawnGameMode.Deathmatch, 0x162, true)]
    [InlineData(SpawnGameMode.Single, 0x12, true)]
    [InlineData(SpawnGameMode.Single, 0x112, true)]
    [InlineData(SpawnGameMode.Single, 0x72, true)]
    [InlineData(SpawnGameMode.Single, 0x10, false)]
    public void BadEditorFlagsAllowNativeMultiplayerMonsterSpawn(SpawnGameMode mode, short options, bool expected)
    {
        var core = new BinaryMapRecords([new MapThingRecord(500, 0, 0, 1, 2),
            new MapThingRecord(20, 0, 0, 3004, options)], [],
            [new MapSectorRecord(0, 128, "", "", 160, 0, 0)]);
        var map = new BinaryMap(core, new BinaryMapGeometry([], [], []),
            new BinaryMapSurface([], []), default, default);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromBinary(map, "MAP01"),
            spawnOptions: new SpawnOptions(Mode: mode));
        Assert.Equal(expected, sim.Actors.Any(actor => actor.DoomEdNum == 3004));
    }
}
