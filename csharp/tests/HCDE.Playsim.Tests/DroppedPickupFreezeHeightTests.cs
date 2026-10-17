using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedPickupFreezeHeightTests
{
    [Theory]
    [InlineData(64, 2007, 16)]
    [InlineData(64, 2007, 32)]
    [InlineData(64, 2007, 64)]
    [InlineData(78, 2001, 16)]
    [InlineData(78, 2001, 32)]
    [InlineData(78, 2001, 64)]
    public void FreezeRestoresDroppedClassDimensionsLikeMapInventory(int index, ushort editor, int height)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nBits = SPECIAL+SHOOTABLE+COUNTKILL\nHit points = 10\nHeight = {height * 65536}\nWidth = 786432\n");
        Assert.Empty(patch.Errors); var sim = Room(editor, patch);
        var map = sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), editor)); var drop = Drop(sim);
        Assert.Equal(map.ResurrectionHeight, drop.ResurrectionHeight); Assert.Equal(map.ResurrectionRadius, drop.ResurrectionRadius);
        Assert.Equal(Fixed.FromInt(height), drop.ResurrectionHeight); Assert.Equal(Fixed.FromInt(12), drop.ResurrectionRadius);
        ActorDamage.Apply(map, 20, damageType: "Ice"); ActorDamage.Apply(drop, 20, damageType: "Ice");
        Assert.True(map.IceCorpse); Assert.True(drop.IceCorpse);
        Assert.Equal(height, map.Height.ToDouble()); Assert.Equal(height, drop.Height.ToDouble());
        var saved = SimSavegame.Write(sim, true); drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(editor, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(height, Drop(fresh).Height.ToDouble()); Assert.Equal(Fixed.FromInt(height), Drop(fresh).ResurrectionHeight);
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(ushort editor, DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = editor, X = 400 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch, compat: CompatSurface.NoTossDrops);
}
