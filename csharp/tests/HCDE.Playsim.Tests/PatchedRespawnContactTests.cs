using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PatchedRespawnContactTests
{
    [Fact]
    public void DifferentRebirthDefaultsAffectChecksumEvenWhenCurrentFlagsMatch()
    {
        var level = new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] };
        var normal = AuthoritySimulation.Start(level);
        var patched = AuthoritySimulation.Start(level, dehacked: DehackedPatch.Apply("Thing 1\nBits = 0\n"));
        normal.Players.Single().CanPickupItems = false;
        normal.Tick(); patched.Tick();
        Assert.NotEqual(normal.Checksum, patched.Checksum);
    }
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, true)]
    [InlineData(2048, true, false)]
    [InlineData(2049, true, true)]
    public void RebirthRestoresPatchedDefaultsAfterRuntimeChanges(int bits, bool pickup, bool special)
    {
        var patch = DehackedPatch.Apply($"Thing 1\nBits = {bits}\n");
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, Coop = true },
                new LevelThing { Type = PickupCatalog.Clip, X = 100, Coop = true }],
        }, dehacked: patch, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
        var player = sim.Players.Single();
        Assert.Equal(pickup, player.CanPickupItems); Assert.Equal(special, player.SpecialPickup);
        player.CanPickupItems = !pickup; player.SpecialPickup = !special;
        // Restoring runtime flags must not replace the map's patched spawn defaults.
        SimSavegame.Apply(sim, SimSavegame.Write(sim));
        player.Health = 0;
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.False(player.IsDead);
        Assert.Equal(pickup, player.CanPickupItems); Assert.Equal(special, player.SpecialPickup);
        var item = sim.Actors.Single(actor => actor.DoomEdNum == PickupCatalog.Clip);
        player.X = item.X; player.Y = item.Y;
        sim.Tick(); Assert.Equal(pickup, item.Destroyed);
    }
}
