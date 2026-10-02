using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedPickupFlagTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, true)]
    [InlineData(2048, true, false)]
    [InlineData(2049, true, true)]
    public void ExplicitBitsImportNativePickupAndSpecialFlags(int bits, bool pickup, bool special)
    {
        var patch = DehackedPatch.Apply($"Thing 1\nBits = {bits}\n");
        Assert.Empty(patch.Errors); Assert.True(patch.Actors[0].BitsPatched);
        var sim = Room(patch); var player = sim.Players.Single();
        Assert.Equal(pickup, player.CanPickupItems); Assert.Equal(special, player.SpecialPickup);
        var item = sim.Actors[^1]; sim.Tick(); Assert.Equal(pickup, item.Destroyed);
    }

    [Fact]
    public void PatchWithoutBitsPreservesPlayerPickupDefault()
    {
        var patch = DehackedPatch.Apply("Thing 1\nHit points = 88\n");
        Assert.False(patch.Actors[0].BitsPatched);
        var sim = Room(patch); Assert.True(sim.Players.Single().CanPickupItems);
        Assert.False(sim.Players.Single().SpecialPickup);
    }

    [Fact]
    public void ExplicitBitsCanDisableRemappedCatalogItemContact()
    {
        var patch = DehackedPatch.Apply("Thing 2\nID # = 2007\nBits = 0\n");
        var sim = Room(patch); var item = sim.Actors[^1];
        Assert.False(item.SpecialPickup); sim.Tick(); Assert.False(item.Destroyed);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Clip }],
    }, dehacked: patch);
}
