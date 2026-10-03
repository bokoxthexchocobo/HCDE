using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class RemappedIceDropTests
{
    [Theory]
    [InlineData(2, PickupCatalog.Clip, false)]
    [InlineData(3, PickupCatalog.Shotgun, false)]
    [InlineData(11, PickupCatalog.Chaingun, false)]
    [InlineData(2, PickupCatalog.Clip, true)]
    [InlineData(3, PickupCatalog.Shotgun, true)]
    [InlineData(11, PickupCatalog.Chaingun, true)]
    public void IceShatterUsesOriginalClassDropAfterEditorRemapping(int index, int item, bool dynamic)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 4000\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = dynamic ? [] : [new LevelThing { Type = 4000 }],
        }, dehacked: patch);
        var actor = dynamic ? sim.AddBot(0, 0, 4000) : Assert.Single(sim.Actors);
        Assert.Equal(4000, actor.DoomEdNum);
        sim.SpawnIceChunks(actor);
        Assert.True(actor.Destroyed);
        Assert.Single(sim.Actors, a => a.DoomEdNum == item);
        sim.SpawnIceChunks(actor);
        Assert.Single(sim.Actors, a => a.DoomEdNum == item);
    }
    [Fact]
    public void RemappedImpDoesNotAcquireDropFromRequestedEditorNumber()
    {
        var patch = DehackedPatch.Apply("Thing 12\nID # = 4000\n");
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch);
        var actor = sim.AddBot(0, 0, 4000); sim.SpawnIceChunks(actor);
        Assert.DoesNotContain(sim.Actors, a => PickupCatalog.IsPickup(a.DoomEdNum));
    }
}