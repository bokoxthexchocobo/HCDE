using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StockKeyDeathmatchSpawnTests
{
    [Theory]
    [InlineData(48, 5)]
    [InlineData(49, 13)]
    [InlineData(50, 6)]
    [InlineData(51, 39)]
    [InlineData(52, 38)]
    [InlineData(53, 40)]
    public void StockKeysAreExcludedOnlyFromDeathmatchAndPatchCanAllowThem(int index, ushort editor)
    {
        var stock = Room(editor, SpawnGameMode.Deathmatch);
        Assert.DoesNotContain(stock.Actors, a => a.DoomEdNum == editor);
        Assert.Contains(Room(editor, SpawnGameMode.Single).Actors, a => a.DoomEdNum == editor);
        Assert.Contains(Room(editor, SpawnGameMode.Cooperative).Actors, a => a.DoomEdNum == editor);
        var patch = DehackedPatch.Apply($"Thing {index}\nBits = SPECIAL\n"); Assert.Empty(patch.Errors);
        var allowed = Room(editor, SpawnGameMode.Deathmatch, patch);
        Assert.Contains(allowed.Actors, a => a.DoomEdNum == editor);
        var saved = SimSavegame.Write(allowed, true);
        allowed.Actors.Single(a => a.MapPickupThingIndex.HasValue).Destroy(); allowed.Tick(); SimSavegame.Apply(allowed, saved);
        Assert.Equal(saved, SimSavegame.Write(allowed, true));
        var fresh = Room(editor, SpawnGameMode.Deathmatch, patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void DeathmatchMapFilterDoesNotSuppressDynamicKeyDrops()
    {
        var sim = Room(5, SpawnGameMode.Deathmatch);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.BlueCard));
        Assert.Single(sim.Actors, a => a.SpawnedDroppedPickup && a.DoomEdNum == PickupCatalog.BlueCard);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(5, SpawnGameMode.Deathmatch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    private static AuthoritySimulation Room(ushort editor, SpawnGameMode mode, DehackedPatchResult? patch = null) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = editor, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch, spawnOptions: new SpawnOptions(Mode: mode));
}
